# -*- coding: utf-8 -*-
"""合并官方 DustID 表（含血污标记）与源码 DustID 表。

数据源：
  ID/DustID.cs + ID/DustID.TML.cs      -> 值 + 规范常量名
  Tmod_Ref/ID/DustID表.txt             -> 值 + 血污标记（✔ / ✘）

用户表的内部名会出现别名粘连（TintableDustt_Slime），所以名字一律采用源码的规范名。

输出：
  AgentProbe/out/DustID_merged.txt   值 \t 常量名 \t 安全标记
  AgentProbe/out/DustID_compare.txt  比对报告
"""
import os
import re

ID_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID"
USER_TABLE = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\ID\DustID表.txt"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

CONST_PATTERN = re.compile(r"public const (?:short|int|byte) (\w+) = (-?\d+);")
# 用户表：数值行 \n 内部名行 \n 标记行（✔ 或 ✘）
USER_PATTERN = re.compile(r"^(\d+)\t[^\n]*\n([^\n]+)\n([\u2714\u274c\u2718])", re.MULTILINE)


def read_text_auto(path):
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8", errors="replace")


def extract_mark(cell):
    cell = cell.strip()
    if not cell:
        return None
    if cell[0] in "\u2714\u221a":
        return "safe"
    if cell[0] in "\u274c\u2718\u00d7xX":
        return "unsafe"
    return None


def has_cjk(text):
    return any("\u4e00" <= ch <= "\u9fff" for ch in text)


def parse_user(text):
    """按数值行切块，再逐单元格识别：标记 / 中文名。兼容单行与多行两种格式。"""
    blocks = []
    current = None
    for line in text.split("\n"):
        match = re.match(r"^(\d+)\t", line)
        if match:
            if current is not None:
                blocks.append(current)
            current = (int(match.group(1)), [line])
        elif current is not None:
            current[1].append(line)
    if current is not None:
        blocks.append(current)

    result = {}
    for value, block in blocks:
        mark = None
        cn = ""
        for line in block:
            for cell in line.split("\t"):
                stripped = cell.strip()
                if not stripped or ".png" in stripped or stripped == "无":
                    continue
                cell_mark = extract_mark(stripped)
                if cell_mark is not None:
                    mark = cell_mark
                    continue
                if not cn and has_cjk(stripped):
                    cn = stripped
        result.setdefault(value, (cn, mark))
    return result


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    names_by_value = {}
    for file_name in ("DustID.cs", "DustID.TML.cs"):
        path = os.path.join(ID_DIR, file_name)
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as handle:
            for name, value in CONST_PATTERN.findall(handle.read()):
                names_by_value.setdefault(int(value), []).append(name)

    text = read_text_auto(USER_TABLE).replace("\r\n", "\n").replace("\r", "\n")
    parsed = parse_user(text)
    from collections import Counter
    mark_counter = Counter(mark for (_name, mark) in parsed.values())
    print(f"parsed_blocks={len(parsed)} marks={dict(mark_counter)}")
    user_mark = {}
    duplicate_marks = 0
    for value, (_name, mark) in parsed.items():
        if mark is not None:
            user_mark[value] = mark

    only_source = sorted(set(names_by_value) - set(parsed))
    only_user = sorted(set(parsed) - set(names_by_value))

    # 全量输出：源码有的全部保留，官方表没覆盖到的标记为 ?
    merged = []
    for value in sorted(names_by_value):
        cn, mark = parsed.get(value, ("", None))
        tag = "✔" if mark == "safe" else ("✘" if mark == "unsafe" else "?")
        for name in names_by_value[value]:
            merged.append((value, name, cn, tag))

    merged_path = os.path.join(OUT_DIR, "DustID_merged.txt")
    with open(merged_path, "w", encoding="utf-8") as handle:
        handle.write("# 值 \\t 常量名 \\t 中文名 \\t 血污标记（✔ 安全 / ✘ 血腥 / ? 未标记）\n")
        for value, name, cn, tag in merged:
            handle.write(f"{value}\t{name}\t{cn}\t{tag}\n")

    safe_count = sum(1 for _, _, _, t in merged if t == "✔")
    unsafe_count = sum(1 for _, _, _, t in merged if t == "✘")
    unknown_count = len(merged) - safe_count - unsafe_count
    unsafe_values = sorted(v for v in set(user_mark) if user_mark[v] == "unsafe")

    report = []
    report.append(f"user parsed values    : {len(parsed)}  (with mark: {len(user_mark)})")
    report.append(f"source unique values  : {len(names_by_value)}")
    report.append(f"source total names    : {sum(len(v) for v in names_by_value.values())}")
    report.append(f"matched values        : {len(set(names_by_value) & set(user_mark))}")
    report.append(f"only in source        : {len(only_source)}")
    report.append(f"only in user table    : {len(only_user)}")
    report.append(f"merged rows           : {len(merged)}  (safe {safe_count}, unsafe {unsafe_count}, unknown {unknown_count})")
    report.append("")
    report.append("=== unsafe values ===")
    for value in unsafe_values:
        report.append(f"{value}\t{','.join(names_by_value.get(value, ['?']))}")
    report.append("")
    report.append("=== only in source ===")
    for value in only_source:
        report.append(f"{value}\t{','.join(names_by_value[value])}")
    report.append("")
    report.append("=== only in user table ===")
    for value in only_user:
        report.append(f"{value}\t{user_mark[value]}")

    report_path = os.path.join(OUT_DIR, "DustID_compare.txt")
    with open(report_path, "w", encoding="utf-8") as handle:
        handle.write("\n".join(report))

    print(f"merged = {merged_path}")
    print(f"report = {report_path}")
    print(f"user={len(parsed)} src_values={len(names_by_value)} "
          f"matched={len(set(names_by_value) & set(parsed))} "
          f"only_src={len(only_source)} only_user={len(only_user)}")
    print(f"merged_rows={len(merged)} safe={safe_count} unsafe={unsafe_count} unknown={unknown_count}")


if __name__ == "__main__":
    main()
