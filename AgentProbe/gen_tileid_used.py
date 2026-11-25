# -*- coding: utf-8 -*-
"""提取本项目实际用到的 TileID，并从原版中文本地化补中文名。

数据源：
  本项目 *.cs                      -> 用到的 TileID 常量名
  ID/TileID.cs                     -> 值
  Localization/Content/zh-Hans/Items.json -> 中文名（ItemName.<同名>）

输出：AgentProbe/out/TileID_used.txt   值 \t 常量名 \t 中文名
"""
import glob
import json
import os
import re

PROJECT_ROOT = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele"
ID_PATH = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID\TileID.cs"
LOC_PATH = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\Localization\Content\zh-Hans\Items.json"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

USAGE_PATTERN = re.compile(r"\bTileID\.([A-Z]\w+)")
CONST_PATTERN = re.compile(r"public const ushort (\w+) = (\d+);")

# TileID 名字与物品名不一致的情况（中文名从 Items.json 的同名条目取）
TILE_TO_ITEM = {
    "WorkBenches": "WorkBench",
    "Anvils": "IronAnvil",
    "Furnaces": "Furnace",
    "Bottles": "Bottle",
    "Bookcases": "Bookcase",
    "TinkerersWorkbench": "TinkerersWorkshop",
    "Ash": "AshBlock",
    "Sinks": "",
    "Trees": "",
    "Sets": "",
}


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    used = set()
    for path in glob.glob(os.path.join(PROJECT_ROOT, "**", "*.cs"), recursive=True):
        if os.sep + "obj" + os.sep in path or os.sep + "bin" + os.sep in path:
            continue
        try:
            with open(path, encoding="utf-8", errors="replace") as handle:
                used.update(USAGE_PATTERN.findall(handle.read()))
        except OSError:
            continue

    constants = {}
    with open(ID_PATH, encoding="utf-8") as handle:
        for name, value in CONST_PATTERN.findall(handle.read()):
            constants.setdefault(name, int(value))

    with open(LOC_PATH, encoding="utf-8-sig") as handle:
        item_names = json.load(handle).get("ItemName", {})

    rows = []
    for name in sorted(used):
        if name == "Sets":
            continue
        lookup = TILE_TO_ITEM.get(name, name)
        value = constants.get(name)
        cn = item_names.get(lookup, "") if lookup else ""
        rows.append((value if value is not None else -1, name, cn))

    rows.sort(key=lambda item: (item[0], item[1]))
    out_path = os.path.join(OUT_DIR, "TileID_used.txt")
    with open(out_path, "w", encoding="utf-8") as handle:
        handle.write("# 本项目实际用到的 TileID：值 \\t 常量名 \\t 中文名\n")
        for value, name, cn in rows:
            shown = value if value >= 0 else "?"
            handle.write(f"{shown}\t{name}\t{cn}\n")

    missing_cn = [name for _, name, cn in rows if not cn]
    print(f"used = {len(rows)}  missing_cn = {len(missing_cn)}  output = {out_path}")
    if missing_cn:
        print("no-cn: " + ", ".join(missing_cn))


if __name__ == "__main__":
    main()
