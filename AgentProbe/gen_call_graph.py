# -*- coding: utf-8 -*-
"""从源码提取方法调用链（客观、可重算）。

只做两件事，不做任何语义判断：
  1. 定位每个方法定义的起始行（括号配平确定 body 边界）
  2. 在 body 内找出「调用了本类已定义方法」的行号

输出的是「谁在哪一行调用了谁」，读者自己看原文判断顺序与后果。
不写「因此应该怎样」，因为那是解释，不是事实。

用法：
    python gen_call_graph.py --file Player.cs --root Update --depth 2
    python gen_call_graph.py --file Player.cs --root PreUpdate --depth 3 --out xxx.txt
"""
import argparse
import os
import re

SRC_ROOT = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria"
DEFAULT_OUT = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out\call_graph.txt"

MODIFIERS = r"(?:public|private|protected|internal|static|unsafe|override|virtual|abstract|sealed|async|partial|extern|new|readonly)"
# 修饰符们 + 返回类型 + 方法名 + '('，前面不允许是赋值语句
METHOD_HEAD = re.compile(
    rf"^\s*(?:{MODIFIERS}\s+)+(?:[\w<>\[\],\.\?]+(?:\s*\?)?)?\s*(\w+)\s*\(")
CALL_PATTERN = re.compile(r"\b(\w+)\s*\(")
STRING_PATTERN = re.compile(r'"(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)*\'')
SKIP_WORDS = {
    "if", "for", "foreach", "while", "switch", "catch", "using", "lock", "do", "else",
    "return", "new", "throw", "await", "yield", "nameof", "sizeof", "typeof", "checked",
    "unchecked", "when", "base", "this", "get", "set", "add", "remove", "in", "is", "as",
    "operator", "var", "delegate", "ref", "out",
}


def read_text(path):
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8", errors="replace")


def strip_strings(text):
    """去掉字符串/字符字面量，避免其中的 { } 打乱括号配平。"""
    return STRING_PATTERN.sub('""', text)


def find_body_end(lines, head_index):
    """从方法头行开始，找到 body 的结束行（0-based）。表达式体方法返回自身。"""
    text = strip_strings(lines[head_index])
    if "=>" in text and "{" not in text:
        return head_index
    depth = 0
    started = False
    for cursor in range(head_index, len(lines)):
        cleaned = strip_strings(lines[cursor])
        depth += cleaned.count("{") - cleaned.count("}")
        if "{" in cleaned:
            started = True
        if started and depth <= 0:
            return cursor
    return len(lines) - 1


TYPE_PATTERN = re.compile(
    r"^\s*(?:public|internal|private|protected)?\s*"
    r"(?:static\s+|abstract\s+|sealed\s+|partial\s+|unsafe\s+|ref\s+)*"
    r"\b(class|struct|interface|record)\s+(\w+)")


def scan_members(lines):
    """返回 {方法名: [(定义行, body 起, body 止, 所属类型), ...]}，1-based 行号。

    必须记录所属类型：Player.cs 里嵌套类型存在同名方法
    （例如 RabbitOrderFrameHelper.Update 与 Player.Update），
    只按方法名取第一个会指错位置。
    """
    methods = {}
    pending_type = None
    type_stack = []
    depth = 0
    for index, raw in enumerate(lines):
        cleaned = strip_strings(raw)
        type_match = TYPE_PATTERN.match(raw)
        if type_match:
            pending_type = type_match.group(2)

        head_match = METHOD_HEAD.match(raw)
        if head_match and pending_type is None:
            name = head_match.group(1)
            trailing = raw.rstrip()
            if name not in SKIP_WORDS and not (trailing.endswith(";") and "{" not in trailing):
                end = find_body_end(lines, index)
                owning = type_stack[-1][0] if type_stack else ""
                methods.setdefault(name, []).append((index + 1, index + 1, end + 1, owning))

        opens = cleaned.count("{")
        closes = cleaned.count("}")
        if pending_type is not None and opens > 0:
            type_stack.append((pending_type, depth + 1))
            pending_type = None
        depth += opens - closes
        while type_stack and depth < type_stack[-1][1]:
            type_stack.pop()
    return methods


def find_calls(lines, start, end, known_names):
    """在 [start, end] 行范围内找调用了 known_names 中方法的位置。"""
    found = []
    for index in range(start - 1, min(end, len(lines))):
        cleaned = strip_strings(lines[index])
        for match in CALL_PATTERN.finditer(cleaned):
            name = match.group(1)
            if name in SKIP_WORDS or name not in known_names:
                continue
            found.append((index + 1, name))
    return found


def build_tree(lines, methods, known_names, root, max_depth, type_filter=""):
    """返回 [(depth, call_line, name, def_line, is_cycle)] 的线性展开。"""
    rows = []
    visited = set()

    def pick_definition(name):
        defs = methods.get(name) or []
        if not defs:
            return None
        if type_filter:
            for entry in defs:
                if entry[3] == type_filter:
                    return entry
        # 无类型约束时取 body 最长者：真正的实现通常比同名小方法长
        return max(defs, key=lambda entry: entry[2] - entry[1])

    def walk(name, depth, call_line):
        chosen = pick_definition(name)
        if chosen is None:
            rows.append((depth, call_line, name, None, False, ""))
            return
        def_line, body_start, body_end, owning = chosen
        cycle = name in visited
        rows.append((depth, call_line, name, def_line, cycle, owning))
        if cycle or depth >= max_depth:
            return
        visited.add(name)
        for line_no, callee in find_calls(lines, body_start, body_end, known_names):
            if callee == name:
                continue
            walk(callee, depth + 1, line_no)
        visited.discard(name)

    walk(root, 0, None)
    return rows


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--file", required=True, help="源码文件名，如 Player.cs")
    parser.add_argument("--root", required=True, help="入口方法名，如 Update")
    parser.add_argument("--type", default="", help="限定所属类型，如 Player（同文件内有同名方法时必须给）")
    parser.add_argument("--depth", type=int, default=2)
    parser.add_argument("--out", default=DEFAULT_OUT)
    args = parser.parse_args()

    path = os.path.join(SRC_ROOT, args.file)
    lines = read_text(path).replace("\r\n", "\n").split("\n")
    methods = scan_members(lines)
    known_names = set(methods)

    header = [
        f"# 调用链：{args.file} :: {args.type or '*'}::{args.root}",
        f"# 由 gen_call_graph.py 从源码机械提取，只含「定义行」与「调用行」坐标",
        f"# 识别到 {len(known_names)} 个可调用方法",
        "#",
        "# 列含义：缩进=层级 | 方法名 | 所属类型 | 定义于 | 在父方法 body 内的调用行",
        "",
    ]

    if args.root not in methods:
        header.append(f"!! 未找到方法 {args.root}（可能是属性/事件/嵌套类型外的私有实现）")
        print("\n".join(header))
        return

    rows = build_tree(lines, methods, known_names, args.root, args.depth, args.type)
    body = []
    for depth, call_line, name, def_line, cycle, owning in rows:
        location = f"{args.file}:{def_line}" if def_line else "未解析"
        indicator = "  " * depth + ("└─ " if depth else "")
        call_info = f"调用行 {call_line}" if call_line else "根"
        suffix = "  [循环引用，已截断]" if cycle else ""
        body.append(f"{indicator}{name:<34s} {owning:<26s} 定义 {location:<26s} {call_info}{suffix}")

    text = "\n".join(header + body)
    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    with open(args.out, "w", encoding="utf-8") as handle:
        handle.write(text + "\n")

    print(f"out = {args.out}")
    print(f"methods = {len(known_names)}  rows = {len(rows)}")
    print("\n".join(body[:20]))


if __name__ == "__main__":
    main()
