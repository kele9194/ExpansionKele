using System;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
namespace ExpansionKele.Content.Projectiles.MeleeProj{
    public class TangDynastySwordEnergyProjectile : EnergySwordProjectileLinear
    {
        protected override float TextureScaleMultiplier => 1f; // 剑气相对较小

        // 唐朝横刀剑气的颜色定义 - 金黄色调
                // 唐朝横刀剑气的颜色定义 - 绿色调
        protected override Color backDarkColor => new Color(0xB8, 0x86, 0x0B); // 深金色
        protected override Color middleMediumColor => new Color(0xFF, 0xD7, 0x00); // 中等金色
        protected override Color frontLightColor => new Color(0xFF, 0xFF, 0xE0); // 浅金色

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide=false;
            Projectile.penetrate = -1; // 剑气穿透2个目标
            Projectile.timeLeft = 180; // 存活1秒
            Projectile.extraUpdates = 1; // 更平滑的运动
            Projectile.usesLocalNPCImmunity=true;
            Projectile.localNPCHitCooldown=30;
        }

        public override void AI()
        {
            base.AI();
            if(Projectile.damage<1){
                Projectile.Kill();
            }
            
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage=(int)(Projectile.damage*0.75f);
            base.OnHitNPC(target, hit, damageDone);
        }
    }
}