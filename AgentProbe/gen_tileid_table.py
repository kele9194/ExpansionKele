# -*- coding: utf-8 -*-
"""生成 TileID 表与 TileID.Sets 索引。

数据源：ID/TileID.cs + ID/TileID.TML.cs
输出：
  AgentProbe/out/TileID_table.txt        值 \t 常量名
  AgentProbe/out/TileIDSets_index.txt    Set 名 \t 类型 \t 文件:行号
"""
import os
import re

ID_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

CONST_PATTERN = re.compile(r"public const ushort (\w+) = (\d+);")
SETS_PATTERN = re.compile(r"public static (bool|int|short|byte)\[\] (\w+)")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    constants = {}
    sets = []
    for file_name in ("TileID.cs", "TileID.TML.cs"):
        path = os.path.join(ID_DIR, file_name)
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as handle:
            for line_no, line in enumerate(handle, 1):
                for name, value in CONST_PATTERN.findall(line):
                    constants.setdefault(name, int(value))
                match = SETS_PATTERN.search(line)
                if match:
                    sets.append((match.group(2), match.group(1), file_name, line_no))

    table_path = os.path.join(OUT_DIR, "TileID_table.txt")
    with open(table_path, "w", encoding="utf-8") as handle:
        for name in sorted(constants, key=lambda n: (constants[n], n)):
            handle.write(f"{constants[name]}\t{name}\n")

    sets_path = os.path.join(OUT_DIR, "TileIDSets_index.txt")
    with open(sets_path, "w", encoding="utf-8") as handle:
        handle.write("# TileID.Sets 成员：Set 名 \t 类型 \t 文件:行号\n")
        for name, kind, file_name, line_no in sorted(sets):
            handle.write(f"{name}\t{kind}\t{file_name}:{line_no}\n")

    values = sorted(constants.values())
    print(f"tiles = {len(constants)}  range = {values[0]}..{values[-1]}")
    print(f"sets  = {len(sets)}")
    print(f"table = {table_path}")
    print(f"sets  = {sets_path}")


if __name__ == "__main__":
    main()
