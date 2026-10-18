"""统计"人工粘贴误伤"的规模：
1) `// ... existing code ...` 占位符数量
2) 顶格的成员声明（缩进为 0 的 public/private/protected/internal 方法签名）——正常 C# 不会这样
"""
import csv
import re
from datetime import datetime
from pathlib import Path

root = Path(__file__).resolve().parent.parent
SKIP = {"AgentProbe", "bin", "obj", ".git", "Tmod_Ref"}

top_decl = re.compile(r'^(public|private|protected|internal)\s+(?!class\b|interface\b|enum\b|struct\b|namespace\b|record\b)')
has_paren = re.compile(r'\([^)]*\)')

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
    marks = text.count("... existing code ...")
    stray = 0
    for l in lines:
        if not l or l[0] in " \t/}":          # 必须有缩进才正常；顶格跳过注释/花括号
            continue
        if top_decl.match(l) and has_paren.search(l):
            stray += 1
    ct = datetime.fromtimestamp(p.stat().st_ctime).strftime("%Y-%m-%d")
    if marks or stray:
        rows.append([rel, ct, marks, stray, len(lines)])

rows.sort(key=lambda r: r[1])
with (root / "AgentProbe" / "paste-damage.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Path", "Created", "ExistingCodeMarks", "StrayTopLevelDecls", "Lines"])
    w.writerows(rows)

print("files", len(rows),
      "marks", sum(r[2] for r in rows),
      "stray", sum(r[3] for r in rows))
