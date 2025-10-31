using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles
{
    public class FragmentsEmergenceProjectile : ModProjectile
    {
        private float finalRotation = 0f;
        private static int MaxTimeLeft=240;
        private readonly Color[] colors = {
            new Color(128, 0, 128),   // 紫色
            new Color(255, 255, 0),   // 黄色
            new Color(0, 255, 0),     // 绿色
            new Color(0, 0, 255),     // 蓝色
            new Color(255, 0, 0),     // 红色
            new Color(255, 255, 255)  // 白色
        };
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("碎片浮现");
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.alpha = 0;
            Projectile.light = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // 更新最终旋转角度（即使速度为零也保持最后一次有效的角度）
            if (Projectile.velocity != Vector2.Zero)
            {
                finalRotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
            // 设置当前旋转角度
            Projectile.rotation = finalRotation;

            // 如果碎片已经减速到几乎停止，则进入滞留状态
            if (Projectile.velocity.Length() < 0.1f)
            {
                // 进入滞留状态，不再移动
                Projectile.velocity = Vector2.Zero;
                
                // 增加伤害（2倍）
                Projectile.damage = (int)(Projectile.originalDamage * 2f);
                
                // 添加粒子效果
            }
            else
            {
                // 减速过程
                Projectile.velocity *= 0.95f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 击中敌人时产生粒子效果
            // for (int i = 0; i < 10; i++)
            // {
            //     Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default(Color), 1.5f);
            //     dust.noGravity = true;
            //     dust.velocity *= 2f;
            // }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 与物块碰撞时产生粒子效果
            // for (int i = 0; i < 10; i++)
            // {
            //     Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default(Color), 1.5f);
            //     dust.noGravity = true;
            //     dust.velocity *= 2f;
            // }
            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // 每个颜色持续20帧（120帧/6个颜色）
            int cycleFrames = 120; // 总循环帧数
            int framesPerColor = cycleFrames/2; // 每个颜色的帧数
            
            // 计算当前循环帧数
            int currentFrame = (MaxTimeLeft - Projectile.timeLeft) % cycleFrames;
            
            // 计算当前颜色索引和下一个颜色索引
            int currentColorIndex = (currentFrame / framesPerColor) % colors.Length;
            int nextColorIndex = (currentColorIndex + 1) % colors.Length;
            
            // 计算颜色过渡因子（0到1之间）
            float transitionFactor = (float)(currentFrame % framesPerColor) / framesPerColor;
            
            // 获取当前颜色和下一个颜色
            Color currentColor = colors[currentColorIndex];
            Color nextColor = colors[nextColorIndex];
            
            // 在两种颜色之间进行线性插值
            Color interpolatedColor = Color.Lerp(currentColor, nextColor, transitionFactor);
            
            // 根据速度改变颜色亮度
            if (Projectile.velocity.Length() < 0.1f)
            {
                // 滞留状态时发出更亮的光
                return interpolatedColor * 1.5f;
            }
            
            return interpolatedColor;
        }
    }
}