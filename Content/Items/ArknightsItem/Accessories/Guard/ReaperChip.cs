using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.ArknightsItem.Materials;
using ExpansionKele.Global;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 收割者芯片（近卫系）：不能自然回血，从敌人身上「收割」生命，并把对方打弱、把自己打强。
    /// <list type="bullet">
    /// <item><b>装备</b>：关闭自然回复（药水 / 护士照常）；
    /// 每次攻击最多对 2 个敌人各回复<b>已损生命 3%</b>；
    /// 命中后 10 秒内该敌人攻击与防御各 −15%，自身攻击与防御各 +15%（均可刷新）。</item>
    /// <item><b>未装备时放于背包并收藏</b>：不禁疗、不回血；命中后 10 秒内
    /// 敌人攻击 / 防御 −5%，自身攻击 / 防御 +5%。</item>
    /// </list>
    /// <para/>
    /// 回血基数是<b>已损生命</b>而不是最大生命 —— 满血时回不了，越残回得越多，
    /// 与武者（最大生命 2%，稳定续航）正好互补。
    /// </summary>
    public class ReaperChip : ModItem
    {
        /// <summary>装备档：每次回血占「已损生命」的比例。</summary>
        public const float EquippedHealRatio = 0.03f;

        /// <summary>每次攻击最多计入回血的目标数。</summary>
        public const int MaxHealTargets = 2;

        /// <summary>装备档：敌人攻击 / 防御削减比例。</summary>
        public const float EquippedEnemyAttackReduction = 0.15f;
        public const float EquippedEnemyDefenseReduction = 0.15f;

        /// <summary>收藏档：敌人攻击 / 防御削减比例。</summary>
        public const float FavoritedEnemyAttackReduction = 0.05f;
        public const float FavoritedEnemyDefenseReduction = 0.05f;

        /// <summary>装备档：自身攻击 / 防御加成。</summary>
        public const float EquippedSelfAttackBonus = 0.15f;
        public const float EquippedSelfDefenseBonus = 0.15f;

        /// <summary>收藏档：自身攻击 / 防御加成。</summary>
        public const float FavoritedSelfAttackBonus = 0.05f;
        public const float FavoritedSelfDefenseBonus = 0.05f;

        /// <summary>增益 / 减益持续时间（帧）：10 秒。</summary>
        public const int EffectDuration = 600;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 80);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ReaperChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<ReaperChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 肉后中后期：近卫基础芯片 + 神圣锭（机械 Boss 掉落），秘银砧加工。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.HallowedBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            ReaperChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>收割者芯片的状态：一档装备 / 一档收藏。</summary>
    public class ReaperChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了收割者芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有收割者芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了收割者芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有收割者芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。回血与禁疗只在装备档生效。</summary>
        public bool IsEquipped => Level >= 3;

        /// <summary>自身增益剩余帧数。</summary>
        public int SelfBuffTimer { get; private set; }

        /// <summary>
        /// 本刀剩余的回血额度（最多 <see cref="ReaperChip.MaxHealTargets"/> 个目标）。
        /// <para/>
        /// 由 <see cref="Global.ReaperChipGlobalItem"/> 在 <c>GlobalItem.UseAnimation</c> 里重置 ——
        /// 那个钩子每刀只执行一次，不依赖 <c>itemAnimation</c> 的逐帧递减。
        /// </summary>
        private int _healCharges;

        /// <summary>本刀已经回血过的目标；同一刀对同一个敌人只回一次。</summary>
        private readonly HashSet<int> _healedTargets = new();

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;
        }

        public override void PostUpdate()
        {
            if (SelfBuffTimer > 0)
                SelfBuffTimer--;
        }

        /// <summary>
        /// 禁疗：只压制<b>正面</b>生命再生。
        /// <para/>
        /// ⚠️ 不能无条件写 <c>lifeRegen = 0</c>：中毒 / 燃烧 / 电击 / 诅咒地狱这些 debuff
        /// 都是靠「负的 lifeRegen」扣血的，一并清零会让玩家戴上芯片就免疫所有持续伤害。
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

            Player.lifeRegenTime = 0;

            if (Player.lifeRegenCount > 0)
                Player.lifeRegenCount = 0;
        }

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level == 0)
                return;

            if (SelfBuffTimer <= 0)
                return;

            bool equipped = level >= 3;

            Player.GetDamage(DamageClass.Melee) += equipped
                ? ReaperChip.EquippedSelfAttackBonus
                : ReaperChip.FavoritedSelfAttackBonus;

            // 百分比防御：statDefense 每帧由装备重算，这里 +15% 不会累积。
            float defenseRatio = equipped
                ? ReaperChip.EquippedSelfDefenseBonus
                : ReaperChip.FavoritedSelfDefenseBonus;

            Player.statDefense += (int)(Player.statDefense * defenseRatio);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Level == 0 || !target.active || target.friendly)
                return;

            if (!item.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            OnMeleeHit(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Level == 0 || !target.active || target.friendly)
                return;

            if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            OnMeleeHit(target);
        }

        /// <summary>
        /// 每次近战挥动开始时由 <see cref="Global.ReaperChipGlobalItem"/> 调用：
        /// 重置本刀的回血额度，并清掉上一刀的目标记忆。
        /// </summary>
        public void OnMeleeSwingStart()
        {
            _healCharges = ReaperChip.MaxHealTargets;
            _healedTargets.Clear();
        }

        /// <summary>
        /// 本体与弹幕共用的处理：先刷新削敌 / 自强化，再按本刀额度回血。
        /// <para/>
        /// 配额是「本刀最多 <see cref="ReaperChip.MaxHealTargets"/> 个<b>不同</b>目标」——
        /// 同一刀用本体打 A、弹幕打 B，两边都能回；同一刀反复打 A 只算一次。
        /// </summary>
        private void OnMeleeHit(NPC target)
        {
            bool equipped = Level >= 3;

            // 削敌 + 自身增益：两档都生效，每次命中都刷新。
            ReaperChipGlobalNPC.ApplyDebuff(
                target,
                equipped ? ReaperChip.EquippedEnemyDefenseReduction : ReaperChip.FavoritedEnemyDefenseReduction,
                equipped ? ReaperChip.EquippedEnemyAttackReduction : ReaperChip.FavoritedEnemyAttackReduction);

            SelfBuffTimer = ReaperChip.EffectDuration;

            if (!equipped)
                return;

            if (_healCharges <= 0)
                return;

            if (!_healedTargets.Add(target.whoAmI))
                return;

            _healCharges--;
            HealFromHit();
        }

        /// <summary>
        /// 按「已损生命」比例回复，<b>至少 1 点</b>。
        /// <para/>
        /// 下限不只是为了手感：满血时已损为 0，不给下限就永远看不到任何反馈，
        /// 根本无法区分「芯片没生效」和「已经满血」。
        /// 满血时 <see cref="Player.Heal"/> 会被上限截断，实际不会溢出。
        /// </summary>
        private void HealFromHit()
        {
            // 玩家生命是客户端权威的：服务端重复加血没有意义，也会白白多消耗一次随机数。
            if (Player.whoAmI != Main.myPlayer)
                return;

            int missing = Player.statLifeMax2 - Player.statLife;
            int heal = System.Math.Max(1, ValueUtils.ProbabilisticRound(missing * ReaperChip.EquippedHealRatio));

            Player.Heal(heal);
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.ReaperChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<ReaperChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "ReaperChipDynamic", body));
        }
    }
}
