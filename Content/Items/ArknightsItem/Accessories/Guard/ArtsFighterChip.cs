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
    /// 术战者芯片（近卫系）：让近战挥击「法术化」—— 近战武器叠上魔法伤害加成，代价是每次挥动消耗魔力。
    /// <list type="bullet">
    /// <item><b>装备</b>：+5% 魔法伤害、+40 魔力上限；近战武器额外获得你全部的魔法伤害加成；
    /// 每次挥动消耗 5 点魔力，魔力不足或处于沉默时挥不出去。</item>
    /// <item><b>未装备时放于背包并收藏</b>：不提供魔法伤害与魔力上限；
    /// 近战武器只获得 30% 的魔法伤害加成；不消耗魔力。</item>
    /// </list>
    /// <para/>
    /// 为什么不用自定义 DamageClass：<c>Item.melee</c> 是 <c>DamageType</c> 的镜像 getter
    /// （<c>Item.cs:735-741</c>），把 <c>DamageType</c> 改成魔法系会让 <c>melee</c> 立刻为 false，
    /// 挥动动画与 meleeHitbox 整套流程随之失效。所以这里只在 <see cref="ModPlayer.ModifyWeaponDamage"/>
    /// 里改动局部的 <see cref="StatModifier"/>，物品字段一个都不碰。
    /// <para/>
    /// 魔力消耗走原版 <c>Item.mana</c>：<c>Player.ItemCheck_CheckCanUse</c> 判断的是
    /// <c>sItem.mana &gt; 0</c> 而不是 <c>sItem.magic</c>，因此沉默禁用、魔力花自动喝药、
    /// 回蓝延迟全部交给原版处理，不需要自己写扣蓝逻辑。
    /// </summary>
    public class ArtsFighterChip : ModItem
    {
        /// <summary>装备档：魔法伤害加成（加算池）。</summary>
        public const float EquippedMagicDamageBonus = 0.05f;

        /// <summary>装备档：魔力上限加成。</summary>
        public const int EquippedManaBonus = 40;

        /// <summary>
        /// 装备档：近战武器每次挥动消耗的魔力。
        /// 原版会再乘 <c>Player.manaCost</c> 后取整，所以堆满魔力消耗减免时可能被截断为 0。
        /// </summary>
        public const int EquippedManaCost = 5;

        /// <summary>装备档：近战武器叠加魔法加算的比例。</summary>
        public const float EquippedTransferRatio = 1f;

        /// <summary>收藏档：近战武器叠加魔法加算的比例。</summary>
        public const float FavoritedTransferRatio = 0.30f;

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
            player.GetModPlayer<ArtsFighterChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<ArtsFighterChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 陨石锭是魔法主题的经典材料，克苏鲁之眼后可得，把术战者定位在肉前早期。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.MeteoriteBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            ArtsFighterChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>术战者芯片的状态：一档装备 / 一档收藏。</summary>
    public class ArtsFighterChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了术战者芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有术战者芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了术战者芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有术战者芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。</summary>
        public bool IsEquipped => Level >= 3;

        /// <summary>本帧近战武器叠加魔法加算的比例（0 = 不生效）。</summary>
        public float TransferRatio
        {
            get
            {
                int level = Level;
                if (level >= 3)
                    return ArtsFighterChip.EquippedTransferRatio;

                if (level >= 1)
                    return ArtsFighterChip.FavoritedTransferRatio;

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

        public override void PostUpdateEquips()
        {
            int level = Level;

            if (level >= 3)
            {
                Player.GetDamage(DamageClass.Magic) += ArtsFighterChip.EquippedMagicDamageBonus;
                Player.statManaMax2 += ArtsFighterChip.EquippedManaBonus;
            }

            // 只有装备档才让近战武器带魔力消耗。收藏档必须显式写 0，
            // 否则卸下芯片后上一帧留在物品上的 mana 会继续扣蓝。
            Item held = Player.HeldItem;
            if (held.IsAir || held.damage <= 0)
                return;

            if (!held.CountsAsClass(DamageClass.Melee))
                return;

            int mana = level >= 3 ? ArtsFighterChip.EquippedManaCost : 0;
            if (held.mana != mana)
                held.mana = mana;
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            float ratio = TransferRatio;
            if (ratio <= 0f)
                return;

            if (!item.CountsAsClass(DamageClass.Melee))
                return;

            // 取「魔法独有」那一段：GetTotalDamage(Magic) 的 Additive 是 1 + Σ魔法 + Σ通用，
            // 减掉通用后只剩 Σ魔法，既不会把通用加成重复计入，也不去减少近战加成（这里是叠加而非替换）。
            // 魔力病作用在 Multiplicative 上，因此这段叠加不会把它一并带进来。
            StatModifier magicStat = Player.GetTotalDamage(DamageClass.Magic);
            StatModifier genericStat = Player.GetTotalDamage(DamageClass.Generic);
            damage += (magicStat.Additive - genericStat.Additive) * ratio;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.ArtsFighterChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<ArtsFighterChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "ArtsFighterChipDynamic", body));
        }
    }
}
