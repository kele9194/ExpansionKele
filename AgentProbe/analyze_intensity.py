"""统计开发强度：按日/月新建 .cs 数、行数、以及"动手时刻"的分布。"""
import csv
import os
from collections import Counter, defaultdict
from datetime import datetime
from pathlib import Path

root = Path(__file__).resolve().parent.parent
SKIP = {"AgentProbe", "bin", "obj", ".git", "Tmod_Ref"}

by_day = Counter()
by_month = Counter()
by_month_lines = defaultdict(int)
by_month_lw = Counter()
hour_ct = Counter()
hour_lw = Counter()
total = 0

for p in root.rglob("*.cs"):
    rel = p.relative_to(root).as_posix()
    if rel.split("/")[0] in SKIP:
        continue
    try:
        st = p.stat()
        n = len(p.read_text(encoding="utf-8-sig", errors="replace").splitlines())
    except Exception:
        continue
    ct = datetime.fromtimestamp(st.st_ctime)
    lw = datetime.fromtimestamp(st.st_mtime)
    by_day[ct.strftime("%Y-%m-%d")] += 1
    by_month[ct.strftime("%Y-%m")] += 1
    by_month_lines[ct.strftime("%Y-%m")] += n
    by_month_lw[lw.strftime("%Y-%m")] += 1
    hour_ct[ct.hour] += 1
    hour_lw[lw.hour] += 1
    total += 1

with (root / "AgentProbe" / "intensity-days.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Date", "NewCsFiles"])
    for d, c in by_day.most_common(25):
        w.writerow([d, c])

with (root / "AgentProbe" / "intensity-months.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Month", "NewFiles", "NewLines", "FilesLastEditedInMonth"])
    for m in sorted(by_month):
        w.writerow([m, by_month[m], by_month_lines[m], by_month_lw.get(m, 0)])

with (root / "AgentProbe" / "intensity-hours.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Hour", "NewedAt", "LastEditedAt"])
    for h in range(24):
        w.writerow([h, hour_ct.get(h, 0), hour_lw.get(h, 0)])

print("files", total)
