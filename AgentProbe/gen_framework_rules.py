# -*- coding: utf-8 -*-
"""扫描框架里的 throw 语句，把「框架会拒绝什么」变成可核对的清单。

只抄源码原文：触发条件（if 判断）与异常消息都原样保留，附 文件:行号。
不写任何「所以你应该怎样」，那是解释，不是事实。

输出：AgentProbe/out/framework_rules.txt
"""
import os
import re

MODLOADER_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ModLoader"
OUT_PATH = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\API\框架校验规则.txt"

MODIFIERS = r"(?:public|private|protected|internal|static|unsafe|override|virtual|abstract|sealed|async|partial|extern|new)"
METHOD_HEAD = re.compile(rf"^\s*(?:{MODIFIERS}\s+)+(?:[\w<>\[\],\.\?]+(?:\s*\?)?)?\s*(\w+)\s*\(")
IF_COND = re.compile(r"^\s*(?:if|else if)\s*\((.*)\)\s*$")
THROW_NEW = re.compile(r"throw\s+new\s+([\w\.]+)\s*\(")
# 只关心「加载期就炸」的校验文件
PRIORITY_FILES = {
    "GlobalType.cs", "ModType.cs", "ModContent.cs", "Mod.cs", "ModLoader.cs",
    "ItemLoader.cs", "NPCLoader.cs", "ProjectileLoader.cs", "PlayerLoader.cs",
    "TileLoader.cs", "WallLoader.cs", "BuffLoader.cs", "DustLoader.cs",
    "MountLoader.cs", "PrefixLoader.cs",
}


def read_text(path):
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8", errors="replace")


def find_method(lines, index, limit=200):
    for back in range(index - 1, max(index - limit, -1), -1):
        match = METHOD_HEAD.match(lines[back])
        if match:
            return match.group(1), back + 1
    return "", 0


def find_condition(lines, index, limit=10):
    for back in range(index - 1, max(index - limit, -1), -1):
        match = IF_COND.match(lines[back])
        if match:
            return re.sub(r"\s+", " ", match.group(1)).strip(), back + 1
    return "", 0


def grab_statement(lines, index):
    cursor = index
    while cursor < len(lines) and ";" not in lines[cursor] and cursor - index < 4:
        cursor += 1
    return re.sub(r"\s+", " ", " ".join(line.strip() for line in lines[index:cursor + 1])).strip()


def main():
    results = {}
    for filename in sorted(os.listdir(MODLOADER_DIR)):
        if not filename.endswith(".cs"):
            continue
        lines = read_text(os.path.join(MODLOADER_DIR, filename)).replace("\r\n", "\n").split("\n")
        entries = []
        for index, line in enumerate(lines):
            if "throw new" not in line:
                continue
            match = THROW_NEW.search(line)
            if not match:
                continue
            condition, condition_line = find_condition(lines, index)
            method, method_line = find_method(lines, index)
            entries.append({
                "method": method,
                "method_line": method_line,
                "condition": condition,
                "condition_line": condition_line,
                "exception": match.group(1),
                "statement": grab_statement(lines, index)[:300],
                "line": index + 1,
            })
        if entries:
            results[filename] = entries

    total = sum(len(value) for value in results.values())
    ordered = sorted(results, key=lambda name: (name not in PRIORITY_FILES, -len(results[name])))

    with open(OUT_PATH, "w", encoding="utf-8") as handle:
        handle.write("# 框架校验规则（源码 throw 语句原文）\n")
        handle.write(f"# 来源：{MODLOADER_DIR}\n")
        handle.write(f"# 共 {total} 条 throw，分布 {len(results)} 个文件\n")
        handle.write("# 每条只给：所在方法 / 触发条件 / 抛出语句 / 行号，读者自行读源码判断\n\n")
        for filename in ordered:
            entries = results[filename]
            handle.write(f"\n## {filename}  ({len(entries)} 条)\n")
            for entry in entries:
                where = f"  (方法 {entry['method']} @ {filename}:{entry['method_line']})" if entry["method"] else ""
                handle.write(f"\n### {filename}:{entry['line']}{where}\n")
                if entry["condition"]:
                    handle.write(f"    条件  {filename}:{entry['condition_line']}  if ({entry['condition']})\n")
                else:
                    handle.write("    条件  （未找到单行 if，可能无条件抛出或在 catch 内）\n")
                handle.write(f"    抛出  {entry['exception']}\n")
                handle.write(f"    原文  {entry['statement']}\n")

    print(f"out = {OUT_PATH}")
    print(f"total throws = {total}  files = {len(results)}")
    for filename in ordered[:12]:
        print(f"  {filename:22s} {len(results[filename])}")


if __name__ == "__main__":
    main()
