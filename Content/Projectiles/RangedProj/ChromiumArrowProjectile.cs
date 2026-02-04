using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class ChromiumArrowProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles";

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.2f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
        }

        public override void AI()
        {
            // 箭矢始终朝向移动方向
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            }
            Projectile.velocity.Y += 0.05f;
            
            // 添加简单的尾迹粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ash, 0f, 0f, 100, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 无视所有敌人防御
            
            // 附加切割buff
            target.AddBuff(ModContent.BuffType<SlicingBuff>(), 180); // 3秒持续时间
            
            // 对没有切割buff的敌人造成额外14点伤害
            if (!target.HasBuff(ModContent.BuffType<SlicingBuff>()))
            {
                // 在击中后再次检查是否有buff，因为上面的AddBuff可能还未生效
                // 实际上这里会增加一次额外的14点伤害
                // 我们需要在OnHitNPC之后处理这个额外伤害
            }
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 无视所有敌人防御
            modifiers.ArmorPenetration += 9999; // 通过增加护甲穿透来忽略防御
            modifiers.DefenseEffectiveness *= 0f;
            
            // 检查目标是否已经有切割buff，如果没有则增加额外伤害
            if (!target.HasBuff(ModContent.BuffType<SlicingBuff>()))
            {
                modifiers.FinalDamage *= 1.2f; // 额外14点伤害
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 消失时产生粒子效果
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,  DustID.Ash, 0f, 0f, 100, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }
    }
}