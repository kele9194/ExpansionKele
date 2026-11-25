using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Materials;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper
{
    /// <summary>
    /// 攻城手芯片（一级芯片）：专打大体型目标。
    /// <list type="bullet">
    /// <item><b>装备</b>：+5% 远程伤害；对大型目标的额外增伤随体型（宽+高）从 60 到 300 由 0% 线性提升至 25%。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+2% 远程伤害；同一区间内由 0% 提升至 10%。</item>
    /// </list>
    /// <para/>
    /// 结算逻辑在 <see cref="ExpansionKele.Global.SiegeChipGlobalNPC"/>。
    /// </summary>
    public class SiegeChip : ModItem
    {
        /// <summary>装备档：远程伤害 / 对大体型目标的最高额外伤害。</summary>
        public const float EquippedRangedBonus = 0.05f;
        public const float EquippedMaxBonus = 0.25f;

        /// <summary>收藏档：远程伤害 / 对大体型目标的最高额外伤害。</summary>
        public const float FavoritedRangedBonus = 0.02f;
        public const float FavoritedMaxBonus = 0.10f;

        /// <summary>体型下界（<c>npc.width + npc.height</c>）：此处额外增伤为 0。</summary>
        public const int MinLargeSize = 60;

        /// <summary>体型上界：此处吃满额外增伤。</summary>
        public const int MaxLargeSize = 300;

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
            player.GetModPlayer<SiegeChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<SiegeChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 肉后中期：基础芯片 + 二级锭（秘银 / 山铜），组由 Content/ExpansionKeleRecipe.cs 注册。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
                .AddRecipeGroup("ExpansionKele:SecondaryBars", 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SiegeChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>攻城手芯片的状态：一档装备 / 一档收藏。</summary>
    public class SiegeChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了攻城手芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有攻城手芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>生效档位：0 = 无，1 = 收藏，2 = 装备。</summary>
        public int Level => chipEquipped ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（决定用装备那套数值）。</summary>
        public bool IsEquipped => chipEquipped;

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
        }

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level == 0)
                return;

            Player.GetDamage(DamageClass.Ranged) += level == 2
                ? SiegeChip.EquippedRangedBonus
                : SiegeChip.FavoritedRangedBonus;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.SiegeChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<SiegeChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "SiegeChipDynamic", body));
        }
    }
}
