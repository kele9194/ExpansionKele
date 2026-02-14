using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
	public class EnhancedLaserProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("强化激光");
		}

		public override void SetDefaults()
		{
            Projectile.CloneDefaults(ProjectileID.GreenLaser);
            Projectile.aiStyle = -1;
			Projectile.penetrate += 3; // 只穿透一次
			Projectile.timeLeft = 360;
			Projectile.light = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
            Projectile.scale=1.5f;
            base.SetDefaults();
		}

		public override void AI()
		{
			// 添加激光粒子效果
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.2f);
				dust.noGravity = true;
				dust.velocity *= 0.2f;
				
				// 添加额外的发光粒子
				Dust glowDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, Color.Cyan, 1f);
				glowDust.noGravity = true;
				glowDust.velocity *= 0.1f;
			}
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;    
            base.AI();

		}
        public override bool OnTileCollide(Vector2 oldVelocity)
		{
				
				// 计算反弹方向
				if (Projectile.velocity.X != oldVelocity.X)
				{
					Projectile.velocity.X = -oldVelocity.X * 0.8f; // 反向并略微减速
				}
				if (Projectile.velocity.Y != oldVelocity.Y)
				{
					Projectile.velocity.Y = -oldVelocity.Y * 0.8f; // 反向并略微减速
				}
                // 每次反弹消耗一次穿透机会
				Projectile.penetrate--;
				// 如果穿透次数用完，则销毁弹幕
				if (Projectile.penetrate <= 0)
				{
					Projectile.Kill();
					return true; // 销毁弹幕
				}
				
				// 播放反弹音效
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.3f }, Projectile.position);
				
				// 反弹时也触发分裂效果
				SplitIntoSecondaryLasers(Projectile.Center);
				

				
				return false; // 不销毁弹幕
			
		}


		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// 命中敌人时触发分裂效果
			SplitIntoSecondaryLasers(target.Center);
		}

		public override void OnKill(int timeLeft)
		{
			// 击中时产生粒子效果
			for (int i = 0; i < 12; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.5f);
				dust.noGravity = true;
				dust.velocity *= 2f;
			}
			
			// 添加紫色能量消散效果
			for (int i = 0; i < 6; i++)
			{
				Dust energyDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, Color.Purple, 1.3f);
				energyDust.noGravity = true;
				energyDust.velocity *= 1.2f;
			}
		}
		private void SplitIntoSecondaryLasers(Vector2 hitPosition)
		{
			// 计算次级激光的伤害（主激光伤害的30%）
			int secondaryDamage = (int)(Projectile.damage * 0.3f);
			
			// 向两个随机方向发射次级激光
			for (int i = 0; i < 2; i++)
			{
				// 生成随机方向
				float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
				Vector2 splitVelocity = Vector2.UnitX.RotatedBy(randomAngle) * 12f; // 速度12
				
				// 创建次级激光弹幕
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					hitPosition,
					splitVelocity,
					ModContent.ProjectileType<EnhancedLaserSplitProjectile>(),
					secondaryDamage,
					Projectile.knockBack * 0.5f,
					Projectile.owner
				);
			}
			
			// 播放音效
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.5f }, hitPosition);
		}
        
        public override Color? GetAlpha(Color lightColor)
		{
			return new Color(0, 255, 0, 150); // 更明亮的绿色，提高可见性
		}

	}
}