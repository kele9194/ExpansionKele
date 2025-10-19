using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExpansionKele.Content.StarySniper
{
    public class LaserDrawer : ModPlayer
    {
        private static List<System.Type> presetWeapons = new List<System.Type>
        {
            typeof(GaSniperA),
            typeof(GaSniperB),
            typeof(GaSniperC),
            typeof(GaSniperD),
            typeof(GaSniperE),
            typeof(GaSniperF),
            typeof(GaSniperG),
            typeof(GaSniperH),
            typeof(GaSniperI),
            typeof(GaSniperJ)
            // 可以在这里添加其他预设的武器类型
        };

        

        public override void PostUpdate()
        {
            
            // 检查玩家是否持有预设的武器
            foreach (var weaponType in presetWeapons)
            {
                if (Player.HeldItem.ModItem?.GetType() == weaponType&&ModContent.GetInstance<ExpansionKeleConfig>().LaserAlwaysOn)
                {
                    
                    // 发射没有伤害的抛射体
                    DrawLaserForWeapon(weaponType);
                    break; // 找到匹配的武器后跳出循环
                }
            }
        }

        public static void DrawLaserForWeapon(System.Type weaponType)
        {
            Player player = Main.LocalPlayer;

            // 检查玩家是否持有指定的武器
            if (player.HeldItem.ModItem?.GetType() == weaponType)
            {
                // 定义抛射体的类型
                int projectileType = ModContent.ProjectileType<NoDamageLaserProjectile>();

                // 定义发射位置和速度
                Vector2 position = player.Center;
                Vector2 velocity = player.DirectionTo(Main.MouseWorld) * 10f; // 根据鼠标位置发射

                // 创建抛射体
                Terraria.Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), position, velocity, projectileType, 0, 0, player.whoAmI);
                
            }
        }
    }

    public class NoDamageLaserProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1; // 穿透所有敌人
            Projectile.tileCollide = false; // 不与地形碰撞
            Projectile.timeLeft = 2; // 持续时间
            Projectile.ignoreWater = true; // 忽略水
        }

        public override void AI()
        {
            // 不改变 velocity 向量
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用 Main.spriteBatch 绘制激光效果
            Texture2D texture = ModContent.Request<Texture2D>("ExpansionKele/Content/StarySniper/SniperLaser").Value;
            Vector2 origin = new Vector2(0, 0);
            float rotation = Projectile.velocity.ToRotation();
            float length = Projectile.velocity.Length() * 5f; // 调整激光长度

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Red * 0.8f,
                rotation,
                origin,
                new Vector2(length, 1f),
                SpriteEffects.None,
                0f
            );

            return false; // 阻止默认绘制
        }

        public override bool? CanDamage()
        {
            return false; // 确保抛射体没有伤害
        }
    }
}