using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.ArknightsItem.Materials;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper
{
    /// <summary>
    /// 神射手芯片，两种激活方式（装备优先，互斥）：
    /// <list type="bullet">
    /// <item><b>装备</b>：+5% 远程伤害、+6% 远程暴击率；对「低防目标」额外 +25% 暴击伤害。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+1% 远程伤害、+2% 远程暴击率；对「低防目标」额外 +10% 暴击伤害。</item>
    /// </list>
    /// <para/>
    /// 「低防目标」= <c>npc.defense &lt; 本次弹幕伤害 × 40%</c>，不设上下限。
    /// 判定落在 <see cref="ExpansionKele.Global.MarksmanChipGlobalNPC"/>。
    /// </summary>
    public class MarksmanChip : ModItem
    {
        /// <summary>装备：远程伤害加成。</summary>
        public const float EquippedRangedBonus = 0.05f;

        /// <summary>装备：远程暴击率加成（点，不是比例）。</summary>
        public const float EquippedCritBonus = 6f;

        /// <summary>装备：对低防目标的额外暴击伤害（2.0x → 2.25x）。</summary>
        public const float EquippedCritDamageBonus = 0.25f;

        /// <summary>仅收藏：远程伤害加成。</summary>
        public const float FavoritedRangedBonus = 0.01f;

        /// <summary>仅收藏：远程暴击率加成（点）。</summary>
        public const float FavoritedCritBonus = 2f;

        /// <summary>仅收藏：对低防目标的额外暴击伤害。</summary>
        public const float FavoritedCritDamageBonus = 0.10f;

        /// <summary>低防判定阈值：目标防御低于「本次伤害 × 该比例」即视为低防。</summary>
        public const float FragileDefenseRatio = 0.4f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += EquippedRangedBonus;
            player.GetCritChance(DamageClass.Ranged) += EquippedCritBonus;
            player.GetModPlayer<MarksmanChipPlayer>().equipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<MarksmanChipPlayer>().favorited = true;
        }

        public override void AddRecipes()
        {
            // 银与钨是互斥世界生成，直接写两条配方，不做 RecipeGroup。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
                .AddIngredient(ItemID.Lens, 1)
                .AddIngredient(ItemID.SilverBar, 1)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
                .AddIngredient(ItemID.Lens, 1)
                .AddIngredient(ItemID.TungstenBar, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            MarksmanChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>
    /// 记录「该玩家是否戴着 / 收藏着神射手芯片」，供
    /// <see cref="ExpansionKele.Global.MarksmanChipGlobalNPC"/> 判断加成档次。
    /// </summary>
    public class MarksmanChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了神射手芯片。</summary>
        public bool equipped;

        /// <summary>本帧是否有神射手芯片被收藏在背包里。</summary>
        public bool favorited;

        /// <summary>生效档位：0 = 无，1 = 收藏，2 = 装备。</summary>
        public int Level => equipped ? 2 : favorited ? 1 : 0;

        public override void ResetEffects()
        {
            equipped = false;
            favorited = false;
        }

        public override void PostUpdateMiscEffects()
        {
            // 装备优先：饰品栏里已有芯片时，背包收藏的数值不生效。
            if (equipped || !favorited)
                return;

            Player.GetDamage(DamageClass.Ranged) += MarksmanChip.FavoritedRangedBonus;
            Player.GetCritChance(DamageClass.Ranged) += MarksmanChip.FavoritedCritBonus;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.MarksmanChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<MarksmanChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");

            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "MarksmanChipDynamic", body));
        }
    }
}
