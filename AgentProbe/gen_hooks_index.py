# -*- coding: utf-8 -*-
"""扫描 tModLoader 框架的 public/protected virtual|abstract 钩子，生成签名速查表。

只收「框架提供给模组重写」的方法：virtual / abstract。
不收 override（那是框架自己的实现）、不收普通方法（那是可调用的 API，不是钩子）。

输出：AgentProbe/out/hooks_index.txt
    # 类名.方法名(参数) \t 文件:行号
    ModItem.CanUseItem(Player player)	...\ModItem.cs:412
"""
import os
import re
from collections import defaultdict

MODLOADER_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ModLoader"
OUT_PATH = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\API\ModLoader钩子索引.txt"

CLASS_PATTERN = re.compile(r"^\s*(?:public|internal|protected)?\s*(?:static\s+|abstract\s+|sealed\s+|partial\s+)*"
                           r"class\s+(\w+)")
HOOK_PATTERN = re.compile(
    r"^\s*(?:public|protected|internal)\s+"
    r"(?:static\s+|unsafe\s+|sealed\s+|new\s+)*"
    r"(?:virtual|abstract)\s+"
    r"(?:new\s+|sealed\s+)*"
    r"([\w<>\[\],\.\?\s]+?)\s+(\w+)\s*(?:<[^>]*>)?\s*\((.*)$"
)


def read_lines(path):
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16").split("\n")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig").split("\n")
    return raw.decode("utf-8", errors="replace").split("\n")


def collect_signature(lines, start):
    """从 start 行开始，一直读到括号配平为止，拼成单行参数串。"""
    text = lines[start].strip()
    depth = text.count("(") - text.count(")")
    cursor = start + 1
    while depth > 0 and cursor < len(lines):
        piece = lines[cursor].strip()
        text += " " + piece
        depth += piece.count("(") - piece.count(")")
        cursor += 1
    match = re.search(r"\((.*)\)", text, re.S)
    params = match.group(1) if match else ""
    params = re.sub(r"\s+", " ", params).strip()
    return params


def main():
    entries = defaultdict(list)
    for filename in sorted(os.listdir(MODLOADER_DIR)):
        if not filename.endswith(".cs"):
            continue
        path = os.path.join(MODLOADER_DIR, filename)
        lines = read_lines(path)
        current_class = ""
        for number, line in enumerate(lines, start=1):
            class_match = CLASS_PATTERN.match(line)
            if class_match:
                current_class = class_match.group(1)
                continue
            if current_class not in ("ModItem", "ModProjectile", "ModNPC", "ModPlayer", "ModSystem",
                                     "ModBuff", "ModTile", "ModWall", "ModDust", "ModPrefix",
                                     "ModMount", "ModSceneEffect", "ModAccessorySlot",
                                     "ModGlobalItem", "GlobalItem", "GlobalNPC", "GlobalProjectile",
                                     "GlobalTile", "GlobalWall", "GlobalDust", "GlobalBuff",
                                     "GlobalProjectile", "ModBossBar", "ModBiome", "ModCommand"):
                continue
            hook_match = HOOK_PATTERN.match(line)
            if not hook_match:
                continue
            return_type = hook_match.group(1).strip()
            if return_type in ("class", "struct", "enum", "interface", "delegate"):
                continue
            name = hook_match.group(2)
            params = collect_signature(lines, number - 1)
            entries[current_class].append(
                (f"{current_class}.{name}({params})", f"{filename}:{number}", return_type))

    total = sum(len(value) for value in entries.values())
    with open(OUT_PATH, "w", encoding="utf-8") as handle:
        handle.write("# 类名.方法签名 \\t 文件:行号 \\t 返回类型\n")
        for class_name in sorted(entries):
            handle.write(f"\n## {class_name}  ({len(entries[class_name])})\n")
            for signature, location, return_type in entries[class_name]:
                handle.write(f"{signature}\t{location}\t{return_type}\n")

    print(f"out = {OUT_PATH}")
    print(f"total hooks = {total}")
    for class_name, value in sorted(entries.items(), key=lambda item: -len(item[1])):
        print(f"  {class_name:22s} {len(value)}")


if __name__ == "__main__":
    main()
