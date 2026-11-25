# -*- coding: utf-8 -*-
"""把官方 SoundID 表压成一行一条，并与源码 SoundID.cs 对比。

原始表结构（每条）：
    2 (5)\tItem5\t          <- 类别 (序号) + 内部名
    Item 5                  <- 音效文件（可能多行）
    大多数弓和连弩、蚁狮攻击    <- 中文用途
    ❌                      <- 标记

输出：
  AgentProbe/out/SoundID_fixed.txt    类别 \t 序号 \t 内部名 \t 用途 \t 标记
  AgentProbe/out/SoundID_compare.txt  比对报告
"""
import os
import re

SOUND_PATH = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID\SoundID.cs"
USER_TABLE = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\ID\SoundID表.txt"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

# 源码里的旧式音效定义
LEGACY_PATTERN = re.compile(
    r"public static readonly LegacySoundStyle (\w+) = new LegacySoundStyle\((\d+), (\d+)\)")
HEAD_PATTERN = re.compile(r"^(\d+)(?:\s*\((\d+)\))?(?:\t(.*))?$")


def read_text_auto(path):
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8", errors="replace")


def has_cjk(text):
    return any("\u4e00" <= ch <= "\u9fff" for ch in text)


def parse_user(text):
    """按头行切块，块内提取中文用途与标记。"""
    blocks = []
    current = None
    for line in text.split("\n"):
        match = HEAD_PATTERN.match(line)
        if match and not line.startswith(" "):
            if current is not None:
                blocks.append(current)
            current = {
                "category": int(match.group(1)),
                "index": int(match.group(2)) if match.group(2) else None,
                "name": (match.group(3) or "").strip(),
                "usage": "",
                "mark": "",
            }
        elif current is not None:
            stripped = line.strip()
            if not stripped:
                continue
            if stripped[0] in ("\u2714", "\u274c", "\u2718"):
                current["mark"] = stripped[0]
            elif has_cjk(stripped) and not current["usage"]:
                current["usage"] = stripped
    if current is not None:
        blocks.append(current)
    return blocks


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    source = {}
    with open(SOUND_PATH, encoding="utf-8") as handle:
        for name, category, index in LEGACY_PATTERN.findall(handle.read()):
            source.setdefault((int(category), int(index)), name)

    text = read_text_auto(USER_TABLE).replace("\r\n", "\n").replace("\r", "\n")
    blocks = parse_user(text)

    out_path = os.path.join(OUT_DIR, "SoundID_fixed.txt")
    with open(out_path, "w", encoding="utf-8") as handle:
        handle.write("# 类别 \\t 序号 \\t 内部名 \\t 用途 \\t 标记\n")
        for block in blocks:
            index = "" if block["index"] is None else block["index"]
            handle.write(f"{block['category']}\t{index}\t{block['name']}\t{block['usage']}\t{block['mark']}\n")

    user_keys = {(b["category"], b["index"]) for b in blocks if b["index"] is not None}
    only_source = sorted(set(source) - user_keys)
    only_user = sorted(user_keys - set(source))
    name_mismatch = []
    for block in blocks:
        key = (block["category"], block["index"])
        if block["index"] is not None and key in source and source[key] != block["name"]:
            name_mismatch.append((key, block["name"], source[key]))

    report = []
    report.append(f"user blocks        : {len(blocks)}")
    report.append(f"user keyed (cat,idx): {len(user_keys)}")
    report.append(f"source legacy      : {len(source)}")
    report.append(f"only in source     : {len(only_source)}")
    report.append(f"only in user table : {len(only_user)}")
    report.append(f"name mismatch      : {len(name_mismatch)}")
    report.append(f"no usage text      : {sum(1 for b in blocks if not b['usage'])}")
    report.append("")
    report.append("=== name mismatch (category, index) user vs source ===")
    for (category, index), user_name, src_name in name_mismatch[:60]:
        report.append(f"({category},{index})\tuser={user_name}\tsource={src_name}")
    report.append("")
    report.append("=== only in source ===")
    for category, index in only_source[:120]:
        report.append(f"({category},{index})\t{source[(category, index)]}")
    report.append("")
    report.append("=== only in user table ===")
    for category, index in only_user[:120]:
        report.append(f"({category},{index})")

    report_path = os.path.join(OUT_DIR, "SoundID_compare.txt")
    with open(report_path, "w", encoding="utf-8") as handle:
        handle.write("\n".join(report))

    print(f"out = {out_path}")
    print(f"report = {report_path}")
    print(f"user_blocks={len(blocks)} keyed={len(user_keys)} src={len(source)} "
          f"only_src={len(only_source)} only_user={len(only_user)} mismatch={len(name_mismatch)} "
          f"no_usage={sum(1 for b in blocks if not b['usage'])}")


if __name__ == "__main__":
    main()
