using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons
{
    public class ProtonCannonA : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        private int chargingCounter; // 蓄力计数器
        private const int maxChargeCounter = 360; // 最大蓄力计数器
        private int floatingCounter;
        private const int burstTime = 10; // 每次射击间隔的 ticks
        private int burstCounter; // 爆炸射击计数器
        private int burstTimer; // 控制射击间隔的计时器
        private const int baseDamage = 100; // 基础伤害
        private bool isCharging; // 是否正在蓄力
        private bool isFullyCharged; // 是否完全蓄力
        private bool shouldFire; // 是否应该发射子弹
        public static int AftershotCooldownFrames = 1;

        public float ChargingMultiplier(int Counter, Player player)
        {
            // 计算蓄力倍率
            float t = (float)(Counter / maxChargeCounter);
            float chargeMultiplier = (float)(Math.Sqrt(48 * t + 16) - 3);

            // 如果在水中，额外增加20%伤害
            if (player.wet)
            {
                chargeMultiplier *= 1.2f;
            }

            return chargeMultiplier;
        }

        public override void SetDefaults()
        {
            Item.SetNameOverride("Proton Cannon A");
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.damage = baseDamage;
            Item.useTime = (base.Item.useAnimation = AftershotCooldownFrames); // 减少使用时间
            //Item.useAnimation = 20; // 减少使用动画时间
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = false; // 禁止自动重复使用
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player)
        {
            // 只有在 shouldFire 为 true 时才允许使用武器
            return shouldFire;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Main.NewText($"shootYesOrNO");
            if (shouldFire)
            {
                Main.NewText($"shootYes");
                // 计算当前的伤害倍率
                float chargeMultiplier = ChargingMultiplier(floatingCounter, player);
                int actualDamage = (int)(Item.damage * chargeMultiplier);
                floatingCounter = 0;

                // 发射子弹
                for (int i = 0; i < 6; i++)
                {
                    Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(10));
                    Terraria.Projectile.NewProjectile(source, position, perturbedSpeed, type, actualDamage, knockback, player.whoAmI);
                }
                Main.NewText($"shootComplete");

                // 重置计时器
                burstCounter = 0;
                burstTimer = 0;

                // 重置蓄力计数器和标志
                chargingCounter = 0;
                isFullyCharged = false;
                shouldFire = false;

                return true; // 消耗物品使用动画
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.controlUseItem)
            {
                isCharging = true;
                chargingCounter += 2;
                if (player.wet)
                {
                    chargingCounter++;
                }
                if (chargingCounter >= maxChargeCounter)
                {
                    chargingCounter = maxChargeCounter;
                    isFullyCharged = true; // 设置完全蓄力标志
                }
                floatingCounter=chargingCounter;
                //Main.NewText($"isCharging:{isCharging}");
            }
            else if (isCharging && (!player.controlUseItem))
            {
                

                isCharging = false;
                shouldFire = true; // 设置发射子弹的标志
                Main.NewText($"shouldFire:{shouldFire}");
            }
        }

        public override void UpdateInventory(Player player)
        {
            // 移除不必要的重置逻辑
        }
    }
}