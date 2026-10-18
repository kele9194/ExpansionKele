"""看"创作节奏"：按日列新文件 + 按同名前缀看主题跨度。"""
import csv
from collections import defaultdict
from datetime import datetime
from pathlib import Path

root = Path(__file__).resolve().parent.parent
src = root / "AgentProbe" / "file-times-content.csv"

by_day = defaultdict(list)
by_prefix = defaultdict(list)
for r in csv.DictReader(src.open(encoding="utf-8-sig")):
    name = r["FullName"].split("\\")[-1][:-3]      # 去掉 .cs
    ct = r["CreationTime"]
    d = ct.split(" ")[0]
    if not (name.startswith(("Star", "FullMoon", "Moon", "Chromium", "Stary", "GaSniper",
                             "Shroomite", "Spectre", "Solar", "PearlWood", "Enhanced",
                             "Chips", "Arknights", "Blocking"))):
        continue
    by_day[d].append(name)
    # 前缀取前 7 个字符作为"主题"
    by_prefix[name[:7]].append((ct, name))

with (root / "AgentProbe" / "theme-days.txt").open("w", encoding="utf-8-sig") as f:
    f.write("=== 单日创建 >=3 个主题文件的日期 ===\n")
    for d in sorted(by_day):
        if len(by_day[d]) >= 3:
            f.write(f"\n{d}  ({len(by_day[d])} 个)\n")
            for n in sorted(by_day[d]):
                f.write(f"    {n}\n")

with (root / "AgentProbe" / "theme-prefix.txt").open("w", encoding="utf-8-sig") as f:
    f.write("=== 同一主题前缀的文件数与创建跨度（>=3 个）===\n")
    for pre, items in sorted(by_prefix.items(), key=lambda kv: -len(kv[1])):
        if len(items) < 3:
            continue
        items.sort()
        first, last = items[0][0], items[-1][0]
        span = (datetime.strptime(last.split(" ")[0], "%Y/%m/%d")
                - datetime.strptime(first.split(" ")[0], "%Y/%m/%d")).days
        f.write(f"\n[{pre}*]  {len(items)} 个   {first.split(' ')[0]} ~ {last.split(' ')[0]}  (跨度 {span} 天)\n")
        for ct, n in items:
            f.write(f"    {ct}  {n}\n")

print("ok")
