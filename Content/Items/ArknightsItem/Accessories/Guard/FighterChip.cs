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
    /// 斗士芯片（近卫系）：以单次伤害换攻击频率，连续命中还能继续加速。
    /// <list type="bullet">
    /// <item><b>装备</b>：近战攻速 <b>+20%</b>、近战伤害 <b>−10%</b>（均走原版加算池）；叠层每层额外 <b>×1.03</b>（乘区，最多 5 层）。</item>
    /// <item><b>未装备时放于背包并收藏</b>：攻速 +10%、伤害 −5%；每层 ×1.01（最多 5 层）。</item>
    /// </list>
    /// <para/>
    /// 基础两项刻意用<b>加算</b>：和原版攻速 / 增伤同池，会被玩家已有的加成稀释，后期不会超模。
    /// 叠层用<b>乘算</b>：它是「叠满才有的奖励」，不应被稀释。
    /// <para/>
    /// 叠层 3 秒内没有新的命中就清空；不区分目标 —— 打任何敌人都能续层，
    /// 所以多部位 Boss 不会像「同目标叠层」那样被反复顶掉。
    /// </summary>
    public class FighterChip : ModItem
    {
        /// <summary>装备档：近战攻速加成（加算池，0.20 = +20%）。</summary>
        public const float EquippedAttackSpeedBonus = 0.20f;

        /// <summary>收藏档：近战攻速加成（加算池）。</summary>
        public const float FavoritedAttackSpeedBonus = 0.10f;

        /// <summary>装备档：近战伤害惩罚（加算池，0.10 = −10%）。</summary>
        public const float EquippedDamagePenalty = 0.10f;

        /// <summary>收藏档：近战伤害惩罚（加算池）。</summary>
        public const float FavoritedDamagePenalty = 0.05f;

        /// <summary>装备档：每层叠加的攻速增量。</summary>
        public const float EquippedStackBonus = 0.03f;

        /// <summary>收藏档：每层叠加的攻速增量。</summary>
        public const float FavoritedStackBonus = 0.01f;

        /// <summary>叠层上限。</summary>
        public const int MaxStacks = 5;

        /// <summary>叠层刷新时长（帧）：3 秒。</summary>
        public const int StackDuration = 180;

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
            player.GetModPlayer<FighterChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<FighterChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 银与钨是互斥世界生成，直接写两条配方，不做 RecipeGroup。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.SilverBar, 5)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.TungstenBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            FighterChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>斗士芯片的状态：一档装备 / 一档收藏 + 叠层。</summary>
    public class FighterChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了斗士芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有斗士芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了斗士芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有斗士芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4），决定用装备那套数值。</summary>
        public bool IsEquipped => Level >= 3;

        /// <summary>当前叠层数（0 ~ <see cref="FighterChip.MaxStacks"/>）。</summary>
        public int Stacks { get; private set; }

        /// <summary>叠层剩余帧数，到 0 时清空。</summary>
        private int _stackTimer;

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;

            // 层数不能在这里清 —— ResetEffects 每帧都跑，会把叠层永远压回 0。
        }

        public override void PostUpdateMiscEffects()
        {
            int level = Level;
            if (level == 0)
                return;

            bool equipped = level >= 3;

            // 基础两项走原版加算池：与玩家已有的攻速 / 增伤同池，后期会被稀释，不会超模。
            Player.GetAttackSpeed(DamageClass.Melee) += equipped
                ? FighterChip.EquippedAttackSpeedBonus
                : FighterChip.FavoritedAttackSpeedBonus;

            Player.GetDamage(DamageClass.Melee) -= equipped
                ? FighterChip.EquippedDamagePenalty
                : FighterChip.FavoritedDamagePenalty;
        }

        public override void PostUpdate()
        {
            if (Level == 0)
            {
                Stacks = 0;
                _stackTimer = 0;
                return;
            }

            if (Stacks == 0)
                return;

            if (--_stackTimer <= 0)
            {
                Stacks = 0;
                _stackTimer = 0;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Level == 0 || !target.active || target.friendly)
                return;

            // 只有近战命中才叠层：魔法 / 召唤 / 远程不应该给斗士攒层。
            if (!hit.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Stacks = System.Math.Min(Stacks + 1, FighterChip.MaxStacks);
            _stackTimer = FighterChip.StackDuration;
        }

        /// <summary>
        /// 叠层的<b>乘算</b>攻速：<c>1 + 每层增量 × 层数</c>。
        /// <para/>
        /// 基础的那 +20% 不在这里 —— 它走 <see cref="PostUpdateMiscEffects"/> 的加算池。
        /// 除数口径：返回值越大，useTime 越小、攻速越快。
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            int level = Level;
            if (level == 0 || Stacks == 0)
                return 1f;

            if (!item.DamageType.CountsAsClass(DamageClass.Melee))
                return 1f;

            float stackBonus = level >= 3
                ? FighterChip.EquippedStackBonus
                : FighterChip.FavoritedStackBonus;

            return 1f + stackBonus * Stacks;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.FighterChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<FighterChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "FighterChipDynamic", body));
        }
    }
}
