# 武器改动史（git churn 分析）

> 生成：2026-10-13 by agent。数据源：`git-numstat.txt`（`git log --numstat --no-renames`）。
> 明细：`weapon-churn.csv`（136 个武器文件）、`weapon-churn-hot.csv`（改动 ≥3 次的 61 个）。

## 一、分析前必须知道的三个陷阱

1. **`FirstSeen`（git 首次出现）≠ 文件创建时间**
   2025-10-19 `first commit` 只入库了**一部分**文件；**2025-11-09「更改武器分区」那次提交把大量早已存在的文件一次性入库**。
   例：`Fade.cs` git FirstSeen = 2025-11-09，但 `CreationTime` = 2025-11-08；`NeutronGun.cs` CreationTime = 2025-01-25，git FirstSeen 却是 2025-11-09。
   ⇒ **`Commits` 次数只统计 2025-10-19 之后的改动**，2024-12~2025-10 的迭代不在其中。

2. **`--no-renames` 会把"移动文件"记成"旧路径全删 + 新路径全加"**
   表中两条高删除率记录其实是移动：
   - `Content/Items/Weapons/CodeChaos.cs`（255/255）→ 移到 `Melee/CodeChaos.cs`
   - `Content/Items/Weapons/FullMoonStaff.cs`（129/129）→ 移到 `Summon/FullMoonStaff.cs`

3. **`TotalDeleted / TotalAdded` 比值高 ≈ 被重写**；比值低 ≈ 只被小修。

## 二、两次目录重组事件（从"移动痕迹"反推）

| 提交 | 说明 |
| --- | --- |
| **2025-11-09** `更改武器分区，新增怨憎，黯淡，救赎碎片，石巨人碎片` | 第一次大分区；同时把大批文件首次入库 |
| **2026-02-14** `工具更新，抛射体位置重整` | 第二次目录重整；**大量武器的 LastTouch 都是这一天**（批量触碰） |

## 三、被"重写"过的武器（删除行数显著）

| 文件 | 提交数 | Added | Deleted | 删/加 | 重写发生在一 |
| --- | --- | --- | --- | --- | --- |
| `Ranged/WoodenCrossbow.cs` | 4 | 167 | 136 | **0.81** | 2026-02-07 `灾厄紧急更新`（7 加 / 132 删） |
| `Melee/SigLightSpear.cs` | 5 | 418 | 332 | **0.79** | 2025-11-09 首次入库后多次削改 |
| `Ranged/BloodCrossbow.cs` | 4 | 170 | 133 | **0.78** | 2026-02-07 `灾厄紧急更新` |
| `Ranged/ShadowCrossbow.cs` | 5 | 210 | 146 | **0.70** | 2026-02-07 `灾厄紧急更新`（24 加 / 142 删） |
| `StarySniper/GaSniperB.cs` | 4 | 156 | 83 | **0.53** | 2026-03-30 `大量优化` |
| `StarySniper/GaSniperC.cs` | 4 | 157 | 84 | 0.53 | 2026-03-30 `大量优化` |
| `StarySniper/GaSniperA.cs` | 4 | 151 | 73 | 0.48 | 2026-03-30 `大量优化` |
| `Melee/CodeChaos.cs`(Melee) | 4 | 353 | 165 | 0.47 | 2026-02-23 `新增武器，修改本地化`（87 加 / 162 删） |
| `Melee/ChromiumSword.cs` | 3 | 147 | 65 | 0.44 | 2026-01-17 `大量更新，稳定版更新` |
| `Ranged/SelfRedemption.cs` | 7 | 243 | 89 | 0.37 | 多次 |
| `Ranged/FullMoonSword.cs` | 3 | 163 | 58 | 0.36 | 2026-01-17 |
| `Summon/FullMoonStaff.cs` | 4 | 152 | 54 | 0.36 | 2026-02-23 `新增武器，修改本地化` |

**注意 `GaSniperA/B/C` 在 `2026-03-30 大量优化` 各被删掉 73~84 行**（约一半）——这正是"从独立实现改为继承 `GaSniperAbs`"造成的。
⇒ **反过来印证了 `GaSniperA/B/C` 上那处冗余 `, IChargeableItem` 声明的来源**：它们原本各自实现接口，基类后来也实现了，改造时没删冗余。

## 四、改动最频繁（git 时期提交数）

| 文件 | 提交数 |
| --- | --- |
| `Ranged/Fade.cs` | **9** |
| `Ranged/SelfRedemption.cs` | 7 |
| `Ranged/GaussRifle.cs`、`Ranged/SoulCannon.cs`、`StaryMelee/StarrySwordAbs.cs` | 6 |
| `Magic/FullMoonEcho.cs`、`Magic/Resentment.cs`、`Melee/KillingsBlade.cs`、`Melee/SigLightSpear.cs`、`Melee/ThoughtsCrossBlade.cs`、`Ranged/AAMissileLauncher.cs`、`Ranged/AutoAimingSniperRifle.cs`、`Ranged/DotZeroFiveSniperRifle.cs`、`Ranged/IronCurtainCannon.cs`、`Ranged/ShadowCrossbow.cs`、`StaryMelee/StarySwordD.cs`、`StarySniper/GaSniperAbs.cs` | 5 |

**反例（几乎没动过）**：`StarySwordA/B/C.cs` 删/加 ≈ 0.07，`BaseMainGun.cs`、`BalloonBow.cs`、`StoneBlade.cs`、`RinyaBlade.cs` 等提交数 ≤2 ⇒ 基本一次成型。

## 五、提交时间线（22 次提交）

```
2025-10-19  first commit（部分文件入库）
2025-10-23  望月召唤杖更新
2025-10-24  新增望月短剑
2025-10-25  望月长矛更新 / 重绘望月矿石贴图 / 新增贴图
2025-10-26  新增贴图，修改贴图
2025-10-28  新增复仇之影 Boss
2025-10-31  新增高斯步枪，杀戮刀片，钢铁意志之盾，钢铁誓约之盾，不再漫游，碎片浮现
2025-11-09  更改武器分区，新增怨憎，黯淡，救赎碎片，石巨人碎片（★大量文件首次入库）
2025-11-09  新增自我救赎，救赎箭，救赎弹，利齿，荡漾水杖，夜萤火等
2025-11-12  新增铬矿，铬锭，铬矿粉，日耀圣物，望月护符，铬钢剑，荡漾水杖
2025-12-04  大量更新和机制改动
2025-12-19  众多更新（弩三兄弟、两款狙击入库）
2025-12-26  平衡版本调整
2025-12-27  大改前稳定版本
2025-12-28  EC 护盾重组前稳定版本
2026-01-17  大量更新，稳定版更新
2026-02-04  贴图更新，追踪算法更新
2026-02-06  VS 迁移准备
2026-02-07  灾厄紧急更新（★弩类机制重写）
2026-02-14  工具更新，抛射体位置重整（★目录重整，大量批量触碰）
2026-02-23  新增武器，修改本地化
2026-03-23  v1.4.5 更新
2026-03-30  大量优化（★GaSniper A/B/C 重构为继承基类）
2026-04-08  1.5.1.5
2026-04-24  v1.5.4
2026-04-30  新增克雷 debuff
2026-07-25  1.6 更新
```

（注：git 提交 message 经 PowerShell 管道后中文有轻微损坏，上表已按语义还原。）
