// using Microsoft.Xna.Framework;
// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;

// namespace ExpansionKele.Content.Customs
// {
//     /// <summary>
//     /// 弹幕速度辅助类。
//     /// InstancePerEntity = true，每个弹幕拥有独立的 speedFactor。
//     /// 外部可通过 projectile.GetGlobalProjectile&lt;ProjectileSpeedHelper&gt;().speedFactor = x 来调整弹幕速度。
//     /// 
//     /// speedFactor 含义：
//     ///   - 1.0f = 正常速度（无效果）
//     ///   - 0.5f = 减速 50%（弹幕移动距离减半）
//     ///   - 0.0f = 完全静止
//     ///   - 大于 1.0f = 加速
//     /// </summary>
//     public class ProjectileSpeedHelper : GlobalProjectile
//     {
//         public override bool InstancePerEntity => true;

//         /// <summary>速度因子，1.0 = 正常速度</summary>
//         public float speedFactor = 1f;

//         public override void AI(Projectile projectile)
//         {
//             if (!projectile.active)
//                 return;

//             if (Main.netMode == NetmodeID.MultiplayerClient)
//                 return;

//             // speedFactor == 1 时无效果，避免不必要的计算
//             if (speedFactor > 0f && speedFactor != 1f)
//             {
//                 Vector2 displacement = projectile.position - projectile.oldPosition;

//                 // 按 speedFactor 缩放位移
//                 Vector2 adjustedDisplacement = displacement * speedFactor;

//                 projectile.position = projectile.oldPosition + adjustedDisplacement;

//                 projectile.netUpdate = true;
//             }
//         }
//     }
// }