using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Materials;
using ExpansionKele.Global;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 本源近卫芯片（近卫系）：把造成的伤害的一部分转成目标身上的元素损伤条，
    /// 攒满后引爆一次无视防御的真实伤害，随后该目标短暂免疫损伤累积。
    /// <list type="bullet">
    /// <item><b>装备</b>：+10% 近战伤害；近战命中累积元素损伤 = 实际造成伤害的 25%；
    /// 损伤达到目标最大生命的 2% 时引爆，造成目标最大生命 1.6% 的真实伤害；
    /// <b>引爆后 4 秒内，你的近战伤害全部变为真实伤害</b>（无视防御与一切减伤）。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+4% 近战伤害，累积率 20%、爆发 0.8%，<b>不提供引爆后的真伤期</b>。</item>
    /// <item><b>未装备时放于背包并收藏</b>：累积率 20%、爆发 0.8%，其余不变。</item>
    /// </list>
    /// <para/>
    /// 上限与爆发量都按目标最大生命取比例，所以等效收益
    /// <c>(累积率 × 爆发率 / 上限率)</c> 与目标血量、玩家伤害都无关 ——
    /// 这是选比例而不是固定值的原因。改成固定值后，后期一刀就会溢出很多倍，
    /// 爆发的绝对数值又跟不上武器成长。
    /// <para/>
    /// 真实伤害走 <see cref="NPC.StrikeNPC(NPC.HitInfo, bool, bool)"/> 而不是 <c>SimpleStrikeNPC</c>：
    /// 后者的内部 <c>CalculateHitInfo</c> 会算防御，只有手动构造 <see cref="NPC.HitInfo"/> 才是无视防御。
    /// 代价是这条伤害不会回调 <c>ModifyHitNPC</c> / <c>OnHitNPC</c>（这正是我们要的：不会递归、也不会再叠损伤）。
    /// </summary>
    public class PrimalGuardChip : ModItem
    {
        /// <summary>装备档：常驻近战伤害加成（加算池）。</summary>
        public const float EquippedMeleeBonus = 0.10f;

        /// <summary>收藏档：常驻近战伤害加成（加算池）。</summary>
        public const float FavoritedMeleeBonus = 0.04f;

        /// <summary>装备档：造成伤害转化为元素损伤的比例。</summary>
        public const float EquippedAccumulationRatio = 0.25f;

        /// <summary>装备档：爆发造成的真实伤害占目标最大生命的比例。</summary>
        public const float EquippedBurstRatio = 0.04f;

        /// <summary>收藏档：造成伤害转化为元素损伤的比例。</summary>
        public const float FavoritedAccumulationRatio = 0.20f;

        /// <summary>收藏档：爆发造成的真实伤害占目标最大生命的比例（装备档的一半）。</summary>
        public const float FavoritedBurstRatio = 0.02f;

        /// <summary>损伤条上限占目标最大生命的比例（两档相同）。</summary>
        public const float ThresholdRatio = 0.04f;

        /// <summary>引爆后该目标不再累积损伤的帧数（480 帧 = 8 秒），同时也是真伤期长度。</summary>
        public const int BurstCooldownFrames = 480;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PrimalGuardChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<PrimalGuardChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 四柱后：此时目标防御开始明显压低实际输出，
            // 「按源伤害累积 + 无视防御的爆发」才真正明显强于等量的普通伤害。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.FragmentSolar, 2)
                .AddIngredient(ItemID.FragmentVortex, 2)
                .AddIngredient(ItemID.FragmentNebula, 2)
                .AddIngredient(ItemID.FragmentStardust, 2)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            PrimalGuardChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>本源近卫芯片的状态：一档装备 / 一档收藏。</summary>
    public class PrimalGuardChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了本源近卫芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有本源近卫芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了本源近卫芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有本源近卫芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。</summary>
        public bool IsEquipped => Level >= 3;

        private float AccumulationRatio => IsEquipped
            ? PrimalGuardChip.EquippedAccumulationRatio
            : PrimalGuardChip.FavoritedAccumulationRatio;

        private float BurstRatio => IsEquipped
            ? PrimalGuardChip.EquippedBurstRatio
            : PrimalGuardChip.FavoritedBurstRatio;

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

            Player.GetDamage(DamageClass.Melee) += level >= 3
                ? PrimalGuardChip.EquippedMeleeBonus
                : PrimalGuardChip.FavoritedMeleeBonus;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!item.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Accumulate(target, damageDone, hit.HitDirection);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Accumulate(target, damageDone, hit.HitDirection);
        }

        /// <summary>把一次命中换算出的损伤量灌进目标身上的元素损伤条，并在攒满时引爆。</summary>
        private void Accumulate(NPC target, int basis, int hitDirection)
        {
            if (Level <= 0 || basis <= 0)
                return;

            if (!target.active || target.friendly || target.immortal)
                return;

            PrimalGuardGlobalNPC global = target.GetGlobalNPC<PrimalGuardGlobalNPC>();

            if (global.BurstCooldown > 0)
                return;

            float cap = target.lifeMax * PrimalGuardChip.ThresholdRatio;
            global.ElementDamage += basis * AccumulationRatio;

            if (global.ElementDamage < cap)
                return;

            global.ElementDamage = 0f;
            global.BurstCooldown = PrimalGuardChip.BurstCooldownFrames;
            Burst(target, hitDirection);
        }

        /// <summary>引爆一次元素损伤：造成无视防御的真实伤害。</summary>
        private void Burst(NPC target, int hitDirection)
        {
            int damage = Math.Max(1, (int)(target.lifeMax * BurstRatio));

            NPC.HitInfo hit = new NPC.HitInfo
            {
                Damage = damage,
                SourceDamage = damage,
                HitDirection = hitDirection,
                Knockback = 0f,
                Crit = false,
                DamageType = DamageClass.Generic,
                HideCombatText = true,
            };

            target.StrikeNPC(hit);

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendStrikeNPC(target, hit);

            if (!Main.dedServ)
                CombatText.NewText(target.getRect(), PrimalGuardGlobalNPC.TrueDamageColor, damage, false, false);

            SpawnBurstVisual(target);
        }

        private static void SpawnBurstVisual(NPC target)
        {
            if (Main.dedServ)
                return;

            Vector2 center = target.Center;
            int count = Math.Clamp(target.width + target.height, 8, 24) / 2;

            for (int i = 0; i < count; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    center + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f),
                    DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(3f, 3f),
                    100,
                    new Color(190, 130, 255),
                    1.5f);

                dust.noGravity = true;
            }
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.PrimalGuardChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<PrimalGuardChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "PrimalGuardChipDynamic", body));
        }
    }
}
