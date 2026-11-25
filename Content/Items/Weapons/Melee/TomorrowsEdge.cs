using System.Collections.Generic;
using ExpansionKele.Content.Audio;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Materials;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 开辟明日的剑刃（Dawn-Piercing Blade）。
    /// <para>本体是能量剑；SP 满后右键进入一阶段「塔纳托斯」，一阶段再次致命伤或右键进入二阶段「俄耳甫斯」。</para>
    /// <para>三形态共用同一个物品实例：形态状态全部托管在 <see cref="TomorrowsEdgePlayer"/>，
    /// 本类只负责按形态切换射弹与使用时间，右键切换本身在 <see cref="TomorrowsEdgePlayer"/> 里做
    /// （刻意不用 <c>AltFunctionUse</c>，否则会连带播放一次武器使用动画）。</para>
    /// </summary>
    public class TomorrowsEdge : ModItem, IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        /// <summary>本体基础伤害。</summary>
        public const int BaseDamage = 200;

        /// <summary>本体基础使用时间（帧）。</summary>
        public const int BaseUseTime = 25;

        /// <summary>一阶段攻速倍率（0.8 = 攻速 −20%，出招间隔 25 → 31 帧，换取更高的单次伤害）。</summary>
        public const float ThanatosAttackSpeed = 0.8f;

        /// <summary>一阶段斩击的伤害倍率（在 <c>ThanatosSlash.ModifyHitNPC</c> 里乘）。</summary>
        public const float ThanatosDamageMultiplier = 2.25f;

        /// <summary>一阶段单次挥动最多斩击的目标数。</summary>
        public const int MaxSlashTargets = 4;

        /// <summary>目标不足 <see cref="MaxSlashTargets"/> 时，每个空缺席位分摊到已有目标的概率。</summary>
        public const float MissingSlotFallbackChance = 0.4f;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = false;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = ExpansionKele.ATKTool(BaseDamage);
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6f;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = BaseUseTime;
            Item.useAnimation = BaseUseTime;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shootsEveryUse = true;
            Item.shoot = ModContent.ProjectileType<TomorrowsEdgeSlash>();
            Item.UseSound = ExpansionKeleSounds.EdgeSwing;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

        public override bool CanUseItem(Player player)
        {
            // 二阶段是治疗形态，左键无效。
            return player.GetModPlayer<TomorrowsEdgePlayer>().Stage != TomorrowsEdgeStage.Orpheus;
        }

        public override void UpdateInventory(Player player)
        {
            bool thanatos = player.GetModPlayer<TomorrowsEdgePlayer>().Stage == TomorrowsEdgeStage.Thanatos;

            // 一阶段没有弧光，改为贴怪斩击。
            Item.shoot = thanatos
                ? ModContent.ProjectileType<ThanatosSlash>()
                : ModContent.ProjectileType<TomorrowsEdgeSlash>();
        }

        /// <summary>
        /// 一阶段攻速 ×0.8（出招 25 帧 → 31 帧），用 ×2.25 伤害、破甲与多目标补偿。
        /// 走 UseSpeedMultiplier：框架会同时缩放使用间隔与挥动动画，
        /// 并与饰品攻速叠乘（CombinedHooks.TotalUseSpeedMultiplier）。
        /// </summary>
        public override float UseSpeedMultiplier(Player player)
        {
            return player.GetModPlayer<TomorrowsEdgePlayer>().Stage == TomorrowsEdgeStage.Thanatos
                ? ThanatosAttackSpeed
                : 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var edgePlayer = player.GetModPlayer<TomorrowsEdgePlayer>();

            if (edgePlayer.Stage == TomorrowsEdgeStage.Thanatos)
            {
                // 一阶段：在至多 4 名目标身上直接生成无弹道斩击。
                int slashType = ModContent.ProjectileType<ThanatosSlash>();
                List<NPC> targets = TomorrowsEdgePlayer.FindTargets(player, MaxSlashTargets);

                if (targets.Count == 0)
                {
                    return false;
                }

                // 每个目标先一刀；空缺席位按概率分摊给已有目标，避免单目标场景白白浪费名额。
                List<NPC> strikes = new List<NPC>(targets);
                int missingSlots = MaxSlashTargets - targets.Count;
                for (int i = 0; i < missingSlots; i++)
                {
                    if (Main.rand.NextFloat() < MissingSlotFallbackChance)
                    {
                        strikes.Add(targets[Main.rand.Next(targets.Count)]);
                    }
                }

                foreach (NPC target in strikes)
                {
                    if (!target.active)
                    {
                        continue;
                    }

                    // ai[0] 不参与绘制（斩击朝向固定为 ThanatosSlash.SlashRotation），
                    // 现在改作伤害倍率：一阶段普攻 2.25，替身结束的归宿斩击 6.0。
                    Projectile.NewProjectile(source, target.Center, Vector2.Zero, slashType, damage, knockback, player.whoAmI, ThanatosDamageMultiplier, target.whoAmI);
                }

                return false;
            }

            // 本体：能量剑弧光（不穿墙由 TomorrowsEdgeSlash 的 ownerHitCheck 负责）。
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, Item.useAnimation, player.GetAdjustedItemScale(Item));
            NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
            return false;
        }

        /// <summary>
        /// 充能条：本体显示 SP 进度，替身期间改为显示剩余时间（从满往下掉）。
        /// 返回值单位是<b>秒</b>；两个分支同时除以 60，比例不变。
        /// <see cref="ItemChargeBarElement"/> 只查询本地玩家的手持物，所以直接取 LocalPlayer。
        /// </summary>
        float IChargeableItem.GetCurrentCharge()
        {
            TomorrowsEdgePlayer edge = Main.LocalPlayer.GetModPlayer<TomorrowsEdgePlayer>();
            return (edge.Stage == TomorrowsEdgeStage.Melee ? edge.Sp : edge.SubstituteTimeLeft) / 60f;
        }

        float IChargeableItem.GetMaxCharge()
        {
            TomorrowsEdgePlayer edge = Main.LocalPlayer.GetModPlayer<TomorrowsEdgePlayer>();
            return (edge.Stage == TomorrowsEdgeStage.Melee
                ? TomorrowsEdgePlayer.SpMax
                : TomorrowsEdgePlayer.SubstituteMax) / 60f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<GolemShard>(3)
                .AddIngredient<SigwutBar>(3)
                .AddIngredient<FullMoonBar>(3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
