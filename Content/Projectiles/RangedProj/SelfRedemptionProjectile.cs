using ExpansionKele.Content.Items.OtherItem;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
	public class SelfRedemptionProjectile : ModProjectile
	{
		public override string LocalizationCategory => "Projectiles";

		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Fade Projectile");
		}

		public override void SetDefaults() {
			Projectile.width = 4;
			Projectile.height = 8;
			Projectile.aiStyle = 0; // 无AI样式
			Projectile.friendly = true; // 友方弹幕
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged; // 远程伤害类型
			Projectile.penetrate = 3; // 穿透2个敌人
			Projectile.timeLeft = 600; // 存活时间
			Projectile.alpha = 0; // 透明度
			Projectile.ignoreWater = true; // 忽略液体
			Projectile.tileCollide = true; // 可以与物块碰撞
			Projectile.extraUpdates = 8; // 额外更新
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
		}
		
		public override void AI() {

			Projectile.rotation=Projectile.velocity.ToRotation()+MathHelper.PiOver2;
			// 添加光效
			Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.5f);
		}
		
		public override void OnKill(int timeLeft) {
			// 死亡时产生粒子效果
			for (int i = 0; i < 5; i++) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default(Color), 1.5f);
			}
		}
		

       // ... existing code ...
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// 当击中NPC且将其击杀时，掉落救赎碎片
			if (target.life <= 0) {
				// 每次击杀掉落10-20个救赎碎片
				int itemCount = Main.rand.Next(4, 7); // Next方法的上限是排他的，所以要写21才能得到最大20
				Item.NewItem(Projectile.GetSource_OnHit(target), target.getRect(), ModContent.ItemType<RedemptionShard>(), itemCount);
			}
			Projectile.damage = (int)(Projectile.damage * 0.7f);
		}
// ... existing code ...
	}
}