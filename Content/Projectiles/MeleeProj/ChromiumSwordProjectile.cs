using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles.EnergySword;
using System;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class ChromiumSwordProjectile : EnergySwordProjectile
    {
        protected override float TextureScaleMultiplier => 1f; // 稍微大一点的能量剑

        // 铬的颜色定义 - 铬是一种银白色金属
        // ... existing code ...
        protected override Color backDarkColor => new Color(0x51, 0x51, 0x53); // 最深灰色
        protected override Color middleMediumColor => new Color(0x69, 0x68, 0x72); // 中等灰色
        protected override Color frontLightColor => new Color(0xbd, 0xbc, 0xce); // 最浅灰色
// ... existing code ...

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 18; // 稍微宽一点
            Projectile.height = 18;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            
            // 削减敌人2点防御
            target.defense = Math.Max(0, target.defense - 2);
            
            // 施加切割减益3秒
            target.AddBuff(ModContent.BuffType<Buff.SlicingBuff>(), 180); // 3秒 = 180 ticks
        }
    }
}