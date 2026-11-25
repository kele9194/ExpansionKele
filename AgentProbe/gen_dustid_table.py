# -*- coding: utf-8 -*-
"""从本地 TML 源码生成 DustID 表。

数据源：ID/DustID.cs + ID/DustID.TML.cs
输出：AgentProbe/out/DustID_table.txt   值 \t 常量名
"""
import os
import re

ID_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

CONST_PATTERN = re.compile(r"public const (?:short|int|byte) (\w+) = (-?\d+);")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    entries = {}
    for file_name in ("DustID.cs", "DustID.TML.cs"):
        path = os.path.join(ID_DIR, file_name)
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as handle:
            for name, value in CONST_PATTERN.findall(handle.read()):
                entries.setdefault(name, int(value))

    rows = sorted(entries.items(), key=lambda item: (item[1], item[0]))
    out_path = os.path.join(OUT_DIR, "DustID_table.txt")
    with open(out_path, "w", encoding="utf-8") as handle:
        for name, value in rows:
            handle.write(f"{value}\t{name}\n")

    values = [value for _, value in rows]
    span = max(values) - min(values) + 1 if values else 0
    print(f"output = {out_path}")
    print(f"entries = {len(rows)}  min = {min(values)}  max = {max(values)}  "
          f"gaps = {span - len(set(values))}")


if __name__ == "__main__":
    main()
