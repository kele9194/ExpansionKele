using System;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class Gatling : ModItem, IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Bullet;
        }

        // ... existing code ...
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<GatlingPlayer>();
            modPlayer.IncrementUseCounter();
            float angle=5f+5f*modPlayer.useCounter*0.05f;

            // 发射第一发子弹（正前方）
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // 发射第二发子弹（随机偏转-5到5度）
            Vector2 velocity2 = velocity.RotatedByRandom(MathHelper.ToRadians(angle));
            Projectile.NewProjectile(source, position, velocity2, type, damage, knockback, player.whoAmI);

            // 发射第三发子弹（随机偏转-5到5度）
            Vector2 velocity3 = velocity.RotatedByRandom(MathHelper.ToRadians(angle));
            Projectile.NewProjectile(source, position, velocity3, type, damage, knockback, player.whoAmI);

            return false; // 我们手动处理了弹幕生成，因此返回false以防止默认行为
        }
// ... existing code ...

        public override float UseSpeedMultiplier(Player player)
        {
            var modPlayer = player.GetModPlayer<GatlingPlayer>();
            // 根据使用次数增加武器速度
            float speedMultiplier = 1f + (modPlayer.useCounter * 0.05f); // 每次使用增加5%的速度
            return speedMultiplier;
        }

        // 添加80%概率不消耗弹药
        public override bool CanConsumeAmmo(Item weapon, Player player)
        {
            // 80%概率不消耗弹药
            return Main.rand.NextFloat() >= 0.8f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SigwutBar>(3)
                .AddIngredient(ItemID.Minishark, 1)
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void HoldItem(Player player)
        {
            var modPlayer = player.GetModPlayer<GatlingPlayer>();
            modPlayer.isHoldingGatling = true;
        }
         public float GetCurrentCharge()
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<GatlingPlayer>();
            return modPlayer.useCounter;
        }

        public float GetMaxCharge()
        {
            return GatlingPlayer.MAX_SPEED_BONUS;
        }
    }

    public class GatlingPlayer : ModPlayer
    {
        // 使用次数计数器
        public int useCounter = 0;
        // 是否正在持有加特林
        public bool isHoldingGatling = false;
        // 上次使用武器的时间（游戏tick）
        private int lastUseTime = 0;
        // 冷却时间（帧）
        private const int COOLDOWN_TIME = 30; // 半秒左右
        // 最大使用时间减少量
        public const int MAX_SPEED_BONUS = 20; // 最多快10倍

        public override void PostUpdate()
        {
            // 只有在持有加特林时才检查冷却
            if (isHoldingGatling)
            {
                // 如果距离上次使用已经过了足够长的时间，则减少使用计数
                if (++lastUseTime >= COOLDOWN_TIME && useCounter > 0)
                {
                    useCounter--;
                    lastUseTime = 0; // 或者可以设为 lastUseTime -= COOLDOWN_TIME 以更精确计算
                }
            }
            else
            {
                // 不持有加特林时重置计数器
                useCounter = 0;
                lastUseTime = 0;
            }
            
            // 重置标志
            isHoldingGatling = false;
        }

        public void IncrementUseCounter()
        {
            useCounter = Math.Min(useCounter + 1, MAX_SPEED_BONUS);
            lastUseTime = 0; // 重置计时器
        }
    }
}