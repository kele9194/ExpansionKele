"""量化星元系列（三条 10 级链 + 套装）的同质化程度与全项目不一致度。"""
import csv
import difflib
import re
from collections import Counter, defaultdict
from pathlib import Path

root = Path(__file__).resolve().parent.parent


def rd(p):
    try:
        return p.read_text(encoding="utf-8-sig", errors="replace")
    except Exception:
        return ""


def ratio(a, b):
    return difflib.SequenceMatcher(None, a, b).ratio()


# ---------- 1. 各"链"的文件间相似度 ----------
chains = {
    "StarySword(A~J)": [f"Content/StaryMelee/StarySword{c}.cs" for c in "ABCDEFGHIJ"],
    "StarStaff(A~J)": [f"Content/StaryMagic/StarStaff{c}.cs" for c in "ABCDEFGHIJ"],
    "GaSniper(A~J)": [f"Content/StarySniper/GaSniper{c}.cs" for c in "ABCDEFGHIJ"],
    "StarHelmet(A~J)": [f"Content/Armor/StarArmorA/StarHelmet{c}.cs" for c in "ABCDEFGHIJ"],
    "StarBreastplate(A~J)": [f"Content/Armor/StarArmorA/StarBreastplate{c}.cs" for c in "ABCDEFGHIJ"],
    "StarLeggings(A~J)": [f"Content/Armor/StarArmorA/StarLeggings{c}.cs" for c in "ABCDEFGHIJ"],
    "FullMoon系列": ["Content/Items/Weapons/Melee/FullMoonSword.cs",
                     "Content/Items/Weapons/Melee/FullMoonShortSword.cs",
                     "Content/Items/Weapons/Melee/FullMoonSpear.cs",
                     "Content/Items/Weapons/Ranged/FullMoonBow.cs",
                     "Content/Items/Weapons/Magic/FullMoonEcho.cs",
                     "Content/Items/Weapons/Summon/FullMoonStaff.cs"],
    "弩三兄弟": ["Content/Items/Weapons/Ranged/WoodenCrossbow.cs",
                 "Content/Items/Weapons/Ranged/ShadowCrossbow.cs",
                 "Content/Items/Weapons/Ranged/BloodCrossbow.cs"],
}
rows = []
for name, files in chains.items():
    texts = [t for t in (rd(root / f) for f in files) if t]
    if len(texts) < 2:
        rows.append([name, len(texts), "", "", ""])
        continue
    adj = [ratio(texts[i], texts[i + 1]) for i in range(len(texts) - 1)]
    pair = [ratio(texts[i], texts[j]) for i in range(len(texts)) for j in range(i + 1, len(texts))]
    rows.append([name, len(texts), round(sum(adj) / len(adj), 3),
                 round(sum(pair) / len(pair), 3), f"{round(min(pair),3)}~{round(max(pair),3)}"])
with (root / "AgentProbe" / "chain-similarity.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Chain", "Files", "AvgAdjacentSim", "AvgPairSim", "PairRange"])
    w.writerows(rows)

# ---------- 2. 全项目：LocalizationCategory 取值分布 / 各目录的坏味道 ----------
lc = Counter()
lc_dirs = defaultdict(set)
grp_stat = defaultdict(lambda: {"files": 0, "lines": 0, "xml": 0, "setname": 0, "existing": 0, "dupiface": 0})
SKIP = {"AgentProbe", "bin", "obj", ".git", "Tmod_Ref"}
for p in root.rglob("*.cs"):
    rel = p.relative_to(root).as_posix()
    if rel.split("/")[0] in SKIP:
        continue
    t = rd(p)
    parts = rel.split("/")
    grp = "/".join(parts[:2]) if parts[0] == "Content" else parts[0]
    lines = t.splitlines()
    s = grp_stat[grp]
    s["files"] += 1
    s["lines"] += len(lines)
    s["xml"] += sum(1 for l in lines if l.strip().startswith("///"))
    s["setname"] += 1 if "SetNameOverride" in t else 0
    s["existing"] += 1 if "... existing code ..." in t else 0
    s["dupiface"] += 1 if re.search(r":\s*\w+Abs\s*,\s*IChargeableItem", t) else 0
    m = re.search(r'LocalizationCategory\s*=>\s*"([^"]+)"', t)
    if m:
        lc[m.group(1)] += 1
        lc_dirs[m.group(1)].add(grp)

with (root / "AgentProbe" / "loc-category.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["LocalizationCategory", "Files", "Dirs"])
    for k, v in lc.most_common():
        w.writerow([k, v, ";".join(sorted(lc_dirs[k]))[:120]])

with (root / "AgentProbe" / "group-metrics.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Group", "Files", "Lines", "XmlLines", "XmlPerFile",
                "FilesWithSetNameOverride", "FilesWithExistingMark", "RedundantIfaceDecl"])
    for g, s in sorted(grp_stat.items(), key=lambda kv: -kv[1]["lines"]):
        w.writerow([g, s["files"], s["lines"], s["xml"], round(s["xml"] / s["files"], 1),
                    s["setname"], s["existing"], s["dupiface"]])

print("ok")
