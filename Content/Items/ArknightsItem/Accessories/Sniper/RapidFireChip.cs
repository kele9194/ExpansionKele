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
    /// 速射手芯片（一级芯片）：贴方舟速射手的定义 —— 优先攻击空中单位。
    /// <list type="bullet">
    /// <item><b>装备</b>：+3% 远程伤害；对<b>空中目标</b>额外 +15% 远程伤害。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+1% 远程伤害；对空中目标额外 +5% 远程伤害。</item>
    /// </list>
    /// <para/>
    /// 一族常量统一放在这里：芯片与模组（<see cref="RapidFireChipModule"/>）共用同一套数值，
    /// 由 <see cref="RapidFireChipPlayer.AerialLevel"/> 等维度属性决定实际生效哪一档。
    /// </summary>
    public class RapidFireChip : ModItem
    {
        /// <summary>本族的本地化键前缀（档位名也定义在这个节点下，模组直接复用）。</summary>
        public const string ChipBaseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.RapidFireChip.";

        /// <summary>装备档（芯片 / 模组通用）：远程伤害加成。</summary>
        public const float EquippedRangedBonus = 0.03f;

        /// <summary>装备档（芯片 / 模组通用）：对空中目标的额外远程伤害。</summary>
        public const float EquippedAerialBonus = 0.15f;

        /// <summary>收藏档（芯片 / 模组通用）：远程伤害加成。</summary>
        public const float FavoritedRangedBonus = 0.01f;

        /// <summary>收藏档（芯片 / 模组通用）：对空中目标的额外远程伤害。</summary>
        public const float FavoritedAerialBonus = 0.05f;

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
            // 实际加成统一由 RapidFireChipPlayer 按档位下发，这里只做“持有”标记。
            player.GetModPlayer<RapidFireChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            // 只有「收藏」才激活背包效果，和本仓库其他符文石先例一致。
            if (Item.favorited)
                player.GetModPlayer<RapidFireChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
                .AddIngredient(ItemID.WoodenArrow, 50)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            RapidFireChipPlayer.AddDynamicTooltips(tooltips, Mod, ChipBaseKey);
        }
    }

    /// <summary>
    /// 速射手芯片一族的状态（芯片与模组共用）。
    /// <para/>
    /// 按<b>效果维度</b>分别取值：同一维度只取最强的来源（装备优先于收藏，同档比等级），
    /// 不同维度各自生效 —— 所以「装备芯片 + 收藏模组」会拿到芯片的装备数值 + 模组的收藏削防。
    /// </summary>
    public class RapidFireChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了速射手芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有速射手芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了速射手芯片模组。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有速射手芯片模组被收藏在背包里。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 加伤维度（远程伤害 + 对空增伤）的最强来源：
        /// 0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int AerialLevel =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>削防维度的最强来源：0 = 无，2 = 收藏模组，4 = 装备模组。</summary>
        public int ModuleLevel =>
            moduleEquipped ? 4 : moduleFavorited ? 2 : 0;

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;
        }

        public override void PostUpdateMiscEffects()
        {
            int aerial = AerialLevel;
            if (aerial == 0)
                return;

            Player.GetDamage(DamageClass.Ranged) += aerial >= 3
                ? RapidFireChip.EquippedRangedBonus
                : RapidFireChip.FavoritedRangedBonus;
        }

        /// <summary>
        /// 往 tooltip 追加「激活提示 + 当前档位效果 + Shift 提示」，按住 Shift 时换成装备 / 收藏两套。
        /// <para/>
        /// 模组与芯片共用这个方法，只是 baseKey 不同。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod, string baseKey)
        {
            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int aerial = Main.LocalPlayer.GetModPlayer<RapidFireChipPlayer>().AerialLevel;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");

            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                // Shift：两套一起展开
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                // 默认：当前档位（未激活时用装备档预览）+ 不叠加说明 + 按键提示
                : head + "\n" + (aerial == 1 || aerial == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "RapidFireChipDynamic", body));

            // 额外一行：混搭（芯片 + 模组 / 装备 + 收藏）时真实生效的是哪一档。
            AddActiveStatusLine(tooltips, mod);
        }

        /// <summary>
        /// 往 tooltip 末尾追加一行「当前生效：XXX」（本地化）。
        /// <para/>
        /// 存在的意义是搭配时的真实结果无法由任何单件物品的 tooltip 描述（例如装备芯片 + 收藏模组），
        /// 玩家不必去推导规则，直接看这行。
        /// </summary>
        public static void AddActiveStatusLine(List<TooltipLine> tooltips, Mod mod)
        {
            int aerial = Main.LocalPlayer.GetModPlayer<RapidFireChipPlayer>().AerialLevel;
            if (aerial == 0)
                return;

            string tierKey = aerial switch
            {
                4 => "TierEquippedModule",
                3 => "TierEquippedChip",
                2 => "TierFavoritedModule",
                _ => "TierFavoritedChip"
            };

            // 档位名统一定义在芯片节点下，模组直接复用同一个键。
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.RapidFireChip.";

            tooltips.Add(new TooltipLine(mod, "RapidFireChipActive",
                Language.GetTextValue(baseKey + "ActiveStatus", Language.GetTextValue(baseKey + tierKey))));
        }
    }
}
