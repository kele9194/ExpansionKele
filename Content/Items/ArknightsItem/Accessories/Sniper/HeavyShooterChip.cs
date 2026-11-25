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
    /// 重射手芯片（一级芯片）：越贴身的命中越重。
    /// <list type="bullet">
    /// <item><b>破甲</b>：20%~50% 比例破甲，只作用于装备者自己造成的伤害。</item>
    /// <item><b>击退</b>：1.5x ~ 3x。</item>
    /// <item><b>距离</b>：按命中时弹幕到玩家的距离插值 —— 240 像素内取近端满值，600 像素外取远端下限。</item>
    /// </list>
    /// <para/>
    /// 结算逻辑在 <see cref="ExpansionKele.Global.HeavyShooterChipGlobalNPC"/>。
    /// </summary>
    public class HeavyShooterChip : ModItem
    {
        /// <summary>近端距离（像素）：再近也不会更强。</summary>
        public const float NearDistance = 240f;

        /// <summary>远端距离（像素）：再远也不会更弱。</summary>
        public const float FarDistance = 600f;

        /// <summary>装备档：近端 / 远端比例破甲。</summary>
        public const float EquippedNearPen = 0.50f;
        public const float EquippedFarPen = 0.20f;

        /// <summary>装备档：近端 / 远端击退倍率。</summary>
        public const float EquippedNearKnockback = 3f;
        public const float EquippedFarKnockback = 1.5f;

        /// <summary>收藏档：近端 / 远端比例破甲。</summary>
        public const float FavoritedNearPen = 0.25f;
        public const float FavoritedFarPen = 0.10f;

        /// <summary>收藏档：近端 / 远端击退倍率。</summary>
        public const float FavoritedNearKnockback = 2f;
        public const float FavoritedFarKnockback = 1.25f;

        /// <summary>装备档：远程伤害 / 远程暴击率加成。</summary>
        public const float EquippedRangedBonus = 0.04f;
        public const float EquippedCritBonus = 8f;

        /// <summary>收藏档：远程伤害 / 远程暴击率加成。</summary>
        public const float FavoritedRangedBonus = 0.01f;
        public const float FavoritedCritBonus = 2f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 25);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<HeavyShooterChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<HeavyShooterChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 肉后早期：基础芯片 + 一级锭（钴 / 钯），组由 Content/ExpansionKeleRecipe.cs 注册。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
                .AddRecipeGroup("ExpansionKele:PrimaryBars", 5)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            HeavyShooterChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>
    /// 重射手芯片的状态。只有一档装备 / 一档收藏，没有模组时不需要维度拆分。
    /// </summary>
    public class HeavyShooterChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了重射手芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有重射手芯片被收藏在背包里。</summary>
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

            bool equipped = level == 2;
            Player.GetDamage(DamageClass.Ranged) += equipped
                ? HeavyShooterChip.EquippedRangedBonus
                : HeavyShooterChip.FavoritedRangedBonus;
            Player.GetCritChance(DamageClass.Ranged) += equipped
                ? HeavyShooterChip.EquippedCritBonus
                : HeavyShooterChip.FavoritedCritBonus;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.HeavyShooterChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<HeavyShooterChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");

            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                // Shift：两套一起展开
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                // 默认：当前档位（未激活时用装备档做预览）+ 不叠加说明 + 按键提示
                : head + "\n" + (level == 1 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "HeavyShooterChipDynamic", body));
        }
    }
}
