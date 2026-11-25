using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.ArknightsItem.Materials;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 武者芯片（近卫系）：不能自然回血，靠攻击把生命打回来；血越少打得越快。
    /// <list type="bullet">
    /// <item><b>装备</b>：关闭自然回复（药水 / 护士照常）；近战每次挥动命中回复 4% 已损失生命；
    /// 生命损失 70% 时获得 +50% 近战攻速与 +50% 近战伤害（线性提升）。</item>
    /// <item><b>未装备时放于背包并收藏</b>：<b>不给回血、不禁疗</b>，只有坚忍：
    /// 生命损失 70% 时 +15% 近战攻速与 +15% 近战伤害。</item>
    /// </list>
    /// <para/>
    /// 禁疗只关 <c>lifeRegen</c>：方舟的「治疗」对应 TR 的持续回复链
    /// （篝火 / 心灯 / 蜂蜜 / 再生药水），而瞬发急救（治疗药水、护士、心形掉落）不属于同一维度，
    /// 也没有任何 mod 能干净地拦下来。
    /// </summary>
    public class MushaChip : ModItem
    {
        /// <summary>装备档：一次挥动命中回复的「已损失生命」比例。</summary>
        public const float EquippedHealRatio = 0.04f;

        /// <summary>装备档：坚忍满额时的近战攻速倍率（乘算，0.5 = ×1.5）。</summary>
        public const float EquippedMaxGritSpeed = 0.50f;

        /// <summary>装备档：坚忍满额时的近战伤害加成（加算池）。</summary>
        public const float EquippedMaxGritDamage = 0.50f;

        /// <summary>收藏档：坚忍满额时的近战攻速倍率（乘算）。</summary>
        public const float FavoritedMaxGritSpeed = 0.15f;

        /// <summary>收藏档：坚忍满额时的近战伤害加成。</summary>
        public const float FavoritedMaxGritDamage = 0.15f;

        /// <summary>坚忍达到满额所需的「已损失生命比例」。</summary>
        public const float GritFullAtLossRatio = 0.70f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 60);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<MushaChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<MushaChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 肉后早期：近卫基础芯片 + 一级锭（钴 / 钯），组由 Content/ExpansionKeleRecipe.cs 注册。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddRecipeGroup("ExpansionKele:PrimaryBars", 5)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            MushaChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>武者芯片的状态：一档装备 / 一档收藏。</summary>
    public class MushaChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了武者芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有武者芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了武者芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有武者芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。回血与禁疗只在装备档生效。</summary>
        public bool IsEquipped => Level >= 3;

        /// <summary>
        /// 当前可用的回血额度（0 或 1）。
        /// <para/>
        /// 由 <see cref="Global.MushaChipGlobalItem"/> 在 <c>GlobalItem.UseAnimation</c> 里授予 ——
        /// 那个钩子由 <c>Player.ItemCheck_StartActualUse</c> → <c>ApplyItemAnimation</c> 调用，
        /// <b>只在实际开始挥动的那一帧执行一次</b>，是「一次 useAnimation」最可靠的信号，
        /// 按住攻击也不会漏，也不依赖 <c>itemAnimation</c> 的逐帧递减。
        /// </summary>
        private int _healCharges;

        /// <summary>每次挥动授予 1 次回血额度。覆盖而不是累加，所以最多存 1 次。</summary>
        public void GrantSwingHealCharge() => _healCharges = 1;

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;
        }

        /// <summary>
        /// 禁疗：只压制<b>正面</b>生命再生。
        /// <para/>
        /// ⚠️ 不能无条件写 <c>lifeRegen = 0</c>：中毒 / 燃烧 / 电击 / 诅咒地狱这些 debuff
        /// 都是靠「负的 lifeRegen」扣血的，一并清零会让玩家戴上芯片就免疫所有持续伤害。
        /// 原版自己的负面再生处理（如 electrified）也是这个写法：先压正数，再减。
        /// </summary>
        public override void UpdateBadLifeRegen()
        {
            if (!IsEquipped)
                return;

            // 星云套的生命回复层数会抵消负再生（原版：lifeRegen < 0 && nebulaLevelLife > 0 → 归零），
            // 禁疗时一并清掉，否则穿着星云套仍能靠它扁住 debuff 掉血。
            Player.nebulaLevelLife = 0;

            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;

            // 清零「连续再生计时」与已累积的再生量，避免脱下芯片后瞬间回血。
            Player.lifeRegenTime = 0;

            if (Player.lifeRegenCount > 0)
                Player.lifeRegenCount = 0;
        }

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level == 0)
                return;

            // 坚忍的伤害走加算池；攻速走 UseSpeedMultiplier（乘算），见下方。
            float gritDamage = ComputeGritRatio() * (level >= 3
                ? MushaChip.EquippedMaxGritDamage
                : MushaChip.FavoritedMaxGritDamage);

            if (gritDamage > 0f)
                Player.GetDamage(DamageClass.Melee) += gritDamage;
        }

        /// <summary>
        /// 坚忍进度（0~1）：已损失生命比例线性映射，
        /// 到 <see cref="MushaChip.GritFullAtLossRatio"/> 时满额。
        /// </summary>
        private float ComputeGritRatio()
        {
            if (Player.dead || Player.statLifeMax2 <= 0)
                return 0f;

            float lostRatio = 1f - Player.statLife / (float)Player.statLifeMax2;
            return MathHelper.Clamp(lostRatio / MushaChip.GritFullAtLossRatio, 0f, 1f);
        }

        /// <summary>
        /// 坚忍攻速走 <c>CombinedHooks.TotalUseSpeedMultiplier</c>：
        /// 它等于 <c>PlayerLoader.UseSpeedMultiplier × ItemLoader.UseSpeedMultiplier × GetWeaponAttackSpeed</c>，
        /// 所以在这里返回 &gt;1 就是在<b>加算攻速之上再做一次乘算</b>，
        /// 既不会污染 <c>GetAttackSpeed</c> 的加算池，也不会和武器自己的乘算互相覆盖。
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            int level = Level;
            if (level == 0 || item.IsAir || !item.CountsAsClass(DamageClass.Melee))
                return 1f;

            float grit = ComputeGritRatio();
            if (grit <= 0f)
                return 1f;

            float maxSpeed = level >= 3
                ? MushaChip.EquippedMaxGritSpeed
                : MushaChip.FavoritedMaxGritSpeed;

            return 1f + grit * maxSpeed;
        }

        /// <summary>
        /// 近战本体命中：一次挥动最多回 1 次（同一刀打到多个敌人也只 1 次）。
        /// </summary>
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!CanHeal(target))
                return;

            if (!item.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            if (!TryConsumeHealCharge())
                return;

            HealFromHit();
        }

        /// <summary>
        /// 近战弹幕命中（剑气 / 投刀 / <c>noMelee</c> 的近战武器）。
        /// <para/>
        /// 与本体的额度共享，所以「本体 + 剑气」一刀合计仍只回 1 次；
        /// 一发弹幕穿透多个敌人也只会消耗那 1 次额度。
        /// </summary>
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!CanHeal(target))
                return;

            if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            if (!TryConsumeHealCharge())
                return;

            HealFromHit();
        }

        /// <summary>消耗一次回血额度；没有额度时返回 <see langword="false"/>。</summary>
        private bool TryConsumeHealCharge()
        {
            if (_healCharges <= 0)
                return false;

            _healCharges--;
            return true;
        }

        /// <summary>回血的通用前置：装备档生效 + 目标是可攻击的活敌人。</summary>
        private bool CanHeal(NPC target)
            => IsEquipped && target.active && !target.friendly;

        /// <summary>
        /// 按「已损失生命」比例回复，<b>至少 1 点</b>。
        /// <para/>
        /// 与坚忍同属「越残越强」的设计：半血时与旧的最大生命 2% 持平，
        /// 高于半血更弱、濒死时更强。
        /// <para/>
        /// 下限不只是手感：满血时已损为 0，不给下限就永远看不到任何反馈，
        /// 无法区分「芯片没生效」和「已经满血」。
        /// </summary>
        private void HealFromHit()
        {
            // 玩家生命是客户端权威的：服务端重复加血没有意义，也会白白多消耗一次随机数。
            if (Player.whoAmI != Main.myPlayer)
                return;

            // ProbabilisticRound 保留小数部分的期望值：
            // 已损 80 × 4% = 3.2，直接 (int) 会永远损失 0.2，长期实际回复量低于标称比例。
            int missing = Player.statLifeMax2 - Player.statLife;
            int heal = System.Math.Max(1, ValueUtils.ProbabilisticRound(missing * MushaChip.EquippedHealRatio));

            Player.Heal(heal);
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.MushaChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<MushaChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "MushaChipDynamic", body));
        }
    }
}
