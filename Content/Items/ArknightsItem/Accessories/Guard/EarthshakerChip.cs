using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Materials;
using ExpansionKele.Content.Projectiles.MeleeProj;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 撼地者芯片（近卫系）：近战命中时在目标脚下炸开地裂，波及周围敌人。
    /// <list type="bullet">
    /// <item><b>装备</b>：+10% 近战伤害；每次挥动命中时，在命中点产生半径 120 像素的范围伤害，
    /// 伤害为面板的 50%，<b>不含</b>被近战直接命中的那个目标。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+4% 近战伤害；范围伤害降为面板的 20%，其余不变。</item>
    /// </list>
    /// <para/>
    /// 基础属性给得比其他近卫芯片高一档（+8%），因为范围伤害对单体目标完全无效，
    /// 打 Boss 时这一档就是全部收益。
    /// <para/>
    /// 每次挥动只触发一次：近战一次挥动会对多个目标调用 <see cref="OnHitNPC"/>，
    /// 不做去重的话砍到 3 个敌人会炸出 3 份范围伤害。
    /// </summary>
    public class EarthshakerChip : ModItem
    {
        /// <summary>装备档：常驻近战伤害加成（加算池）。</summary>
        public const float EquippedMeleeBonus = 0.10f;

        /// <summary>收藏档：常驻近战伤害加成（加算池）。</summary>
        public const float FavoritedMeleeBonus = 0.04f;

        /// <summary>装备档：溅射伤害占武器面板的比例。</summary>
        public const float EquippedBlastRatio = 0.5f;

        /// <summary>收藏档：溅射伤害占武器面板的比例。</summary>
        public const float FavoritedBlastRatio = 0.2f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<EarthshakerChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<EarthshakerChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 精金 / 钛金：肉后中后期，群怪密度开始上升，范围伤害价值最高。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddRecipeGroup("ExpansionKele:TertiaryBars", 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            EarthshakerChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>撼地者芯片的状态：一档装备 / 一档收藏。</summary>
    public class EarthshakerChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了撼地者芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有撼地者芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了撼地者芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有撼地者芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>本次挥动是否已经炸过，用于避免多目标命中时重复触发。</summary>
        private bool _triggeredThisSwing;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。</summary>
        public bool IsEquipped => Level >= 3;

        /// <summary>本帧的溅射伤害比例（0 = 不触发）。</summary>
        public float BlastRatio
        {
            get
            {
                int level = Level;
                if (level >= 3)
                    return EarthshakerChip.EquippedBlastRatio;

                if (level >= 1)
                    return EarthshakerChip.FavoritedBlastRatio;

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

        public override void PostUpdate()
        {
            // 用 ItemAnimationJustStarted 而不是 itemAnimation <= 0：
            // 连续挥动时 itemAnimation 可能在同一帧内归零又被重置，
            // PostUpdate 跑在 ItemCheck 之后，读到的已经是新值，会漏掉那个 0。
            if (Player.ItemAnimationJustStarted)
                _triggeredThisSwing = false;
        }

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level <= 0)
                return;

            // 溅射对单体无效，这一档是打 Boss 时的全部收益，所以给得比其他芯片高。
            Player.GetDamage(DamageClass.Melee) += level >= 3
                ? EarthshakerChip.EquippedMeleeBonus
                : EarthshakerChip.FavoritedMeleeBonus;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float ratio = BlastRatio;
            if (ratio <= 0f || _triggeredThisSwing)
                return;

            if (!hit.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Item held = Player.HeldItem;
            if (held.IsAir)
                return;

            int damage = (int)(Player.GetWeaponDamage(held) * ratio);
            if (damage <= 0)
                return;

            _triggeredThisSwing = true;

            Projectile.NewProjectile(
                Player.GetSource_ItemUse(held),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EarthshakerBlastProjectile>(),
                damage,
                0f,
                Player.whoAmI,
                ai0: target.whoAmI);
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.EarthshakerChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<EarthshakerChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "EarthshakerChipDynamic", body));
        }
    }
}
