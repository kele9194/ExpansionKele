"""统计 Content 下所有 .cs 的代码风格指标，并按创建月份聚合（只读，输出到同目录）。"""
import csv
import os
from collections import defaultdict
from datetime import datetime
from pathlib import Path

root = Path(__file__).resolve().parent.parent
SKIP = ("AgentProbe", "bin", "obj", ".git", "Tmod_Ref")

rows = []
for p in root.rglob("*.cs"):
    rel = p.relative_to(root).as_posix()
    if rel.split("/")[0] in SKIP:
        continue
    try:
        text = p.read_text(encoding="utf-8-sig", errors="replace")
    except Exception:
        continue
    lines = text.splitlines()
    xml = sum(1 for l in lines if l.strip().startswith("///"))
    cmt = sum(1 for l in lines if l.strip().startswith("//") and not l.strip().startswith("///"))
    existing = text.count("... existing code ...")
    setname = text.count("SetNameOverride")
    han = sum(1 for l in lines if any("\u4e00" <= c <= "\u9fff" for c in l))
    ct = datetime.fromtimestamp(os.path.getctime(p))
    rows.append([rel, ct.strftime("%Y-%m"), ct.strftime("%Y-%m-%d"),
                 len(lines), xml, cmt, existing, setname, han])

rows.sort(key=lambda r: r[2])
with (root / "AgentProbe" / "style-stats.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Path", "Month", "Created", "Lines", "Xml", "LineCmt",
                "ExistingCodeMarks", "SetNameOverride", "HanLines"])
    w.writerows(rows)

# 按创建月份聚合
agg = defaultdict(lambda: {"n": 0, "lines": 0, "xml": 0, "cmt": 0, "ex": 0, "set": 0, "han": 0})
for rel, month, _c, ln, xml, cmt, ex, st, han in rows:
    a = agg[month]
    a["n"] += 1; a["lines"] += ln; a["xml"] += xml; a["cmt"] += cmt
    a["ex"] += 1 if ex else 0; a["set"] += 1 if st else 0; a["han"] += han

with (root / "AgentProbe" / "style-by-month.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Month", "Files", "AvgLines", "AvgXmlPerFile", "AvgLineCmtPerFile",
                "FilesWithExistingMark", "FilesWithSetNameOverride", "AvgHanLinesPerFile"])
    for m in sorted(agg):
        a = agg[m]
        w.writerow([m, a["n"], round(a["lines"] / a["n"], 1), round(a["xml"] / a["n"], 1),
                    round(a["cmt"] / a["n"], 1), a["ex"], a["set"],
                    round(a["han"] / a["n"], 1)])

print("files", len(rows), "months", len(agg))
