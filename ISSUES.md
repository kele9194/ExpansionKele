# 已知问题 / 待改进

> 记录已发现、但目前没修的问题。发现日期 + 问题 + 修复方向，方便未来回来看。
> 每修掉一条就把对应小节删掉（或移到"已修复"）。

---

## #1 平衡指令系统基本不可用（`/balance` `/HHbalance` `/modi`）

**状态**：未修 ｜ 记录于 2026-10-15

### 它本来想做什么

一套**运行时数值调整系统**，目标是：多人模式下（多模组共开）平衡各模组武器的实际战力，
避免某个模组的武器完全沦为废品。

| 指令 | 作用 |
| --- | --- |
| `/balance global set <float>` | 全局伤害倍率 |
| `/HHbalance h set <float>` | **按手持武器所属模组**设倍率 |
| `/HHbalance van set <float>` | 原版物品倍率 |
| `/modi` | 单个物品的属性修改 |

载体物品：`BalanceScale`（平衡天平）。

### 问题

1. **没有网络同步** —— 全目录搜不到任何 `NetSend` / `SendData`。
   `ModSystem` 的 `static` 字段在服务端/客户端是两份独立副本 ⇒ **多人下只有敲指令的那一端生效**。
   而它的设计目标恰恰是多人 ⇒ **唯一想解决的场景，正好是它唯一解决不了的**。
2. **存在世界存档里**（`SaveWorldData` / `LoadWorldData`）⇒ 换世界、开新世界就丢。
   服主预期的显然是"服务器级全局生效"，应该用 `ModConfig`（ServerSide）。
3. **`_saved*` 是 static** ⇒ `Unload()` 后丢失 ⇒ **退出游戏再进，倍率回到 1.0**。
4. **四重门槛** = 等于没有入口：拿到 `BalanceScale`（**无配方，拿不到**）+ 打开配置开关（**默认 false**）
   + 手持该物品 + 记住指令与参数格式。
5. 玩家（和作者本人）都不知道它存在；代码注释里还留着 `"Effects currently empty"`。

### 修复方向（若要做）

- 用 **`ModConfig`（`ConfigScope.ServerSide`）** 代替整个指令系统：
  - 自动保存（配置文件，跨世界/跨重启）
  - 多人时由服务端自动下发给客户端（TML 原生支持）
  - 游戏内设置界面，不用记指令
- 倍率应用改为 **`GlobalItem.ModifyWeaponDamage`**（读配置，按 `item.ModItem.Mod.Name` 查表）。
- `/modi` 可保留（"改单个物品"确实适合指令）；`BalanceScale` 可改造为只读展示，或删除。
- ⚠️ 待核实：`ModConfig` 对 `List<T>` / `Dictionary<string,float>` 的支持（本机不编译，未验证）；
  保守做法是用 `List<自定义条目类>`。

### 相关文件

```
Content/Customs/Commands/BalancingCommand.cs          （/balance + BalancingSystem）
Content/Customs/Commands/HandHeldItemDamageCommand.cs （/HHbalance）
Content/Customs/Commands/ItemModificationCommand.cs   （/modi）
Content/Customs/Commands/HandHeldSystem.cs            （模组倍率表 + 存档）
Content/Customs/Commands/HandHeldGlobalItem.cs        （把倍率应用到物品）
Content/Customs/Commands/ItemPropertyModification.cs
Content/Items/OtherItem/BalanceScale.cs               （载体物品）
ExpansionKeleConfig.cs                                （EnableGlobalDamageMultiplierModification，默认 false）
```

---

## #2 「芯片刻印」系统拆分独立模组（计划中）

**状态**：待办 ｜ 记录于 2026-10-15

### 定位

`Content/Items/ArknightsItem/`（灵感来自明日方舟的「芯片刻印」系统）。
**用途：专门给"非方舟武器"套上方舟式的职业模板** ⇒ 本质是个通用的武器改造层，
并不依赖方舟内容，因此适合独立。

### 规模

- 目录内：**52 个文件 / 4814 行**
- 目录外但属于方舟的：**10 个** —— `Content/Customs/ArkModel.cs`、
  `BlockingSystem` / `BlockingPlayer` / `BlockingMarkerNPC`、`NPCProjectileSourceTracker`、
  `Content/Buff/` 下 5 个 Buff（Fighter / Liberator / DreadnoughtRebirth / MushaRevivalCooldown / Marksman）
- 合计约 **62 个文件**

### 盘点结论（好消息：边界比预想的干净）

- 方舟引用的"工具库"里**绝大多数本身就是方舟专属**（`ArkModel.cs` 全家、`Blocking*`），只是放错了目录
- **真正跨模组共享的只有 3 个类 / 6 处**：`ValueUtils`(2)、`UseTimeHelper`(2)、`ExpansionKeleDamageMulti`(2)
- **反向依赖只有 3 个文件**：`Content/Buff/FighterBuff.cs`、`Content/Customs/ItemChargeUI.cs`、
  `Content/NPCs/TownNPC/Cola.cs`

### 计划（四步，每步可单独验证）

1. **目录归位**（先在现模组内做）：10 个外部文件搬进方舟目录 + 改命名空间与引用 —— 纯搬家，零行为变更
2. **解决 3 个决策点**：`FighterBuff` 一起搬；`ItemChargeUI`（公共充能条）如何解耦；
   `ChipExtractor` 的新入口（现在由可乐 NPC 售卖）
3. **处理 6 处真依赖**：三选一 —— 复制一份 / 上提到 `InnoVault` / 方舟自己重写
4. **建新模组工程**并整体搬迁

### 关键判断

- **先做第 1 步，别直接跳到第 4 步**（第 1 步的收益独立存在，万一不拆也留着）
- 不建议用 `modReferences = ExpansionKele`（那就不算真正独立了）
- 参考数据：`AgentProbe/ark-split-report.txt`
