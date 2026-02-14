using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.SummonProj
{
    /// <summary>
    /// SpaceFunnel光束 - 浮游炮发射的绿色追踪光束
    /// 具有无限穿透能力和追踪效果
    /// </summary>
    public class SpaceFunnelBeam : ModProjectile
    {
        private const int HOMING_DELAY = 15; // 追踪延迟帧数
        private const float HOMING_STRENGTH = 0.15f; // 追踪强度
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("空间光束");
            
            // 设置为召唤物射击物
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1; // 自定义AI
            Projectile.friendly = true; // 友善弹幕
            Projectile.hostile = false; // 非敌对
            Projectile.DamageType = DamageClass.Summon; // 召唤伤害类型
            Projectile.penetrate = 3; // 无限穿透
            Projectile.timeLeft = 180; // 存在时间：3秒
            Projectile.ignoreWater = true; // 忽略水
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 使用本地NPC无敌帧
            Projectile.localNPCHitCooldown = 15; // 本地NPC击中冷却
            Projectile.scale = 1.2f; // 稍大的尺寸
        }

        public override void AI()
        {
            // 添加光束粒子效果
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.GreenTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
                
                // 添加额外的发光粒子
                Dust glowDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.MagicMirror, 0f, 0f, 100, Color.Lime, 1.2f);
                glowDust.noGravity = true;
                glowDust.velocity *= 0.2f;
            }
            
            
            // 设置旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 逐渐增加亮度
            if (Projectile.timeLeft > 150)
            {
                Projectile.scale = 1.2f + (180 - Projectile.timeLeft) * 0.01f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中时添加额外效果
            target.AddBuff(BuffID.CursedInferno, 180); // 添加混乱debuff
            
            // 播放命中音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.5f }, Projectile.Center);
        }

        public override void OnKill(int timeLeft)
        {
            // 击中时产生粒子效果
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.GreenTorch, 0f, 0f, 100, default, 1.8f);
                dust.noGravity = true;
                dust.velocity *= 2.5f;
            }
            
            // 添加绿色能量消散效果
            for (int i = 0; i < 4; i++)
            {
                Dust energyDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.MagicMirror, 0f, 0f, 100, Color.LimeGreen, 1.5f);
                energyDust.noGravity = true;
                energyDust.velocity *= 1.5f;
            }
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(0, 255, 100, 180); // 明亮的绿色，提高可见性
        }
    }
}