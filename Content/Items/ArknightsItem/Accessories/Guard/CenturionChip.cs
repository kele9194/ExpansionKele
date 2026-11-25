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
    /// 强攻手芯片（近卫系）：周围的敌人越多，打它们越疼。
    /// <list type="bullet">
    /// <item><b>装备</b>：+6% 近战伤害；半径 500 像素内的敌人按「圈内人数」额外受伤：
    /// 1 人 +4%、2 人 +8%、3 人及以上 +12%。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+2% 近战伤害；1 / 2 / 3 人及以上分别为 +2% / +4% / +6%。</item>
    /// </list>
    /// <para/>
    /// 设计意图：近战一次挥动本来就会命中相邻的多个敌人，按「人数」而非「距离」给档，
    /// 玩家看到的是「人越多越猛」而不是「越远越疼」，也与方舟强攻手「挡 3」的语义一致。
    /// </summary>
    public class CenturionChip : ModItem
    {
        /// <summary>装备档：常驻近战伤害加成（加算池）。</summary>
        public const float EquippedMeleeBonus = 0.06f;

        /// <summary>收藏档：常驻近战伤害加成（加算池）。</summary>
        public const float FavoritedMeleeBonus = 0.02f;

        /// <summary>装备档：圈内 1 / 2 / 3 人及以上时，对圈内敌人的额外最终伤害。</summary>
        public const float EquippedCrowd1Bonus = 0.04f;
        public const float EquippedCrowd2Bonus = 0.08f;
        public const float EquippedCrowd3Bonus = 0.12f;

        /// <summary>收藏档：圈内 1 / 2 / 3 人及以上时的额外最终伤害。</summary>
        public const float FavoritedCrowd1Bonus = 0.02f;
        public const float FavoritedCrowd2Bonus = 0.04f;
        public const float FavoritedCrowd3Bonus = 0.06f;

        /// <summary>判定圈半径（像素）。</summary>
        public const float MarkRange = 320f;

        /// <summary>加成档位上限：3 人及以上统一按最高档。</summary>
        public const int MaxMarked = 3;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 40);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CenturionChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<CenturionChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 蜂王掉落材料，所以这件的时期由蜂蜡决定（肉前后期）。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.BeeWax, 4)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            CenturionChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>强攻手芯片的状态：一档装备 / 一档收藏。</summary>
    public class CenturionChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了强攻手芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有强攻手芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了强攻手芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有强攻手芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

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

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level == 0)
                return;

            // 常驻增伤走加算池，与斗士 / 无畏者一致，后期随玩家属性自然稀释。
            Player.GetDamage(DamageClass.Melee) += level >= 3
                ? CenturionChip.EquippedMeleeBonus
                : CenturionChip.FavoritedMeleeBonus;
        }

        /// <summary>该 NPC 是否在圈内且可作为目标（挂在玩家周围的敌人）。</summary>
        public static bool IsMarkableTarget(Player player, NPC npc)
        {
            if (!IsMarkable(npc))
                return false;

            float rangeSq = CenturionChip.MarkRange * CenturionChip.MarkRange;
            return Vector2.DistanceSquared(player.Center, npc.Center) <= rangeSq;
        }

        /// <summary>圈内可攻击敌人的数量，封顶 <see cref="CenturionChip.MaxMarked"/>。</summary>
        public static int GetNearbyCount(Player player)
        {
            float rangeSq = CenturionChip.MarkRange * CenturionChip.MarkRange;
            int count = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!IsMarkable(npc))
                    continue;

                if (Vector2.DistanceSquared(player.Center, npc.Center) > rangeSq)
                    continue;

                count++;

                // 只区分「1 / 2 / 3 及以上」，数到上限就可以停。
                if (count >= CenturionChip.MaxMarked)
                    return CenturionChip.MaxMarked;
            }

            return count;
        }

        /// <summary>可参与排名的敌人：友好单位、假人、无敌与无伤目标都不算。</summary>
        private static bool IsMarkable(NPC npc)
            => npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage
                && npc.type != NPCID.TargetDummy;

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.CenturionChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<CenturionChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "CenturionChipDynamic", body));
        }
    }
}
