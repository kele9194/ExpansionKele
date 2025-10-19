using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using System;
using static Terraria.Player;
using Terraria.ID;
using Terraria.DataStructures;
using System.Collections.Generic;
using System.Linq;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles
{
    public class SoulCannonHoldOut : ModProjectile
    {
        private Player owner;
        private bool _isShooting = false;
        private float _currentChargingFrames = 0f;

        private float MaxChargeingFrame = 360;

        private int _shootCounter;
        private int _bulletCount;
        private const int MaxshootingBullet = 6;
        private const int ShootingInterval = 12;

        private const float xOffset = -3;
        private const float yOffset = -5;
        private const float MaxOffsetLengthFromArm = 40f;
        private float OffsetLengthFromArm;
        private const float OffsetXUpwards = 5f;
        private const float OffsetXDownwards = -0f;
        private const float BaseOffsetY = 0f;
        private const float OffsetYUpwards = -0f;
        private const float OffsetYDownwards = -0f;

        private CompositeArmStretchAmount FrontArmStretch = CompositeArmStretchAmount.None;
        private float ExtraFrontArmRotation = 0f;

        private CompositeArmStretchAmount BackArmStretch = CompositeArmStretchAmount.None;
        private float ExtraBackArmRotation = 0f;
        private bool _isPlayingChargeSound = false;
        private bool _hasPlayedMaxChargeSound = false;

        public override string Texture => "ExpansionKele/Content/Projectiles/SoulCannonHoldOut";

        public override void OnSpawn(IEntitySource source)
        {
            OffsetLengthFromArm = MaxOffsetLengthFromArm;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 7200;
            Projectile.velocity = Vector2.Zero;
            Projectile.knockBack = 8f;
        }

        public override void AI()
        {
            owner = Main.player[Projectile.owner];

            if (owner == null)
            {
                return;
            }

            if (!owner.CantUseHoldout())
            {
                _currentChargingFrames += owner.wet ? 3f : 2f;
                if (_currentChargingFrames >= MaxChargeingFrame)
                {
                    _currentChargingFrames = MaxChargeingFrame;
                    if (!_hasPlayedMaxChargeSound)
                    {
                        SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                        _hasPlayedMaxChargeSound = true;
                    }
                }
            }
            else
            {
                if (_currentChargingFrames >= 2 && !_isShooting)
                {
                    _isShooting = true;
                    _shootCounter = ShootingInterval;
                    _bulletCount = MaxshootingBullet;
                }
            }

            if ((_currentChargingFrames == 2 || _currentChargingFrames == 3) && !_isPlayingChargeSound && SoundEngine.IsAudioSupported)
            {
                SoundEngine.PlaySound(SoulCannon.Charge, Projectile.Center);
                _isPlayingChargeSound = true;
            }
            else
            {
                if (_isPlayingChargeSound && SoundEngine.IsAudioSupported && owner.CantUseHoldout())
                {
                    SoundEngine.StopTrackedSounds();
                    _isPlayingChargeSound = false;
                }
            }

            Player player = Main.player[Projectile.owner];
            Vector2 armPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
            Vector2 ownerToMouse = Main.MouseWorld - armPosition;
            float holdoutDirection = Projectile.velocity.ToRotation();
            float proximityLookingUpwards = Vector2.Dot(ownerToMouse.SafeNormalize(Vector2.Zero), -Vector2.UnitY * player.gravDir);
            int direction = MathF.Sign(ownerToMouse.X);
            Vector2 lengthOffset = Projectile.rotation.ToRotationVector2() * OffsetLengthFromArm;
            Vector2 armOffset = new Vector2(
                Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, (proximityLookingUpwards > 0f) ? OffsetXUpwards : OffsetXDownwards) * (float)direction,
                BaseOffsetY * player.gravDir + Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, (proximityLookingUpwards > 0f) ? OffsetYUpwards : OffsetYDownwards) * player.gravDir
            );
            Projectile.Center = armPosition + lengthOffset + armOffset;
            Projectile.velocity = holdoutDirection.AngleTowards(ownerToMouse.ToRotation(), 0.2f).ToRotationVector2();
            Projectile.rotation = holdoutDirection;
            Projectile.spriteDirection = direction;

            player.ChangeDir(direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = (player.itemAnimation = 2);
            player.itemRotation = (Projectile.velocity * (float)Projectile.direction).ToRotation();

            float armRotation = (Projectile.rotation - (float)Math.PI / 2f) * player.gravDir + ((player.gravDir == -1f) ? ((float)Math.PI) : 0f);
            player.SetCompositeArmFront(enabled: true, FrontArmStretch, armRotation + ExtraFrontArmRotation * (float)direction);
            player.SetCompositeArmBack(enabled: true, BackArmStretch, armRotation + ExtraBackArmRotation * (float)direction);

            if (_isShooting)
            {
                if (_bulletCount > 0)
                {
                    if (_shootCounter > 0)
                    {
                        _shootCounter--;
                    }
                    else
                    {
                        ShootProjectile(owner);
                        _bulletCount--;
                        _shootCounter = ShootingInterval;
                    }
                }
                else
                {
                    _isShooting = false;
                    Projectile.Kill();
                }
            }
        }

        private void ShootProjectile(Player owner)
        {
            Vector2 shootVelocity = Main.MouseWorld - owner.RotatedRelativePoint(owner.MountedCenter, reverseRotation: true);
            shootVelocity = shootVelocity.SafeNormalize(Vector2.UnitY) * 36f;

            int damage = (int)(Projectile.damage * ChargingMultiplier(_currentChargingFrames, owner, MaxChargeingFrame));
            float knockBack = Projectile.knockBack;

            SoundEngine.PlaySound(SoulCannon.Fire, Projectile.Center);

            Vector2 perturbedSpeed = shootVelocity.RotatedByRandom(MathHelper.ToRadians(0));
            Terraria.Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.RotatedRelativePoint(owner.MountedCenter, reverseRotation: true), perturbedSpeed, ModContent.ProjectileType<SoulCannonProjectile>(), damage, knockBack, owner.whoAmI);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + ((Projectile.spriteDirection == -1) ? MathHelper.Pi : 0f);
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = (SpriteEffects)((Projectile.spriteDirection * owner.gravDir == -1f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            if (!owner.CantUseHoldout())
            {
                float rumble = MathHelper.Clamp(_currentChargingFrames, 0f, SoulCannon.FullChargeFrames);
                drawPosition += Main.rand.NextVector2Circular(rumble / 30f, rumble / 30f);
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, origin, Projectile.scale * owner.gravDir, effects);
            return false;
        }

        public static float ChargingMultiplier(float Counter, Player player, float maxChargeCounter)
        {
            float t = (float)(Counter / maxChargeCounter);
            float chargeMultiplier = (float)(Math.Sqrt(48 * t + 16) - 3);

            if (player.wet)
            {
                chargeMultiplier *= 1.2f;
            }

            return chargeMultiplier;
        }
    }

    public class SoulCannonProjectile : ModProjectile
    {
        private int frameCounter;
        private int currentFrame;
        private const int frameCount = 3; // 3帧动画
        private const int frameDelay = 5;  // 每5帧切换一次动画

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1; // 多穿透效果
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5; // 5的局部无敌帧
        }

        public override void AI()
        {
            // 添加追踪AI
            ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 30f, 640f, 10f);
            
            // 旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 动画逻辑
            frameCounter++;
            if (frameCounter >= frameDelay)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= frameCount)
                {
                    currentFrame = 0;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.immune[Projectile.owner] = 0;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 计算64像素范围内的敌人数
            float searchRadius = 64f;
            int enemyCount = 0;
            
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5 && Vector2.Distance(Projectile.Center, npc.Center) <= searchRadius)
                {
                    enemyCount++;
                }
            }
            
            // 每多存在一个生物额外造成12.5%伤害，最高提升50%
            float damageMultiplier = 1f + Math.Min(enemyCount * 0.125f, 0.5f);
            modifiers.FinalDamage *= damageMultiplier;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用特定帧绘制投射物
            string texturePath = "ExpansionKele/Content/Projectiles/SoulCannonProjAsset/SoulCannonProjectile";
            if (currentFrame >= 0)
            {
                texturePath += "_" + (currentFrame+1); // 从SoulCannonProjectile_1.png开始
            }
            
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, 1, 0, 0);
            Vector2 origin = frame.Size() / 2f;
            Color drawColor = Projectile.GetAlpha(lightColor);
            
            Main.EntitySpriteDraw(texture, drawPosition, frame, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }

    public static class SoulCannon
    {
        public static readonly int FullChargeFrames = 88;
        public static readonly int AftershotCooldownFrames = 12;
        public static readonly SoundStyle Charge = new SoundStyle("ExpansionKele/Content/Audio/Charge");
        public static readonly SoundStyle Fire = SoundID.Item12;
    }
}