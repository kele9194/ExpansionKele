# -*- coding: utf-8 -*-
"""提取 Terraria ID 类里的 Sets 成员，带 XML 注释摘要。

与只有「名字 + 类型」的旧索引不同，这里把源码里的 <summary> 一并抽出来——
那是「这个 Set 到底管什么」唯一的一手来源，比 wiki 准。

输出：Tmod_Ref/ID/Sets索引.txt
列：所属类 \t Set 名 \t 类型 \t 摘要 \t 文件:行号
"""
import os
import re

ID_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID"
OUT_PATH = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\API\Sets索引.txt"

SETS_CLASS = re.compile(r"^\s*(?:public\s+|internal\s+)?(?:partial\s+|static\s+)*class\s+Sets\b")
MEMBER = re.compile(
    r"^\s*public\s+static\s+(?:readonly\s+)?"
    r"((?:[\w<>\[\],\.\?]+)(?:\s*,\s*[\w<>\[\],\.\?]+)*)\s+(\w+)\s*[=;]")
DOC_LINE = re.compile(r"^\s*///\s?(.*)$")
STRING_PATTERN = re.compile(r'"(?:[^"\\]|\\.)*"')


def read_lines(path):
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8", errors="replace")


def strip_tags(text):
    """把 XML 文档注释清洗成纯文本。"""
    text = re.sub(r'<see\s+cref="([^"]+)"\s*/>', r"\1", text)
    text = re.sub(r'<see\s+langword="([^"]+)"\s*/>', r"\1", text)
    text = re.sub(r'<paramref\s+name="([^"]+)"\s*/>', r"\1", text)
    text = re.sub(r"<c>(.*?)</c>", r"\1", text)
    text = re.sub(r"<inheritdoc[^>]*/>", "", text)
    text = re.sub(r"<(br|para)\s*/?>", " ", text)
    text = re.sub(r"<[^>]+>", "", text)
    return re.sub(r"\s+", " ", text).strip()


def summarize(lines, index):
    """向上收集 /// 注释，取 <summary> 首句。"""
    doc = []
    cursor = index - 1
    while cursor >= 0:
        match = DOC_LINE.match(lines[cursor])
        if match:
            doc.insert(0, match.group(1).strip())
            cursor -= 1
            continue
        if lines[cursor].strip() == "" and doc:
            cursor -= 1
            continue
        break
    if not doc:
        return ""
    joined = " ".join(doc)
    match = re.search(r"<summary>(.*?)</summary>", joined, re.S)
    body = match.group(1) if match else joined
    body = strip_tags(body)
    # 只取第一句，避免表过长
    first = re.split(r"(?<=[.!?])\s+", body)[0] if body else ""
    return (first or body)[:180]


def find_sets_blocks(lines):
    """返回 [(起始行, 结束行)]（0-based，闭区间）。"""
    blocks = []
    index = 0
    while index < len(lines):
        if SETS_CLASS.match(lines[index]):
            depth = 0
            started = False
            cursor = index
            while cursor < len(lines):
                cleaned = STRING_PATTERN.sub('""', lines[cursor])
                depth += cleaned.count("{") - cleaned.count("}")
                if "{" in cleaned:
                    started = True
                if started and depth <= 0:
                    break
                cursor += 1
            blocks.append((index, cursor))
            index = cursor + 1
            continue
        index += 1
    return blocks


def main():
    rows = []
    for filename in sorted(os.listdir(ID_DIR)):
        if not filename.endswith(".cs"):
            continue
        lines = read_lines(os.path.join(ID_DIR, filename)).replace("\r\n", "\n").split("\n")
        owner = filename[:-3]
        if owner.endswith(".TML"):
            owner = owner[:-4]
        for block_start, block_end in find_sets_blocks(lines):
            for index in range(block_start, block_end + 1):
                match = MEMBER.match(lines[index])
                if not match:
                    continue
                member_type = re.sub(r"\s+", " ", match.group(1)).strip()
                name = match.group(2)
                summary = summarize(lines, index)
                rows.append({
                    "owner": owner,
                    "name": name,
                    "type": member_type,
                    "summary": summary,
                    "location": f"{filename}:{index + 1}",
                })

    rows.sort(key=lambda row: (row["owner"], row["name"]))
    counts = {}
    for row in rows:
        counts[row["owner"]] = counts.get(row["owner"], 0) + 1

    os.makedirs(os.path.dirname(OUT_PATH), exist_ok=True)
    with open(OUT_PATH, "w", encoding="utf-8") as handle:
        handle.write("# Terraria ID 类 Sets 成员索引（含源码 XML 注释摘要）\n")
        handle.write("# 列：所属类 \\t Set 名 \\t 类型 \\t 摘要 \\t 文件:行号\n")
        handle.write("# 摘要取自源码 /// <summary>，是「这个 Set 管什么」的一手来源\n")
        handle.write(f"# 合计 {len(rows)} 项：" + "、".join(f"{k} {v}" for k, v in sorted(counts.items())) + "\n\n")
        current = None
        for row in rows:
            if row["owner"] != current:
                current = row["owner"]
                handle.write(f"\n## {current}  ({counts[current]})\n")
            handle.write(f"{row['name']}\t{row['type']}\t{row['summary']}\t{row['location']}\n")

    print(f"out = {OUT_PATH}")
    print(f"total = {len(rows)}")
    for owner, count in sorted(counts.items()):
        print(f"  {owner:14s} {count}")


if __name__ == "__main__":
    main()
