using ExpansionKele.Content.Projectiles.MagicProj;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
	public class EnhancedLaserSplitProjectile : EnhancedLaserProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("分裂激光");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1; // 只穿透一次
			Projectile.timeLeft = 120; // 较短的生命周期
			Projectile.light = 0.5f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
            Projectile.scale=0.5f;
            base.SetDefaults();
		}

		public override void AI()
		{
			// 添加分裂激光的粒子效果（更小更密集）
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 0.8f);
				dust.noGravity = true;
				dust.velocity *= 0.1f;
				
				// 添加淡蓝色发光粒子
				Dust glowDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, Color.LightBlue, 0.6f);
				glowDust.noGravity = true;
				glowDust.velocity *= 0.05f;
			}

			// 设置旋转角度跟随运动方向
			if (Projectile.velocity != Vector2.Zero)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			
			// 逐渐减速效果
			if (Projectile.timeLeft < 120)
			{
				Projectile.velocity *= 0.98f;
			}
		}

        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// 添加激光命中时的粒子效果
			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1f);
			}
		}
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
            modifiers.ArmorPenetration += 15;
		}
        public override bool OnTileCollide(Vector2 oldVelocity)
		{
				
				// 计算反弹方向
				if (Projectile.velocity.X != oldVelocity.X)
				{
					Projectile.velocity.X = -oldVelocity.X * 0.8f; // 反向并减速
				}
				if (Projectile.velocity.Y != oldVelocity.Y)
				{
					Projectile.velocity.Y = -oldVelocity.Y * 0.8f; // 反向并减速
				}
                // 每次反弹减少30点弹幕时间
				Projectile.timeLeft -= 15;
				// 确保时间不会小于0
				if (Projectile.timeLeft < 0)
				{
					Projectile.timeLeft = 0;
				}
				
				// 播放反弹音效
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.6f }, Projectile.position);
				
				// 添加反弹粒子效果

				
				return false; // 不销毁弹幕
		
		}

		public override void OnKill(int timeLeft)
		{
			// 分裂激光消散时的粒子效果
			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1f);
				dust.noGravity = true;
				dust.velocity *= 1.5f;
			}
			
			// 添加少量紫色粒子
			for (int i = 0; i < 3; i++)
			{
				Dust energyDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, Color.Purple, 0.8f);
				energyDust.noGravity = true;
				energyDust.velocity *= 0.8f;
			}
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return new Color(0, 100, 0, 100); // 深暗绿色
		}
	}
}