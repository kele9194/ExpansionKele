"""方舟内容拆分盘点：文件清单 + 对外依赖 + 被外部依赖。"""
import re
from collections import Counter
from pathlib import Path

root = Path(__file__).resolve().parent.parent
SKIP = {"AgentProbe", "bin", "obj", ".git", "Tmod_Ref"}


def rd(p):
    try:
        return p.read_text(encoding="utf-8-sig", errors="replace")
    except Exception:
        return ""


# ---------- A. 工具库（Content/Customs）里的公共类名 ----------
tools = {}
for p in (root / "Content/Customs").rglob("*.cs"):
    t = rd(p)
    for m in re.finditer(r'(?:public|internal)\s+(?:static\s+|abstract\s+|sealed\s+|partial\s+)*'
                         r'(?:class|interface|struct|enum)\s+(\w+)', t):
        tools[m.group(1)] = p.relative_to(root).as_posix()

# ---------- B. 方舟目录 ----------
ark_dir = root / "Content/Items/ArknightsItem"
ark_files = sorted(ark_dir.rglob("*.cs"))
ark_lines = sum(len(rd(p).splitlines()) for p in ark_files)

used = Counter()
for p in ark_files:
    t = rd(p)
    for name in tools:
        if re.search(rf"\b{name}\b", t):
            used[name] += 1

# ---------- C. 外部对 ArknightsItem 的引用 ----------
reverse = []
for p in root.rglob("*.cs"):
    rel = p.relative_to(root).as_posix()
    if rel.split("/")[0] in SKIP or "ArknightsItem" in rel:
        continue
    if "ArknightsItem" in rd(p):
        reverse.append(rel)

# ---------- D. "疑似方舟专属"的公共文件：只被方舟目录引用的 ----------
candidates = ["BlockingSystem", "BlockingPlayer", "BlockingMarkerNPC", "NPCProjectileSourceTracker",
              "FighterBuff", "LiberatorBuff", "DreadnoughtRebirthBuff", "MushaRevivalCooldown",
              "MarksmanBuff", "ArknightsHomingPlayer", "ECShieldSystem"]
exclusive = {}
for name in candidates:
    inside, outside = 0, []
    for p in root.rglob("*.cs"):
        rel = p.relative_to(root).as_posix()
        if rel.split("/")[0] in SKIP:
            continue
        if not re.search(rf"\b{name}\b", rd(p)):
            continue
        if "ArknightsItem" in rel or "Arknights" in rd(p) or "Blocking" in rel:
            inside += 1
        else:
            outside.append(rel)
    exclusive[name] = (inside, outside)

with (root / "AgentProbe" / "ark-split-report.txt").open("w", encoding="utf-8-sig") as f:
    f.write(f"=== 方舟目录规模 ===\n文件 {len(ark_files)} 个 / {ark_lines} 行\n")
    f.write("\n=== 方舟目录用到的工具库类（Content/Customs）===\n")
    for n, c in used.most_common():
        f.write(f"  {c:>3} 处   {n}   ({tools[n]})\n")
    f.write("\n=== 外部（非方舟）引用 ArknightsItem 的文件 ===\n")
    for r in reverse:
        f.write(f"  {r}\n")
    f.write("\n=== 疑似方舟专属的公共文件（潜在搬运对象）===\n")
    for n, (ins, outs) in exclusive.items():
        f.write(f"\n[{n}]  方舟内引用 {ins} 处\n")
        for o in outs[:10]:
            f.write(f"      外部: {o}\n")

print("ark files", len(ark_files), "lines", ark_lines, "reverse", len(reverse))
