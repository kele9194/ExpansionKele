"""量化武器机制复杂度：每个武器类覆写了哪些钩子、配了多少自定义弹幕/ModPlayer。"""
import csv
import re
from collections import Counter, defaultdict
from pathlib import Path

root = Path(__file__).resolve().parent.parent


def rd(p):
    try:
        return p.read_text(encoding="utf-8-sig", errors="replace")
    except Exception:
        return ""


HOOKS = ["Shoot", "OnHitNPC", "ModifyHitNPC", "AltFunctionUse", "CanUseItem", "UseItem",
         "HoldItem", "UpdateInventory", "ModifyWeaponDamage", "ModifyWeaponCrit",
         "ModifyShootStats", "ModifyTooltips", "UseSpeedMultiplier", "MeleePrefix",
         "PreDrawInWorld", "PreDrawInInventory", "DrawHeldProj"]
BASE = {"SetDefaults", "SetStaticDefaults", "AddRecipes"}

weapon_dirs = ["Content/Items/Weapons", "Content/StaryMelee", "Content/StaryMagic",
               "Content/StarySniper", "Content/StarySummon"]

hook_ct = Counter()
rows = []
for d in weapon_dirs:
    for p in (root / d).rglob("*.cs"):
        rel = p.relative_to(root).as_posix()
        t = rd(p)
        if "ModItem" not in t:
            continue
        found = [h for h in HOOKS if re.search(rf"\boverride\b[^;\n]{{0,80}}\b{h}\s*\(", t)]
        for h in found:
            hook_ct[h] += 1
        rows.append([rel, len(t.splitlines()), ";".join(found),
                     len(found),
                     "Y" if re.search(r"class\s+\w+\s*:\s*ModPlayer", t) else "",
                     len(re.findall(r"Projectile\.NewProjectile", t)),
                     "Y" if "IChargeableItem" in t else "",
                     "Y" if "ModContent.BuffType" in t or "AddBuff" in t else ""])

rows.sort(key=lambda r: -r[3])
with (root / "AgentProbe" / "weapon-complexity.csv").open("w", encoding="utf-8-sig", newline="") as f:
    w = csv.writer(f)
    w.writerow(["Path", "Lines", "Hooks", "HookCount", "HasModPlayer",
                "NewProjectileCalls", "Chargeable", "UsesBuff"])
    w.writerows(rows)

# 弹幕侧
proj_files = list((root / "Content/Projectiles").rglob("*.cs"))
proj_lines = sum(len(rd(p).splitlines()) for p in proj_files)

with (root / "AgentProbe" / "weapon-complexity-summary.txt").open("w", encoding="utf-8-sig") as f:
    n = len(rows)
    avg = sum(r[3] for r in rows) / n
    f.write(f"武器类文件: {n}\n")
    f.write(f"平均覆写机制钩子(不含 SetDefaults/SetStaticDefaults/AddRecipes): {avg:.2f}\n")
    f.write(f"其中覆写 >=3 个机制钩子的: {sum(1 for r in rows if r[3] >= 3)}\n")
    f.write(f"覆写 >=5 个的: {sum(1 for r in rows if r[3] >= 5)}\n")
    f.write(f"带配套 ModPlayer 的: {sum(1 for r in rows if r[4])}\n")
    f.write(f"实现 IChargeableItem（蓄力条）的: {sum(1 for r in rows if r[6])}\n")
    f.write(f"含 AddBuff 的: {sum(1 for r in rows if r[7])}\n")
    f.write(f"自定义弹幕文件: {len(proj_files)} 个 / {proj_lines} 行\n")
    f.write("\n钩子使用频次:\n")
    for h, c in hook_ct.most_common():
        f.write(f"  {h}: {c}\n")

print("weapons", len(rows))
