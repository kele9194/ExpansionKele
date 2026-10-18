"""解析 git-numstat.txt，生成武器改动史统计（只读输入，输出到同目录）。"""
import csv
from pathlib import Path

root = Path(__file__).resolve().parent.parent
src = root / "AgentProbe" / "git-numstat.txt"
text = src.read_text(encoding="utf-8-sig", errors="replace")

cur = None
data = {}
for line in text.splitlines():
    if line.startswith("C|"):
        _, date, h, subj = line.split("|", 3)
        cur = (date, h, subj)
    elif not line.strip():
        continue
    else:
        parts = line.split("\t")
        if len(parts) != 3:
            continue
        a, d, p = parts
        data.setdefault(p, []).append((cur[0], cur[2], a, d))

pref = ("Content/Items/Weapons/", "Content/StaryMelee/", "Content/StaryMagic/",
        "Content/StarySniper/", "Content/StarySummon/")


def num(x):
    return int(x) if x.isdigit() else 0


rows = []
for p, lst in data.items():          # lst: 新的在前
    if not (p.startswith(pref) and p.endswith(".cs")):
        continue
    commits = len(lst)
    tot_a = sum(num(a) for _, _, a, _ in lst)
    tot_d = sum(num(d) for _, _, _, d in lst)
    newest = lst[0]
    oldest = lst[-1]
    biggest = max(lst, key=lambda t: num(t[2]) + num(t[3]))
    rows.append([p, commits, tot_a, tot_d,
                 oldest[0], newest[0], newest[1],
                 biggest[0], biggest[1], num(biggest[2]), num(biggest[3])])

rows.sort(key=lambda r: (-r[1], r[0]))
header = ["Path", "Commits", "TotalAdded", "TotalDeleted", "FirstSeen",
          "LastTouch", "LastSubject", "BiggestDate", "BiggestSubject", "BigAdd", "BigDel"]

with (root / "AgentProbe" / "weapon-churn.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(header)
    w.writerows(rows)

hot = [r for r in rows if r[1] >= 3]
with (root / "AgentProbe" / "weapon-churn-hot.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(header)
    w.writerows(hot)

print("total", len(rows), "hot(>=3)", len(hot))
