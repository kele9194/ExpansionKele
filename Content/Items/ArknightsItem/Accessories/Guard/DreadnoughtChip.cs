using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Materials;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 无畏者芯片（近卫系）：附近只剩一个敌人时，对它打出更重的伤害。
    /// <list type="bullet">
    /// <item><b>装备</b>：+5% 近战伤害；半径 300 像素内只有这 1 个敌人时，对该敌人伤害 ×1.15。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+2% 近战伤害；同一条件下 ×1.05。</item>
    /// </list>
    /// <para/>
    /// 结算逻辑在 <see cref="ExpansionKele.Global.DreadnoughtChipGlobalNPC"/>。
    /// </summary>
    public class DreadnoughtChip : ModItem
    {
        /// <summary>装备档：近战伤害加成。</summary>
        public const float EquippedMeleeBonus = 0.05f;

        /// <summary>收藏档：近战伤害加成。</summary>
        public const float FavoritedMeleeBonus = 0.02f;

        /// <summary>装备档：圈内只剩一个敌人时，对该敌人的额外最终伤害。</summary>
        public const float EquippedSoleBonus = 0.12f;

        /// <summary>收藏档：圈内只剩一个敌人时，对该敌人的额外最终伤害。</summary>
        public const float FavoritedSoleBonus = 0.04f;

        /// <summary>判定半径（像素），以玩家为圆心。</summary>
        public const float SoleRange = 300f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 40);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DreadnoughtChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<DreadnoughtChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 材料是基础芯片 + 木剑两件早期物品，所以工作台也一并降到工作台，不再卡秘银砧。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.WoodenSword, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            DreadnoughtChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>无畏者芯片的状态：一档装备 / 一档收藏。</summary>
    public class DreadnoughtChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了无畏者芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有无畏者芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了无畏者芯片模组。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有无畏者芯片模组被收藏在背包里。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// <para/>
        /// 模组是芯片的上位替代：同档时模组优先，所以「装备芯片(3)」也低于「装备模组(4)」。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4），决定用装备那套数值。</summary>
        public bool IsEquipped => Level >= 3;

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;
        }

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level == 0)
                return;

            Player.GetDamage(DamageClass.Melee) += level >= 3
                ? DreadnoughtChip.EquippedMeleeBonus
                : DreadnoughtChip.FavoritedMeleeBonus;
        }

        /// <summary>
        /// 本帧被标记的敌人索引，<c>-1</c> 表示无。
        /// <para/>
        /// 只给绘制用（<see cref="ExpansionKele.Global.DreadnoughtChipGlobalNPC.PostDraw"/>）；
        /// 伤害结算每次命中都自己重算，不依赖这个缓存。
        /// </summary>
        public int MarkedTargetWhoAmI { get; private set; } = -1;

        public override void PostUpdate()
        {
            // 标记只画给本地玩家的客户端，其他端算出来没人看。
            // 服务端的 Main.myPlayer 是 255，这里会直接落到 -1，不会误算。
            if (Player.whoAmI != Main.myPlayer)
            {
                MarkedTargetWhoAmI = -1;
                return;
            }

            MarkedTargetWhoAmI = Level == 0 ? -1 : FindMarkedTargetIndex(Player);
        }

        /// <summary>
        /// 当前被标记的敌人索引，<c>-1</c> 表示没有标记。
        /// <para/>
        /// 伤害结算与头顶标记都走这一个入口 —— 将来模组芯片要换判定规则
        /// （例如从「圈内唯一」换成「圈内最近」），只改这里，不用动 <c>GlobalNPC</c>。
        /// </summary>
        public static int FindMarkedTargetIndex(Player player)
        {
            // TODO: 模组芯片上线后，在此按 moduleEquipped / moduleFavorited 分支到「圈内最近」规则。
            return FindSoleTargetIndex(player);
        }

        /// <summary>
        /// 找出「以玩家为圆心、半径 <see cref="DreadnoughtChip.SoleRange"/> 内唯一的那一个敌人」。
        /// <para/>
        /// 圈内只剩一个时，它必然就是离玩家最近的那个 —— 这就是芯片描述里的「标记」。
        /// </summary>
        /// <returns>唯一敌人的 <c>whoAmI</c>；没有敌人或不止一个时返回 <c>-1</c>。</returns>
        private static int FindSoleTargetIndex(Player player)
        {
            float rangeSq = DreadnoughtChip.SoleRange * DreadnoughtChip.SoleRange;
            int found = -1;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!IsMarkable(npc))
                    continue;

                if (Vector2.DistanceSquared(player.Center, npc.Center) > rangeSq)
                    continue;

                // 已经有第二个了 —— 标记失效，直接放弃整轮扫描。
                if (found != -1)
                    return -1;

                found = i;
            }

            return found;
        }

        /// <summary>
        /// 能否被计数 / 标记：友好单位、假人、无敌与无伤目标都不算，
        /// 否则城镇里永远触发不了。判据与 <c>HomingProjectileHelper</c> 一致。
        /// </summary>
        private static bool IsMarkable(NPC npc)
            => npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage
                && npc.type != NPCID.TargetDummy;

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.DreadnoughtChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<DreadnoughtChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "DreadnoughtChipDynamic", body));
        }
    }
}
