---
applyTo: "**/*.cs"
description: "写 C# 时如何查原版 ItemID/NPCID/ProjectileID 等常量名；禁止硬编码数字 ID。"
---

# 原版 ID 查询方式

## 铁律：禁止硬编码数字 ID

一律使用 `Terraria.ID` 下的强类型常量，并确保文件顶部有 `using Terraria.ID;`。

```csharp
// ✅ 正确
Projectile.NewProjectile(src, pos, vel, ProjectileID.BlueFlare, damage, kb, owner);
recipe.AddIngredient(ItemID.IronBar, 5);

// ❌ 错误：难读、易错、重命名后无人能懂
Projectile.NewProjectile(src, pos, vel, 307, damage, kb, owner);
recipe.AddIngredient(22, 5);   // 22 到底是 IronBar 还是 Demonite？
```

> 上面两个数字都是真实的：`ItemID.IronBar == 22`、`ProjectileID.BlueFlare == 307`。
> 数值在不同 ID 类之间**会重叠**（`ItemID.IronBar` 与 `TileID.Demonite` 同为 22），
> 所以必须用**与内容类型匹配的**那个 ID 类。

## 常量名怎么查（按顺序）

1. **先靠 IDE 自动补全**：绝大多数常量的内部名就是英文名的直接拼接，
   键入 `ItemID.` / `ProjectileID.` 后靠补全最快。
2. **中文名 → 内部名**（本项目 `Tmod_Ref/ID/` 下的全表，唯一权威的本地对照表）：
   - `Tmod_Ref/ID/泰拉瑞亚物品id大全1449.txt` —— `ItemID`（序号 1–5455）
   - `Tmod_Ref/ID/泰拉瑞亚射弹ID1449.txt` —— `ProjectileID`（多一列"用途 / 来源"）
   例：查"蓝照明弹" → `ProjectileID.BlueFlare`。
3. **`Tmod_Ref/tModLoader.wiki/`（官方 Wiki 克隆）**：
   - **带本地数值表**的页面：`Vanilla-Tile-IDs.md`、`Vanilla-Ammo-IDs.md`、
     `Vanilla-Projectile-AIs.md`（ProjectileID → `aiStyle`）、`Vanilla-Item-Sorting-Priorities.md`、
     `Vanilla-Interface-layers-values.md`、`Named-ID-Sets.md`
   - ⚠️ **只有官网外链、没有本地数值表**的页面：`Vanilla-Content-IDs.md`、
     `Vanilla-Item-Field-Values.md`、`Vanilla-NPC-Field-Values.md`、`Vanilla-Projectile-Field-Values.md`
     —— 别去这几页里逐条翻 ID 名称，那里只有链接。
   - `Vanilla-Content-IDs.md` 里仍值得看的通用知识：各 ID 类的 `Count` 字段、
     `Search` 字段（数字 ↔ 名字互查）、以及 `Sets` 内嵌类。
4. **语义型 ID 直接读本地 TML 源码**（`NPCAIStyleID` / `ProjAIStyleID` / `ItemRarityID` 这类，
   源码里带"行为说明 + 被哪些原版内容使用"的注释清单）：
   `E:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID\`
   （完整生成源码，比旧的 `patches\` 更全，另有 `*.TML.cs` 记录 TML 新增项）
5. **仍然找不到时**：`Tmod_Ref/` → 本地 TML 源码 → 联网（官方 wiki `terraria.wiki.gg/wiki/Data_IDs`）。
   **不要凭记忆猜常量名**；确实没有对应常量（新版本内容 / 命名反直觉）时，**先向我确认**。

## 必须遵守

- 文件顶部确保有 `using Terraria.ID;`，否则 `ItemID` 等类不可见。
- 只写 `ItemID.Name` 或直接写数字，**不要**写 `ItemID.22` 这种形式（不存在，也没有意义）。
- 涉及 `NPCID.Sets` / `ProjectileID.Sets` / `TileID.Sets` 等 `Sets` 表时，先查该表是否存在，
  再决定用 `Sets` 还是自行用 `ModPlayer` / `GlobalNPC` 记录状态。
- 模组内部的槽位登记（`ai[]` / `localAI[]` / `[SyncVar]` / Named ID Sets）**暂缓**，
  需要时再统一建，且不要与原版 ID 混淆。