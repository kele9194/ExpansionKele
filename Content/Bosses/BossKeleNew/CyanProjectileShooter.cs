using System;
using ExpansionKele.Content.Customs;
using InnoVault;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public class CyanProjectileShooter : InvisibleNPCProjectile
    {
        public override string Texture => this.GetRelativeTexturePath("./InvisibleNPCProjectile");
        protected override int MaxLifetime => 100;
        private float shootTimer = 0f;
        private const float ShootInterval = 5f;
        private int currentAngleOffset = -5;
        private int angleDirection = 1;
        public int npcIndex;
        public Phase magicPhase;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        
        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            magicPhase = (Phase)(int)Math.Round(Projectile.ai[1]);

            // 这个发射器完全由服务端驱动：弹幕隐形、不造成伤害，只负责按节奏生成真正的攻击。
            // 客户端若也跑一遍 AI，就会在「服务端同步过来的那份」之外再本地生成一份，
            // 表现为玩家看到双倍弹幕（伤害仍在服务端结算，所以只翻倍视觉不翻倍伤害）。
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }
            
            Phase phase = (Phase)(int)Projectile.ai[1];
            
            shootTimer++;
            Projectile.Center = ownerNPC.Center;
            
            // ... existing code ...
            if (shootTimer >= ShootInterval)
            {
                shootTimer = 0f;
                
                Vector2 targetVelocity = Vector2.Zero;
                if (ownerNPC.HasValidTarget)
                {
                    targetVelocity = ownerNPC.DirectionTo(Main.player[ownerNPC.target].Center);
                }
                else
                {
                    targetVelocity = Vector2.UnitX;
                }
                Projectile.velocity =Vector2.Lerp(Projectile.velocity, targetVelocity, 0.75f);
                
                
                float angleOffset = MathHelper.ToRadians(currentAngleOffset);
                float shootAngle = Projectile.velocity.ToRotation() + angleOffset;
                
                float speed = 20f + currentAngleOffset * 0.4f;
                Vector2 shootVelocity = shootAngle.ToRotationVector2() * speed;
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    shootVelocity,
                    ModContent.ProjectileType<MagicCyanBossProjectile>(),
                    Projectile.damage,
                    0f,
                    Main.myPlayer,
                    ownerNPC.whoAmI,
                    (int)phase
                );
                
                currentAngleOffset += angleDirection;
                if (currentAngleOffset >= 5)
                {
                    angleDirection = -1;
                }
                else if (currentAngleOffset <= -5)
                {
                    angleDirection = 1;
                }
            }
        }
    }
}