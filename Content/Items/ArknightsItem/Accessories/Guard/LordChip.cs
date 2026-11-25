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
    /// 领主芯片（近卫系）：让近战武器在挥动时额外射出一道剑气，把射程延伸到远处。
    /// <list type="bullet">
    /// <item><b>装备</b>：+5% 近战伤害；近战武器每次挥动朝鼠标方向发射一道剑气，伤害为面板的 30%，
    /// 附带 20 点忽视防御，命中后附加 5 秒「着火了！」，速度 10，不穿透。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+2% 近战伤害；剑气伤害降为面板的 10%，其余不变。</item>
    /// </list>
    /// <para/>
    /// 剑气方向完全由玩家决定（朝鼠标），不做自动索敌：自动瞄准会在打空时剥夺玩家的补救手段，
    /// 而“瞄得准收益高”才是这个芯片应有的操作回报。
    /// <para/>
    /// 触发点是 <see cref="GlobalItem.UseAnimation"/>（<c>Player.cs:42436</c>，只在实际开始挥动的那一帧调用一次），
    /// 因此不会因为挥动动画持续多帧而刷出成倍剑气。
    /// </summary>
    public class LordChip : ModItem
    {
        /// <summary>装备档：常驻近战伤害加成（加算池）。</summary>
        public const float EquippedMeleeBonus = 0.05f;

        /// <summary>收藏档：常驻近战伤害加成（加算池）。</summary>
        public const float FavoritedMeleeBonus = 0.02f;

        /// <summary>装备档：剑气伤害占武器面板的比例。</summary>
        public const float EquippedSlashRatio = 0.3f;

        /// <summary>收藏档：剑气伤害占武器面板的比例。</summary>
        public const float FavoritedSlashRatio = 0.1f;

        /// <summary>剑气飞行速度（像素 / 帧）。</summary>
        public const float SlashSpeed = 14f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(silver: 60);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<LordChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<LordChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 狱石锭：肉前末期，填「蜂王之后、肉山之前」的空档。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.HellstoneBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            LordChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>领主芯片的状态：一档装备 / 一档收藏。</summary>
    public class LordChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了领主芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有领主芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了领主芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有领主芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。</summary>
        public bool IsEquipped => Level >= 3;

        /// <summary>本帧的剑气伤害比例（0 = 不发射）。</summary>
        public float SlashRatio
        {
            get
            {
                int level = Level;
                if (level >= 3)
                    return LordChip.EquippedSlashRatio;

                if (level >= 1)
                    return LordChip.FavoritedSlashRatio;

                return 0f;
            }
        }

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
            if (level <= 0)
                return;

            // 剑气命中率取决于操作，基础属性给一份稳定收益。
            Player.GetDamage(DamageClass.Melee) += level >= 3
                ? LordChip.EquippedMeleeBonus
                : LordChip.FavoritedMeleeBonus;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.LordChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<LordChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "LordChipDynamic", body));
        }
    }
}
