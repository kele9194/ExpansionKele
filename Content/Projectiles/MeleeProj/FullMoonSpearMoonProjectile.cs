using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
	public class FullMoonSpearMoonProjectile : ModProjectile
	{
        private bool damageReduced=false;
		private bool returningToPlayer = false;
		private int returnTimer = 0;
		
		public override void SetDefaults() {
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = true;
			Projectile.penetrate = 1; // 无限穿透
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 180; // 初始时间
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

				public override void AI() {
			// 60帧后开始返回玩家
			if (Projectile.timeLeft <= 120 && !returningToPlayer) {
				returningToPlayer = true;
			}
            if(!damageReduced&&returningToPlayer){
                Projectile.damage = (int)(Projectile.damage *0.6f);
                damageReduced = true;
            }
			
			if (returningToPlayer) {
				// 寻找玩家位置并朝其移动
				Player player = Main.player[Projectile.owner];
				Vector2 direction = player.Center - Projectile.Center;
				float distance = direction.Length();
				
				// 如果接近玩家则销毁
				if (distance < 20f) {
					Projectile.Kill();
					return;
				}
				
				if (distance > 5f) {
					direction.Normalize();
					Projectile.velocity = direction * 20f; // 回归速度
				}
			}
			
			// 添加紫色视觉效果
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 1f);
				dust.noGravity = true;
				dust.velocity *= 0.3f;
			}
		}
		
		public override Color? GetAlpha(Color lightColor) {
			// 将弹幕染为紫色
			return new Color(200, 100, 255, 100);
		}
	}
}