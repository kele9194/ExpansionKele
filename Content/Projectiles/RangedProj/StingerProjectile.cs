using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ExpansionKele.Content.Items.Weapons.Ranged;

namespace ExpansionKele.Content.Projectiles.RangedProj{
 
 public class StingerProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 使用标准子弹AI
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255; // 透明度
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            AIType = ProjectileID.Bullet;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 忽略20点防御
            modifiers.ArmorPenetration += StingerLauncher.ArmorPenetration;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 应用3秒的酸性减益（Venom）
            target.AddBuff(BuffID.Venom, 180); // 3秒 = 180 ticks
        }
    }

}