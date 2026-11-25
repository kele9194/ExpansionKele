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
    /// 教官芯片（近卫系，鞭子向）：鞭子本身是近战的一种，所以归在近卫里。
    /// <list type="bullet">
    /// <item><b>装备</b>：+5% 召唤伤害、+5% 鞭子攻速、+5% 鞭子范围；
    /// 用非召唤攻击（含鞭子）命中敌人后，敌人身上留下 5 秒标记，
    /// 期间所有召唤类弹幕对它造成的伤害 ×1.15。</item>
    /// <item><b>未装备时放于背包并收藏</b>：只有 +5% 召唤伤害；标记倍率降为 ×1.05。</item>
    /// </list>
    /// <para/>
    /// 标记挂在敌人身上（<see cref="ExpansionKele.Global.InstructorChipGlobalNPC"/>），
    /// 而不是玩家身上：这样“谁打的那只怪”和“谁受益”是同一个目标，玩家看得见、也能主动选。
    /// <para/>
    /// 鞭子攻速走 <c>GetAttackSpeed(DamageClass.Summon)</c>（<see cref="SummonMeleeSpeedDamageClass"/>
    /// 对 Summon 继承 Full、含攻速），鞭子范围走原版字段 <c>Player.whipRangeMultiplier</c>。
    /// </summary>
    public class InstructorChip : ModItem
    {
        /// <summary>召唤伤害加成，装备与收藏共用同一个值。</summary>
        public const float SummonDamageBonus = 0.05f;

        /// <summary>装备档：鞭子攻速加成。</summary>
        public const float WhipSpeedBonus = 0.05f;

        /// <summary>装备档：鞭子范围加成。</summary>
        public const float WhipRangeBonus = 0.05f;

        /// <summary>装备档：标记期间的召唤伤害倍率。</summary>
        public const float EquippedBuffMultiplier = 1.15f;

        /// <summary>收藏档：标记期间的召唤伤害倍率。</summary>
        public const float FavoritedBuffMultiplier = 1.05f;

        /// <summary>标记持续时间（帧）：5 秒。重复触发只刷新，不叠加。</summary>
        public const int MarkDuration = 300;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 70);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InstructorChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<InstructorChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 肉后中期：近卫基础芯片 + 二级锭（秘银 / 山铜），组由 Content/ExpansionKeleRecipe.cs 注册。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddRecipeGroup("ExpansionKele:SecondaryBars", 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            InstructorChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>教官芯片的状态：一档装备 / 一档收藏。</summary>
    public class InstructorChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了教官芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有教官芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了教官芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有教官芯片模组被收藏（预留）。</summary>
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

            // 召唤伤害：两档都吃。
            Player.GetDamage(DamageClass.Summon) += InstructorChip.SummonDamageBonus;

            // 鞭子攻速 / 范围：只有装备档。
            if (level < 3)
                return;

            Player.GetAttackSpeed(DamageClass.Summon) += InstructorChip.WhipSpeedBonus;
            Player.whipRangeMultiplier += InstructorChip.WhipRangeBonus;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.InstructorChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<InstructorChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "InstructorChipDynamic", body));
        }
    }
}
