# 武器设计趋势 & AI 参与痕迹

> 生成：2026-10-13 by agent。数据源：`weapon-times.csv`、`git-numstat.txt`、
> `style-stats.csv` / `style-by-month.csv`（512 个 .cs 全量扫描）。
> ⚠️ 所有"AI"结论都是**风格推断**，不是行级证据（要行级证据得用 `git blame`）。

## 一、设计趋势：五个阶段

| 阶段 | 时间 | 特征 | 代表 |
| --- | --- | --- | --- |
| ① 模仿期 | 2024-12 ~ 2025-02 | 照抄官方 ExampleMod；三条 10 级平行链（狙/剑/杖）；机制=单一套路+减益 | GaSniperA~J、StarySwordA~J、StarStaffA~J |
| ② 体系期 | 2025-04 ~ 2025-08 | 原版材质升级 + 大量套装/徽章；"复制-改名"量产 | FullMoon 系列、Starry/Moon 徽章（**8/9 一天建 16 个**）、星元 10 套 |
| ③ 机制期 | 2025-10 ~ 2025-12 | 原创机制爆发：积攒-释放、蓄力、条件触发 | TangDynastySaber、KillingsBlade、ProtonCannon、弩三兄弟、NightFirefly |
| ④ 平台期 | 2026-01 ~ 2026-04 | 多模式切换、抽象基类回填、充能条、数学化弹道 | LightGap、CodeChaos、BaseCrossbow、CardMaster |
| ⑤ 联动期 | 2026-07 ~ 2026-10 | 跨作品联动 + 复杂子系统（仇恨/阻挡、AI 扩展） | ArknightsItem 芯片、AnaxaMagicTrick、BlockingSystem |

**一句话**：`照抄官方示例 → 数值换皮 → 原创机制 → 系统化平台 → 跨作品联动`。

### 基础设施（`Content/Customs/`）的演进同样清晰
- **单文件工具箱**（2025）：`HomingProjectileHelper`(1/31)、`Tools.cs`(4/20)、`DamageFlatBonus`(8/10)
- **子系统化**（2025 下半年）：`MentalOmega/`(7/13)、`Commands/`(11/30 起，聊天调试命令)、`ECShield/`(12/27 一天建 4 文件)、`HandHeld*`
- **框架化**（2026）：`AttackSpeedHelper`(1/11)、`ItemChargeUI`(1/13)、`ProjectileAIExtensionSystem` + `NPCAIExtensionSystem`(2/23)、`MinionSlotCalculator`(2/20)、`GlowmaskHelper`(2/21)、`BlockingSystem`(7/13，一天 4 文件)

## 二、AI 参与的痕迹（4 类证据）

### 1. `// ... existing code ...` —— Copilot 的省略占位符被写进源码
- 全项目 **100+ 处**，且**高度集中在系统类**：`ExpansionKele.cs`、`ShadowOfRevenge.cs`、
  `MentalOmegaUtil.cs`、`HomingProjectileHelper.cs`、`Tools.cs`、`Player/ExpansionKelePlayer.cs`…
- 含义：作者长期用 Copilot 做**整段替换式**修改，且未清理占位符。
- 按月（含该标记的文件数）：2025-08 **26/45**、2025-11 13/44、2025-12 16/46、2026-03 11/30、2024-12 5/38。

### 2. XML 中文注释的"突然出现"
`/// <summary>` 平均行数/文件（按创建月）：
```
2024-12  2.9      2025-04 19.2      2025-12  7.3      2026-04  4.6
2025-01  3.0      2025-05 16.4      2026-01 11.1      2026-07 12.1
2025-02  0.0      2025-06 33.0      2026-02  8.6      2026-10 15.0
```
⇒ **2025-04 起从 ~3 跃升到 16~33**。人手写不会突然给所有方法补 XML 注释 —— 这是 AI 整理代码的典型产物。

### 3. `SetNameOverride` 的兴衰（本地化迁移）
含 `SetNameOverride` 的文件数 / 当月文件数：
`2024-12 8/38` → `2025-08 33/45` → `2025-10 12/57` → **`2025-11 0/44` 之后归零**
⇒ 2025-11 完成 hjson 本地化迁移；2025-08 的高峰说明那批徽章是**模板批量复制**的。

### 4. 从"保留死代码"到"先写规格"
- **早期**（GaSniperA 2024-12）：带官方示例痕迹 —— `// 请看 Content/ExampleRecipes.cs 了解配方创建的详细解释`、
  `// 替换为 GaSniperA 的类型`，以及被注释掉的 `ExampleInstancedProjectile`；大量注释掉的 tooltip 死代码。
- **后期**（LightGap 2026-07）：干净的取模轮转数学（`baseProgress % (2*MaxNum)`）、项目自研工具
  `PvUtils.PositionSet(...)`、规范本地化 `GetLocalization("Tooltip")`、零死代码。
- **设计文档驱动**：`Content/Items/Weapons/Magic/CardMaster.txt` 是一份完整 AI 设计规格
  （"指令：请依据以上设计规范，编写核心的 HandEvaluator 静态类…"）——**2026-03 已进入"先规格、后实现"**。

## 三、可以看出的"三级台阶"

1. **2025-04 前后**：AI 开始参与**代码整理**（结构化注释从 3 行/文件跳到 16~33 行/文件）。
2. **2025-08 ~ 2025-12**：AI 开始参与**大规模重构/量产**（`// ... existing code ...` 密集、套装/徽章批量生成、子系统拆分）。
3. **2026-03 之后**：AI 开始参与**设计**（设计文档 → 实现；代码复杂度、抽象度明显上台阶）。

## 四、局限（务必注意）

- 以上都是**风格推断**。`// ... existing code ...` 只能证明"用了 Copilot"，不能证明"代码是 AI 写的"。
- 时间戳是**文件级**的（`CreationTime` = 首次创建，内容可能被整体重写）。
- 要行级归因，需要 `git blame`（只覆盖 2025-10-19 之后）。
