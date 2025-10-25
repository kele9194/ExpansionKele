using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles
{
	public class FullMoonSpearProjectile : ModProjectile
	{
		// 定义长矛的最小和最大距离
		protected virtual float HoldoutRangeMin => 50f;
		protected virtual float HoldoutRangeMax => 150f;

		// ... existing code ...
public override void SetDefaults() {
			// Projectile.CloneDefaults(ProjectileID.Spear); // 克隆原版长矛的默认值
			Projectile.width = 36;
            Projectile.height = 36;
            Projectile.aiStyle = 19;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1.3f;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
		}
// ... existing code ...

		public override bool PreAI() {
			Player player = Main.player[Projectile.owner]; // 获取持有玩家实例
			int duration = player.itemAnimationMax; // 定义弹幕存在的持续时间

			player.heldProj = Projectile.whoAmI; // 更新玩家持有的弹幕ID

			// 如果需要，重置弹幕剩余时间
			if (Projectile.timeLeft > duration) {
				Projectile.timeLeft = duration;
			}

			Projectile.velocity = Vector2.Normalize(Projectile.velocity); // 标准化速度向量，用于存储攻击方向

			float halfDuration = duration * 0.5f;
			float progress;

			// 计算动画进度，从0.0到1.0再回到0.0
			if (Projectile.timeLeft < halfDuration) {
				progress = Projectile.timeLeft / halfDuration;
			}
			else {
				progress = (duration - Projectile.timeLeft) / halfDuration;
			}

			// 使用SmoothStep移动弹幕从最小距离到最大距离再返回
			Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);

			// 应用正确的旋转
			if (Projectile.spriteDirection == -1) {
				// 如果精灵面向左边，旋转45度
				Projectile.rotation += MathHelper.ToRadians(45f);
			}
			else {
				// 如果精灵面向右边，旋转135度
				Projectile.rotation += MathHelper.ToRadians(135f);
			}

			return false; // 不执行原版AI
		}
	}
}