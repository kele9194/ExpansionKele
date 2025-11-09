using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
	public class NightFireflyProjectile : ModProjectile
	{
		public override string LocalizationCategory => "Projectiles.MagicProj";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 4; // 可以穿透3个敌人（总共击中4次）
			Projectile.timeLeft = 600;
			Projectile.light = 0.8f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.ArmorPenetration = 40; // 40点护甲穿透
		}

		public override void AI()
        {
            // 添加粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1f);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }

            // 跟踪效果
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 如果发射者处于萤火状态，则调整伤害
            Player player = Main.player[Projectile.owner];
            if (player.HasBuff(ModContent.BuffType<NightFireflyBuff>()))
            {
                NightFireflyPlayer modPlayer = player.GetModPlayer<NightFireflyPlayer>();
                // 计算伤害倍率: 100% + 100% * 2^(计时器/200)
                float damageMultiplier = 1f + 1f * (float)System.Math.Pow(2, modPlayer.nightFireflyTimer / 200f);
                // 限制最大伤害为900%
                if (damageMultiplier > 9f) damageMultiplier = 9f;
                Projectile.damage = (int)(Projectile.originalDamage * damageMultiplier);
            }
        }


		public override void OnKill(int timeLeft)
		{
			// 击中时产生粒子效果
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.5f);
				dust.noGravity = true;
				dust.velocity *= 2f;
			}
		}
	}
}