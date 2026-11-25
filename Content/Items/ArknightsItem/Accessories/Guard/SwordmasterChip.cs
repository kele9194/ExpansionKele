using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Materials;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 剑豪芯片（近卫系）：走攻速流，连续命中的第 4 次挥动会打出加重一击。
    /// <list type="bullet">
    /// <item><b>装备</b>：+10% 近战攻速；每第 4 次<b>命中</b>的挥动，该次伤害 ×1.45。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+5% 近战攻速；每第 4 次命中的挥动，该次伤害 ×1.10。</item>
    /// </list>
    /// <para/>
    /// 攻速故意压在 +10%：`useTime` 是整数帧，+15% 会让常见武器跨一帧（实际变 +25%），
    /// +10% 则不跨（实际 +11%），纸面与实际才能对上。
    /// <para/>
    /// 计数方式为「命中的挥动」而不是「挥动」或「命中次数」：
    /// 空挥不推进连击（否则可以对着空气刷第三击），一次挥动砍到多个敌人也只算 1 次
    /// （否则群攻时第三击会连续触发），但该次挥动命中的所有目标都会吃到加重倍率。
    /// <para/>
    /// 连击窗口 3 秒：超过这个时间没有新的命中就断开。空挥不会主动打断，只有「长时间不命中」才算断连。
    /// </summary>
    public class SwordmasterChip : ModItem
    {
        /// <summary>装备档：近战攻速加成（加算池）。</summary>
        public const float EquippedAttackSpeedBonus = 0.10f;

        /// <summary>收藏档：近战攻速加成（加算池）。</summary>
        public const float FavoritedAttackSpeedBonus = 0.05f;

        /// <summary>装备档：加重一击的伤害倍率。</summary>
        public const float EquippedThirdStrikeMultiplier = 1.45f;

        /// <summary>收藏档：加重一击的伤害倍率。</summary>
        public const float FavoritedThirdStrikeMultiplier = 1.10f;

        /// <summary>第几次命中的挥动触发加重一击。</summary>
        public const int ThirdStrikeInterval = 4;

        /// <summary>连击窗口（帧）：3 秒内没有新的命中则断开。</summary>
        public const int ComboWindow = 180;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.LightPurple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SwordmasterChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<SwordmasterChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 幽灵锭：肉后地牢/花后阶段，攻速流需要足够快的武器才体现价值。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.SpectreBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SwordmasterChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>剑豪芯片的状态：一档装备 / 一档收藏。</summary>
    public class SwordmasterChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了剑豪芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有剑豪芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了剑豪芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有剑豪芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>连续命中的挥动次数（0 ~ ThirdStrikeInterval-1）。</summary>
        private int _comboCount;

        /// <summary>连击剩余窗口帧数，归零则断开连击。</summary>
        private int _comboTimer;

        /// <summary>本次挥动是否已经计过数，避免一刀砍多个目标时重复推进连击。</summary>
        private bool _hitThisSwing;

        /// <summary>本次挥动是否是第三击。</summary>
        private bool _thirdStrike;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。</summary>
        public bool IsEquipped => Level >= 3;

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;
        }

        public override void PostUpdate()
        {
            // 只处理连击窗口。本刀的命中/加重标记由 OnMeleeSwingStart 复位——
            // 不能在这里用 itemAnimation <= 0 判断：autoReuse 时 TML 不会有 itemAnimation == 0 的帧
            // （Player.cs:35628），那样标记永远不会被清，连击卡在第 1 层。
            if (_comboTimer > 0 && --_comboTimer == 0)
                _comboCount = 0;
        }

        /// <summary>
        /// 每次近战挥动开始时由 <see cref="Global.SwordmasterChipGlobalItem"/> 调用：
        /// 复位「本刀是否已命中」与「本刀是否加重一击」。
        /// <para/>
        /// 挥空不计入连击 —— 计数器只在 <see cref="ModifyHitNPC"/> 里推进。
        /// </summary>
        public void OnMeleeSwingStart()
        {
            _hitThisSwing = false;
            _thirdStrike = false;
        }

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level <= 0)
                return;

            Player.GetAttackSpeed(DamageClass.Melee) += level >= 3
                ? SwordmasterChip.EquippedAttackSpeedBonus
                : SwordmasterChip.FavoritedAttackSpeedBonus;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int level = Level;
            if (level <= 0)
                return;

            if (!modifiers.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            // 本次挥动的第一次命中：推进连击并判定加重一击
            if (!_hitThisSwing)
            {
                _hitThisSwing = true;
                _comboCount++;
                _comboTimer = SwordmasterChip.ComboWindow;

                _thirdStrike = _comboCount >= SwordmasterChip.ThirdStrikeInterval;
                if (_thirdStrike)
                    _comboCount = 0;
            }

            if (!_thirdStrike)
                return;

            modifiers.FinalDamage *= level >= 3
                ? SwordmasterChip.EquippedThirdStrikeMultiplier
                : SwordmasterChip.FavoritedThirdStrikeMultiplier;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.SwordmasterChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<SwordmasterChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "SwordmasterChipDynamic", body));
        }
    }
}
