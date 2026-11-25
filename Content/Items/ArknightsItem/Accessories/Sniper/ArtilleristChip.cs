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
    /// 炮手芯片：每发弹幕第一次命中敌人时，在命中点炸出一圈范围伤害。
    /// <list type="bullet">
    /// <item><b>装备</b>：+10% 远程伤害；溅射按弹幕面板伤害的 25% 结算，半径 128 像素（≈ 8 格），主目标也吃。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+3% 远程伤害；溅射比例降为 15%。</item>
    /// </list>
    /// <para/>
    /// 结算逻辑在 <see cref="ExpansionKele.Global.ArtilleristChipGlobalNPC"/>，
    /// 「每发弹幕只炸一次」的标记在 <see cref="ExpansionKele.Global.ArtilleristChipGlobalProjectile"/>。
    /// </summary>
    public class ArtilleristChip : ModItem
    {
        /// <summary>装备档：远程伤害加成。</summary>
        public const float EquippedRangedBonus = 0.10f;

        /// <summary>收藏档：远程伤害加成。</summary>
        public const float FavoritedRangedBonus = 0.03f;

        /// <summary>装备档：溅射伤害占弹幕面板伤害的比例。</summary>
        public const float EquippedBlastRatio = 0.25f;

        /// <summary>收藏档：溅射伤害占弹幕面板伤害的比例。</summary>
        public const float FavoritedBlastRatio = 0.15f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 60);
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ArtilleristChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<ArtilleristChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 肉后后期：基础芯片 + 三级锭（精金 / 钛金），组由 Content/ExpansionKeleRecipe.cs 注册。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
                .AddRecipeGroup("ExpansionKele:TertiaryBars", 5)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            ArtilleristChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>炮手芯片的状态：一档装备 / 一档收藏。</summary>
    public class ArtilleristChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了炮手芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有炮手芯片被收藏在背包里。</summary>
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
                ? ArtilleristChip.EquippedRangedBonus
                : ArtilleristChip.FavoritedRangedBonus;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.ArtilleristChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<ArtilleristChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "ArtilleristChipDynamic", body));
        }
    }
}
