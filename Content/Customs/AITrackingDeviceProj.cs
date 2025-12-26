// using Microsoft.Xna.Framework;
// using Terraria;
// using Terraria.ModLoader;

// namespace ExpansionKele.Content.Customs
// {
//     public class AITrackingDeviceProj : GlobalProjectile
//     {
//         private int trackingCooldown = 0;
//         private int timeWithoutHit = 0;

//         public override bool InstancePerEntity => true;

//         public override void AI(Projectile projectile)
//         {

//             if (trackingCooldown > 0)
//             {
//                 trackingCooldown--;
//                 return;
//             }
//             if (projectile.friendly && !projectile.hostile && IsEligibleProjectile(projectile))
//             {
//                 float num = float.MaxValue;
//                 NPC val = null;
//                 NPC[] npc = Main.npc;
//                 foreach (NPC val2 in npc)
//                 {
//                     if (val2.active && !val2.friendly && !val2.dontTakeDamage && !val2.immortal && val2.lifeMax > 5)
//                     {
//                         float num2 = Vector2.Distance(projectile.Center, val2.Center);
//                         if (num2 < num)
//                         {
//                             num = num2;
//                             val = val2;
//                         }
//                     }
//                 }
//                 if (val != null && num < 350f)
//                 {
//                     Vector2 velocity = projectile.velocity;
//                     Vector2 val3 = Utils.SafeNormalize(val.Center - projectile.Center, Vector2.UnitY);
//                     float num3 = Utils.ToRotation(velocity);
//                     float num4 = Utils.ToRotation(val3);
//                     float num5 = MathHelper.ToRadians(9f);
//                     float num6 = Utils.AngleTowards(num3, num4, num5);
//                     projectile.velocity = velocity.Length() * Utils.ToRotationVector2(num6);
//                     timeWithoutHit = 0;
//                 }
//                 else
//                 {
//                     timeWithoutHit++;
//                 }
//             }
//             if (timeWithoutHit >= 90)
//             {
//                 projectile.Kill();
//             }
//             trackingCooldown = 3;
//         }

//         // 替代原代码中的DamageClassUtils.IsEligibleProjectile方法
//         private bool IsEligibleProjectile(Projectile projectile)
//         {
//             // 允许大多数弹幕类型，排除一些特定类型
//             return projectile.type != 0; // 简单实现，可根据需要扩展
//         }
//     }

// public class AIAssistGlobalProjectile : GlobalProjectile
// {
// 	public bool forceNoTileCollide = false;

// 	public override bool InstancePerEntity => true;

// 	public override void OnSpawn(Projectile projectile, IEntitySource source)
// 	{
// 		if (projectile.owner == Main.myPlayer && Main.player[projectile.owner].GetModPlayer<AIAssistPlayer>().assistActive)
// 		{
// 			projectile.tileCollide = false;
// 			forceNoTileCollide = true;
// 		}
// 	}

// 	public override void AI(Projectile projectile)
// 	{
// 		if (forceNoTileCollide)
// 		{
// 			projectile.tileCollide = false;
// 		}
// 	}
// }
// }