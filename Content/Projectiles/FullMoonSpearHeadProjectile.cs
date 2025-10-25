using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles
{
	public class FullMoonSpearHeadProjectile : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.penetrate = -1; // 无限穿透
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 120; 
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
            Projectile.damage=(int)(Projectile.damage*1.2f);
            Projectile.usesLocalNPCImmunity = true; // 使用本地无敌帧
            Projectile.localNPCHitCooldown = 10; 
		}

		public override void AI() {
			// 设置弹幕朝向其移动方向
			// 贴图默认是向左上45度，所以我们需要减去这个角度来校正
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4+2*MathHelper.PiOver4; // 加上PiOver4来补偿贴图初始角度
			
			// 每15帧发射两个追踪弹幕
			if (Projectile.timeLeft % 10 == 0) {
				// 计算垂直于飞行方向的两个方向
				Vector2 perpendicular = Vector2.Normalize(Projectile.velocity).RotatedBy(MathHelper.PiOver2);
				
				// 发射左边的追踪弹幕
				Vector2 spawnPos1 = Projectile.Center + perpendicular * 10f;
				Vector2 velocity1 = perpendicular * 6f; // 垂直方向速度
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					spawnPos1, 
					velocity1, 
					ModContent.ProjectileType<FullMoonSpearMoonProjectile>(), 
					(int)(Projectile.damage *0.75f), 
					Projectile.knockBack / 2, 
					Projectile.owner
				);
				
				// 发射右边的追踪弹幕
				Vector2 spawnPos2 = Projectile.Center - perpendicular * 10f;
				Vector2 velocity2 = -perpendicular * 3f; // 相反方向速度
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					spawnPos2, 
					velocity2, 
					ModContent.ProjectileType<FullMoonSpearMoonProjectile>(), 
					(int)(Projectile.damage *0.75f), 
					Projectile.knockBack / 2, 
					Projectile.owner
				);
			}
			
			// 添加视觉效果
			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 1f);
				dust.noGravity = true;
				dust.velocity *= 0.3f;
			}
		}
	}
}