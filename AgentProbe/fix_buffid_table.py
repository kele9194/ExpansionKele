# -*- coding: utf-8 -*-
"""把 Tmod_Ref 的 BuffID 表重排成「一条一行」，并标注 1.4.5 条目。

原始表结构（每条 4 行，数值行后面还跟一个制表符）：
    1\t
    黑曜石皮
    黑曜石皮
    ObsidianSkin\t增益

标注规则：
  数值 > 354                        -> [1.4.5]（当前源码 BuffID.Count 之外的编号）
  数值 <= 354 但内部名不在源码里     -> [1.4.5]（1.4.5 的重命名，如矿车 Left/Right 合并）

输出：AgentProbe/out/buffID_fixed.txt
"""
import os
import re

ID_PATH = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID\BuffID.cs"
USER_TABLE = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\ID\buffID表.txt"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

# 1.4.4.9 的 BuffID 最大值
VERSION_THRESHOLD = 354

CONST_PATTERN = re.compile(r"public const (?:short|int|byte) (\w+) = (-?\d+);")
NAME_PATTERN = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")
ROW_PATTERN = re.compile(
    r"^(\d+)[ \t]*\n([^\n]*)\n([^\n]*)\n([A-Za-z_][A-Za-z0-9_]*)\t([^\n]*)",
    re.MULTILINE,
)


def read_text_auto(path):
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8", errors="replace")


def parse_rows(text):
    """兼容两种格式：一行一条（tab 分隔）与 4 行一条（网页复制残留）。"""
    rows = []
    for line in text.split("\n"):
        parts = line.split("\t")
        if len(parts) < 3 or not parts[0].strip().isdigit():
            continue
        name = parts[2].strip()
        if not NAME_PATTERN.fullmatch(name):
            continue
        kind = parts[3].strip() if len(parts) > 3 else ""
        rows.append((parts[0].strip(), parts[1], "", name, kind))

    if rows:
        return rows

    return ROW_PATTERN.findall(text)


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    with open(ID_PATH, encoding="utf-8") as handle:
        pairs = CONST_PATTERN.findall(handle.read())
    source = {name: int(value) for name, value in pairs}

    # 源码里成对的 Left/Right 常量：去掉 Left/Right 之后建立索引
    stripped = {}
    for name, value in source.items():
        key = name.replace("Left", "").replace("Right", "")
        if key != name:
            stripped.setdefault(key, []).append((name, value))

    text = read_text_auto(USER_TABLE).replace("\r\n", "\n").replace("\r", "\n")
    rows = parse_rows(text)

    output = []
    direct = 0
    expanded = 0
    missing = 0
    for value, cn_a, cn_b, name, kind in rows:
        display = cn_a.strip() or cn_b.strip()
        kind = kind.strip()

        if name in source:
            src_value = source[name]
            output.append((src_value, f"{src_value}\t{display}\t{name}\t{kind}"))
            direct += 1
        elif name in stripped:
            for src_name, src_value in stripped[name]:
                output.append((src_value, f"{src_value}\t{display}\t{src_name}\t{kind}"))
            expanded += 1
        else:
            number = int(value)
            output.append((number, f"{number}\t{display}\t{name}\t{kind}\t[1.4.5]"))
            missing += 1

    output.sort(key=lambda item: item[0])
    out_path = os.path.join(OUT_DIR, "buffID_fixed.txt")
    with open(out_path, "w", encoding="utf-8") as handle:
        for _, line in output:
            handle.write(line + "\n")

    print(f"output   = {out_path}")
    print(f"rows     = {len(output)}")
    print(f"direct   = {direct}")
    print(f"expanded = {expanded} entries -> {len(output) - direct - missing} lines")
    print(f"marked   = {missing}")


if __name__ == "__main__":
    main()
