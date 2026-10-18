# ExpansionKele 武器开发时间线（考古报告）

> 本文件由 agent 生成，仅作资料，不参与编译（非 .cs）。
> 生成日期：2026-10-13

## 一、数据来源与可信度

| 来源 | 覆盖范围 | 说明 |
| --- | --- | --- |
| 文件系统 `CreationTime` | 2024-12-05 ~ 2026-07-07（全程） | 与 git 交叉验证一致，可信 |
| 文件系统 `LastWriteTime` | 最近一次编辑 | **仅供参考**（老文件被后续批量改动刷新过） |
| `git log`（22 个提交） | **仅 2025-10-19 及之后** | 首次提交 `first commit` = 2025-10-19；之前的开发**无 git 记录** |

- 仓库：`hasDotGit=True`，最后一次提交 `2026-07-25`（作者 Kele9194，"1.6 更新"），未提交改动 39 项。
- `git --diff-filter=A`（首次新增）与 `CreationTime` 在 2025-10 之后**逐条吻合**（如 RinyaBlade 2026-04-24、StoneBlade 2026-03-30、ChromiumSword 2025-11-12）。
- 原始数据：`file-times-content.csv`、`weapon-times.csv`、`git-log.txt`、`git-added-files.txt`。

## 二、时间线（按创建时间）

### 2024-12（起点）
| 日期 | 内容 |
| --- | --- |
| 12-05~07 | **GaSniper A~J**（星元狙击 10 把，逐个创建，间隔 1~14 小时）← **全项目最早的武器** |
| 12-09 | `ArmorPodwered`（buff，正常版） |
| 12-15 | `StarySword`（星元剑「始祖」）、`ColaProjectile`（始祖后 7 分钟） |
| 12-16 | `ArmorPodweredLower`（**晚于正常版 7 天**） |
| 12-18 | `StarySwordA`→`B`→`ColaProjectileLower`→`C`→`LifePercentageProjectile`→`D` |
| 12-20 | `StarySwordE`~`J` |

### 2025-01 ~ 2025-02
| 日期 | 内容 |
| --- | --- |
| 01-15 | `ProtonCannon` |
| 01-25 | `NeutronGun`、`AAMissileLauncher`、`IronCurtainCannon` |
| 01-29 | `LaserDrawer`（狙击激光绘制） |
| 01-30 | `ForceAttack`、`FrostRayGun` |
| 01-31~02-01 | `StarStaff` + **StarStaff A~J**（同日内批量铺开） |
| 02-02 | `StarBlade`（召唤剑，**后来整文件被注释废弃**） |

### 2025-05 ~ 2025-08
| 日期 | 内容 |
| --- | --- |
| 05-01 | `FullMoonSword`、`FullMoonBow`、`FullMoonEcho`（满月三件同日） |
| 06-28 | `PowerShotSMG` |
| 07-04 | `BaseMainGun` |
| 07-06 | **`AbsStarStaff`（法杖基类）** ← 距 A~J 已有 **5 个月** |
| 08-06 | `StingerLauncher` |

### 2025-09
| 日期 | 内容 |
| --- | --- |
| **09-29** | **`StarrySwordAbs` + `GaSniperAbs` 同日创建** ← 距 A~J 各 **9 个月**；"基类重构日" |

### 2025-10
| 日期 | 内容 |
| --- | --- |
| 10-02 | `VortexMissileLauncher` |
| 10-03 | `SoulCannon`、`SpectralCurtainCannon` |
| 10-04 | `CodeChaos` |
| 10-14 | `SigLightSpear`、`PearlWoodSpear` |
| 10-15 | `SolarSpear` ← **材料在前、产物在后（+1 天）** |
| **10-19** | **git `first commit`** |
| 10-23 | `FullMoonStaff` |
| 10-24 | `FullMoonShortSword`、`FullMoonSpear` |
| 10-28 | `KillingsBlade`（**重做版**，tooltip 自述"纪念该 mod 第一把未完成的同名武器"） |
| 10-31 | `FragmentsEmergence`、`NoMoreRoving` |

### 2025-11
| 日期 | 内容 |
| --- | --- |
| 11-01 | `Resentment` |
| 11-08 | `Fade`、`GaussRifle` |
| 11-09 | `SharpTeeth`、`SelfRedemption`、`NightFirefly`、`RippleStaff` |
| 11-12 | `Gatling`、`ChromiumSword` |
| 11-28 | `EMPPistol` |
| 11-29 | `ElectricBallista`、`SeitaadBallista`、`SolarSword` |

### 2025-12
| 日期 | 内容 |
| --- | --- |
| 12-12 | `ThoughtsCrossBlade` |
| 12-13 | `WoodenCrossbow` |
| 12-14 | `BloodCrossbow`、`ShadowCrossbow` |
| 12-15 | `SuperGear`、`BalloonBow` |
| 12-16 | `DotZeroFiveSniperRifle`、`AutoAimingSniperRifle` |

### 2026-01
| 日期 | 内容 |
| --- | --- |
| 01-11 | `SpectreSword` |
| 01-15 | `ShroomiteSword` ← 与 changelog「v1.3.4(2026.1.15) 新增蘑菇剑、幽灵长剑」吻合 |
| 01-25 | `ChromiumBow` |
| 01-26 | `M37Shotgun`、`LaserCutter` |

### 2026-02
| 日期 | 内容 |
| --- | --- |
| 02-03 | `StarySwordSP`（测试品，无配方、无本地化） |
| 02-06 | `Crossbow.cs`(`BaseCrossbow`) ← 距三个弩 2 个月 |
| 02-09 | `EnhancedSpaceGun`、`SpaceFunnel`、`BurningPaperAirPlane` |
| 02-13 | `TangDynastySaber` |
| 02-19 | `AnaxaMagicTrick` |
| 02-20 | `GoldenWeaverNeedle` |

### 2026-03 ~ 2026-07（最新批次）
| 日期 | 内容 |
| --- | --- |
| 03-23 | `Fireworks`、`WPSmokeLauncher` |
| 03-24 | `CardDeck`、`CardMaster`、`CardHandEvaluator`（同日，与 `CardMaster.txt` AI 设计文档驱动吻合） |
| 03-25 | `BoxingGloves` |
| 03-30 | `StoneBlade`（**新手起点武器，几乎最后才做**） |
| 04-01 | `BaseSwordHeldProjectile`（新抽象基类，**目前零引用** = 为将来预留） |
| 04-05 | `FeatherStaff`、`Rinya` |
| 04-07 | `CelestialPlumeStaff`、`ThousandEyedSoulChase` |
| 04-08 | `RemoteGatling` |
| 04-24 | `RinyaBlade` |
| 07-07 | `LightGap` ← 最后一个新增武器 |

## 三、可复用的规律（设计模式）

1. **抽象基类永远"事后"出现**（先写实例、后抽基类）：
   - `GaSniper A~J`(2024-12) → `GaSniperAbs`(2025-09-29)，间隔 **9 个月**
   - `StarySword A~J`(2024-12) → `StarrySwordAbs`(2025-09-29)，间隔 **9 个月**
   - `StarStaff A~J`(2025-02) → `AbsStarStaff`(2025-07-06)，间隔 **5 个月**
   - 弩三兄弟(2025-12) → `BaseCrossbow`(2026-02-06)，间隔 **2 个月**
   ⇒ 所以"子类冗余声明基类已实现的接口"这类痕迹，是**基类晚于子类**造成的，不代表子类写得更早。

2. **三条 10 级链都是"正向、逐个创建"**：GaSniper(A→J, 2024-12)、StarySword(A→J, 2024-12)、StarStaff(A~J, 2025-02)。
   - `StarySword`（始祖, 12-15）早于 A(12-18) 3 天 ⇒ 先有**废弃原型**，再从 A 重建整条链。
   - `ColaProjectile`(12-15) 早于 `ColaProjectileLower`(12-18)；`ArmorPodwered`(12-09) 早于 `ArmorPodweredLower`(12-16) ⇒ 「正常版先于下级版」。

3. **"武器合武器"配方都是正向的**：材料早于产物（`SigLightSpear`/`PearlWoodSpear` 10-14 → `SolarSpear` 10-15；`ProtonCannon` 01-15 → `SoulCannon` 10-03）。

4. **命名风格差异可作年代标记**：
   - `StaryMelee` / 裸目录名（早期）vs `Items.Weapons.Melee`（后期规范化）
   - `AbsStarStaff`（前缀 Abs，2025-07）vs `StarrySwordAbs`/`GaSniperAbs`（后缀 Abs，2025-09）

## 四、对既有推断的修正

| 既有推断 | 实际（CreationTime） | 修正 |
| --- | --- | --- |
| 星元剑 A~C 是"从完整体倒推的低阶" | A~J 逐个正向创建（12-18~12-20） | **正向链**，始祖是更早的废弃原型 |
| StarStaff 是"机制先定、后填 10 档" | A~J 同日批量铺开（02-01），基类 5 个月后 | 正向批量，基类后补 |
| GaSniper A/B/C 冗余接口 ⇒ A~C 先写 | 整条链先写（2024-12），基类 2025-09 | 结论对、理由错 |
| **StoneBlade 是早期武器** | **创建于 2026-03-30** | **错**：它是晚期补的"新手起点" |
| SolarSpear 可能是反向（拆卖点） | 材料 10-14、产物 10-15 | **正向** |
| KillingsBlade 重做版晚于星元剑 | 2025-10-28，tooltip 自述 | **正确** |
| 星元狙击在 v1.3.4 附近 | 2024-12-05（最早） | **错**：它是最早的系列 |
| ChromiumSword 早于 ThoughtsCrossBlade | 11-12 vs 12-12 | **正确** |
| StoneBlade → RinyaBlade 正向 | 03-30 → 04-24 | **正确** |
