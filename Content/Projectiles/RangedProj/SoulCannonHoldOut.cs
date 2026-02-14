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
using ReLogic.Content;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class SoulCannonHoldOut : ModProjectile
    {
        private Player owner;
        private bool _isShooting = false;
        public float _currentChargingFrames = 0f;

        public float MaxChargeingFrame = 360;

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

        public override string Texture => "ExpansionKele/Content/Projectiles/RangedProj/SoulCannonHoldOut";
        private static Asset<Texture2D> _cachedTexture;

        public override void Load()
        {
            // 预加载纹理资源
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }

        public override void Unload()
        {
            // 清理资源引用
            _cachedTexture = null;
        }

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
            // 初始化AI数组用于存储鼠标位置
            Projectile.ai[0] = 0f; // 鼠标X坐标
            Projectile.ai[1] = 0f; // 鼠标Y坐标
        }

        public override void AI()
        {
            owner = Main.player[Projectile.owner];

            if (owner == null)
            {
                return;
            }

            // 只在拥有者的客户端更新鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 mousePosition = Main.MouseWorld;
                Projectile.ai[0] = mousePosition.X;
                Projectile.ai[1] = mousePosition.Y;
                Projectile.netUpdate = true; // 强制网络同步
            }

            // 使用同步的鼠标位置
            Vector2 targetMousePosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);

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
            Vector2 ownerToMouse = targetMousePosition - armPosition; // 使用同步的鼠标位置
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
                        ShootProjectile(owner, targetMousePosition); // 传递同步的鼠标位置
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

        private void ShootProjectile(Player owner, Vector2 targetMousePosition)
        {
            Vector2 shootVelocity = targetMousePosition - owner.RotatedRelativePoint(owner.MountedCenter, reverseRotation: true); // 使用同步的鼠标位置
            shootVelocity = shootVelocity.SafeNormalize(Vector2.UnitY) * 36f;

            int damage = (int)(Projectile.damage * ChargingMultiplier(_currentChargingFrames, owner, MaxChargeingFrame));
            float knockBack = Projectile.knockBack;

            SoundEngine.PlaySound(SoulCannon.Fire, Projectile.Center);

            Vector2 perturbedSpeed = shootVelocity.RotatedByRandom(MathHelper.ToRadians(0));
            Terraria.Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.RotatedRelativePoint(owner.MountedCenter, reverseRotation: true), perturbedSpeed, ModContent.ProjectileType<SoulCannonProjectile>(), damage, knockBack, owner.whoAmI);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = _cachedTexture.Value;
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
    
    // 使用Asset<Texture2D>数组来存储所有动画帧
    private static Asset<Texture2D>[] _animationFrames;

    public override void Load()
    {
        // 预加载所有动画帧纹理
        _animationFrames = new Asset<Texture2D>[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            string texturePath = $"ExpansionKele/Content/Projectiles/SoulCannonProjAsset/SoulCannonProjectile_{i + 1}";
            _animationFrames[i] = ModContent.Request<Texture2D>(texturePath);
        }
    }

    public override void Unload()
    {
        // 清理所有纹理资源引用
        _animationFrames = null;
    }

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false; // 修改：不与地形碰撞，类似AAMissile
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10; // 修改：增加无敌帧时间
        Projectile.ignoreWater = true; // 修改：忽略水体
        Projectile.netUpdate = true;
    }

    public override void AI()
    {
        // 添加追踪AI - 使用带鼠标位置参数的版本，类似AAMissile
        float maxTrackingDistance =640f; // 640f
        float speed = 30f;
        float turnResistance = 10f;
        Vector2 mousePosition = Main.MouseWorld;
        
        ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed, maxTrackingDistance, turnResistance, mousePosition);
        
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
        if (Projectile.alpha < 255) // 只有在非透明状态下才检测
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5)
                {
                    Rectangle npcHitbox = npc.getRect();
                    Rectangle projHitbox = Projectile.getRect();
                    
                    if (projHitbox.Intersects(npcHitbox))
                    {
                        // 命中处理
                        Projectile.alpha = 255; // 设置为完全透明
                        Projectile.Kill(); // 销毁弹幕
                    }
                }
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // 在AI处处理这些逻辑
        target.immune[Projectile.owner] = 2; // 设置10帧免疫时间
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
        // 使用预加载的动画帧纹理
        if (_animationFrames == null || currentFrame < 0 || currentFrame >= frameCount)
            return false;

        Asset<Texture2D> frameAsset = _animationFrames[currentFrame];
        if (frameAsset?.Value == null)
            return false;

        Texture2D texture = frameAsset.Value;
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