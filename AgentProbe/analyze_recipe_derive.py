"""检查"配方→价值/稀有度"自动推导的薄弱点：哪些物品有多个配方（只算第一个）。"""
import re
from pathlib import Path

root = Path(__file__).resolve().parent.parent
rows = []
for p in root.rglob("*.cs"):
    rel = p.relative_to(root).as_posix()
    if rel.split("/")[0] in {"AgentProbe", "bin", "obj", ".git", "Tmod_Ref"}:
        continue
    t = p.read_text(encoding="utf-8-sig", errors="replace")
    n = len(re.findall(r"CreateRecipe\(\)|Recipe\.Create\(", t))
    if n >= 2:
        rows.append((n, rel))
    # 同时找"手写 rare"的漏网之鱼
    hand = re.findall(r"Item\.rare\s*=\s*(ItemRarityID\.\w+|\d+)", t)
    if hand:
        rows.append((-1, rel + "   [手写rare: " + ",".join(hand) + "]"))

rows.sort()
with (root / "AgentProbe" / "recipe-derive-check.txt").open("w", encoding="utf-8-sig") as f:
    f.write("=== 多配方物品（价值/稀有度只按第一个配方算）===\n")
    for n, rel in sorted([r for r in rows if r[0] > 0], reverse=True):
        f.write(f"{n} 个配方  {rel}\n")
    f.write("\n=== 手写 Item.rare（绕过自动推导）===\n")
    for n, rel in [r for r in rows if r[0] == -1]:
        f.write(f"{rel}\n")

multi = [r for r in rows if r[0] > 0]
hand = [r for r in rows if r[0] == -1]
print("multi-recipe files:", len(multi), " hand-written rare:", len(hand))
