using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ExpansionKele.Content.Projectiles.RangedProj;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExpansionKele.Content.Projectiles.SummonProj
{
    public class RemoteGatlingSentry : ModProjectile
    {
        private const float MAX_ATTACK_RANGE = 800f;
        private const int TARGET_SEARCH_INTERVAL = 30;
        private const int SHOOT_COOLDOWN_BASE = 10;
        private const int MIN_SHOOT_COOLDOWN = 5;
        private const float SPREAD_ANGLE = 0.08f;
        
        private NPC currentTarget;
        private int targetSearchTimer;
        private int shootCooldown;
        private int fireCounter;
        private float rotation;

        private static Asset<Texture2D> _cachedTexture;
        private static Asset<Texture2D> _cachedTexture2;

        public override void Load()
        {
            // 预加载纹理资源
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
            _cachedTexture2 = ModContent.Request<Texture2D>("ExpansionKele/Content/Projectiles/SummonProj/GroundMount");
        }

        public override void Unload()
        {
            // 清理资源引用
            _cachedTexture = null;
            _cachedTexture2 = null;
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.netImportant = true;




        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false; // Allow this projectile to collide with platforms
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false; // Prevent tile collision from killing the projectile
		}


         public override bool? CanDamage() => false;
        // Don't die on tile collision



         public override void AI()
        {
            
            Player owner = Main.player[Projectile.owner];
            
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            UpdateTarget();
            
            if (currentTarget != null && currentTarget.active)
            {
                AimAtTarget(currentTarget.Center);
                
                if (shootCooldown <= 0)
                {
                    ShootAtTarget(currentTarget.Center);
                    
                    fireCounter++;
                    shootCooldown = Math.Max(MIN_SHOOT_COOLDOWN, SHOOT_COOLDOWN_BASE - fireCounter / 5);
                }
                else
                {
                    shootCooldown--;
                }
            }
            else
            {
                fireCounter = Math.Max(0, fireCounter - 1);
                shootCooldown = SHOOT_COOLDOWN_BASE;
                
                if (Projectile.rotation != 0)
                {
                    Projectile.rotation *= 0.9f;
                }
            }
            
            LightAndVisuals();
        }

        private void UpdateTarget()
        {
            targetSearchTimer++;
            
            // 每 30 帧或者当前目标失效时，重新搜索
            if (targetSearchTimer >= TARGET_SEARCH_INTERVAL || currentTarget == null || !currentTarget.active || !currentTarget.CanBeChasedBy())
            {
                targetSearchTimer = 0;
                currentTarget = FindBestTarget();
            }
            else
            {
                // 如果当前目标还在，但跑出了攻击范围，也放弃目标
                if (Vector2.Distance(currentTarget.Center, Projectile.Center) > MAX_ATTACK_RANGE)
                {
                    currentTarget = null;
                }
            }
        }


        // ... existing code ...
        private NPC FindBestTarget()
        {
            NPC bossTarget = null;
            NPC highHealthTarget = null;
            NPC closestTarget = null;
            
            float highestBossHealth = 0;
            float highestHealth = 0;
            float closestDistance = MAX_ATTACK_RANGE;
            
            foreach (var npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy())
                    continue;
                
                float distance = Vector2.Distance(npc.Center, Projectile.Center);
                
                if (distance > MAX_ATTACK_RANGE)
                    continue;
                
                bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                
                if (!lineOfSight)
                    continue;
                
                bool isBoss = npc.boss ;
                
                if (isBoss)
                {
                    if (npc.lifeMax > highestBossHealth)
                    {
                        highestBossHealth = npc.lifeMax;
                        bossTarget = npc;
                    }
                }
                
                if (npc.lifeMax > highestHealth)
                {
                    highestHealth = npc.lifeMax;
                    highHealthTarget = npc;
                }
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = npc;
                }
            }
            
            if (bossTarget != null)
                return bossTarget;
            
            if (highHealthTarget != null)
                return highHealthTarget;
            
            return closestTarget;
        }
// ... existing code ...

        private void AimAtTarget(Vector2 targetCenter)
        {
            Vector2 direction = targetCenter - Projectile.Center;
            float targetRotation = direction.ToRotation();
            
            //float rotationSpeed = 0.15f;
            // rotation = MathHelper.Lerp(rotation, targetRotation, rotationSpeed);
            // Projectile.rotation = rotation;
            Projectile.rotation = targetRotation;
        }

        private void ShootAtTarget(Vector2 targetCenter)
        {
            Vector2 direction = targetCenter - Projectile.Center;
            direction.Normalize();
            
            float spread = Main.rand.NextFloat(-SPREAD_ANGLE, SPREAD_ANGLE);
            direction = direction.RotatedBy(spread);
            
            Vector2 spawnPosition = Projectile.Center+new Vector2(0,-10) + direction * 35f;
            
            int projType = ModContent.ProjectileType<ChromiumBulletProjectile>();
            int damage = Projectile.damage;
            
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPosition,
                direction * 12f,
                projType,
                damage,
                2f,
                Projectile.owner,
                0f,
                0f
            );
            
            if (fireCounter % 4 == 0)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item11 with { Pitch = -0.2f }, Projectile.Center);
            }
        }

        private void LightAndVisuals()
        {
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.7f, 0.5f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * 2f, 26f, DelegateMethods.CastLight);
        
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Smoke, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 AimingVector=new Vector2(1,0);
            if (_cachedTexture == null|| _cachedTexture2 == null)
                return true;
            if(currentTarget!= null){
                AimingVector = currentTarget.Center - Projectile.Center;
            }

            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            Texture2D tex2 = _cachedTexture2.Value;
            
            int direction = AimingVector.X > 0 ? 1 : -1;

            float rot = direction > 0 ? Projectile.rotation : Projectile.rotation+MathHelper.Pi;
            Vector2 origin =direction > 0 ? new Vector2(tex.Width *0.375f, tex.Height ):new Vector2(tex.Width *0.625f+2, tex.Height );
            SpriteEffects effect = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(tex, 
                Projectile.Center - Main.screenPosition, 
                null, 
                lightColor, 
                rot, 
                origin, 
                Projectile.scale, 
                effect, 
                0);

            

            Main.spriteBatch.Draw(tex2, 
                Projectile.Center - Main.screenPosition+new Vector2(12,20), 
                null, 
                lightColor, 
                0, 
                new Vector2(tex.Width *0.375f, tex.Height ), 
                Projectile.scale, 
                SpriteEffects.None, 
                0);
            
            return false;
        }
    }
}