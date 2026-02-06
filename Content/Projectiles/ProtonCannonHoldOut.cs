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
using ReLogic.Content;
namespace ExpansionKele.Content.Projectiles
{
    public class ProtonCannonHoldOut : ModProjectile
    {
        // ... existing code ...
        private Player owner;
        private bool _isShooting = false; // 引入状态变量来管理发射状态
        private float _currentChargingFrames; // 引入私有字段

        private float MaxChargeingFrame = 360;

        public float CurrentChargingFrames => _currentChargingFrames;
        public float MaxChargeingFrames=>MaxChargeingFrame;

        public static float MaxChargeFrameValue => 360f;

        private int _shootCounter; // 引入发射计数器
        private int _bulletCount; // 引入子弹计数器
        private const int MaxshootingBullet = 6; // 定义最大发射子弹数
        private const int ShootingInterval = 10; // 定义发射间隔
// ... existing code ...

        private const float xOffset = -3;

        private const float yOffset = -5;

        private const float MaxOffsetLengthFromArm = 40f;
        private  float OffsetLengthFromArm; 
        private const float OffsetXUpwards = 5f; // 根据实际需求调整值
private const float OffsetXDownwards = -0f; // 根据实际需求调整值
private const float BaseOffsetY = 0f; // 根据实际需求调整值
private const float OffsetYUpwards = -0f; // 根据实际需求调整值
private const float OffsetYDownwards = -0f; // 根据实际需求调整值

private CompositeArmStretchAmount FrontArmStretch = CompositeArmStretchAmount.None;
private float ExtraFrontArmRotation = 0f;

private CompositeArmStretchAmount BackArmStretch = CompositeArmStretchAmount.None;
private float ExtraBackArmRotation = 0f;
private bool _isPlayingChargeSound = false; // 新增变量来管理音效播放状态


private bool _hasPlayedMaxChargeSound = false; // 新增变量

        public override string Texture => "ExpansionKele/Content/Projectiles/ProtonCannonHoldOut"; // 确保纹理资源正确
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
            Projectile.friendly = false; // 设置为 true
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 7200;
            Projectile.velocity = Vector2.Zero; // 初始化速度为零
            //Projectile.damage = 50; // 设置伤害
            Projectile.knockBack = 8f; // 设置击退力
            // 初始化AI数组用于存储鼠标位置
            Projectile.ai[0] = 0f; // 鼠标X坐标
            Projectile.ai[1] = 0f; // 鼠标Y坐标
        }

        public override void AI()
        {
            owner = Main.player[Projectile.owner];

            if (owner == null)
            {
                //Main.NewText("Owner is null!");
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

            //Main.NewText($"{!owner.CantUseHoldout()}");
            // 更新蓄力帧数
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
                //Main.NewText($"_currentChargingFrames:{_currentChargingFrames}");
            }
            else
            {
                // 玩家松开攻击键
                if (_currentChargingFrames >= 2 && !_isShooting)
                {
                    _isShooting = true; // 开始发射
                    _shootCounter = ShootingInterval; // 初始化发射计数器
                    _bulletCount = MaxshootingBullet; // 初始化子弹计数器
                }
            }
            

            //播放蓄力音效
            if ((_currentChargingFrames == 2||_currentChargingFrames == 3) && !_isPlayingChargeSound && SoundEngine.IsAudioSupported)
{
    SoundEngine.PlaySound(ProtonCannon.Charge, Projectile.Center);
    _isPlayingChargeSound = true;
}
        else
        {
        

            // 停止播放蓄力音效
            if (_isPlayingChargeSound && SoundEngine.IsAudioSupported&&owner.CantUseHoldout())
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
            Vector2 armOffset = default(Vector2);
            armOffset = new Vector2(
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
            if(_isShooting)
            {
                //Main.NewText($"_shootCounter:{_shootCounter},_bulletCount:{_bulletCount},_isShooting:{_isShooting}");
                
                if(_bulletCount>0){
                    if (_shootCounter > 0)
                    {
                    _shootCounter--;
                    }
                    else
                    {
                    ShootProjectile(owner, targetMousePosition); // 传递同步的鼠标位置
                    _bulletCount--;
                    _shootCounter=ShootingInterval;
                    
                    }
                }
                else{
                    _isShooting=false;
                    Projectile.Kill();
                }
                
            }
        
            
        }

        private void ShootProjectile(Player owner, Vector2 targetMousePosition)
        {
            
                Vector2 shootVelocity = targetMousePosition - owner.RotatedRelativePoint(owner.MountedCenter, reverseRotation: true); // 使用同步的鼠标位置
                shootVelocity = shootVelocity.SafeNormalize(Vector2.UnitY) * 36f;

                int damage = (int)(Projectile.damage * ChargingMultiplier(_currentChargingFrames, owner, MaxChargeingFrame)); // 确保每次发射的子弹都具有蓄力加成后的伤害
                float knockBack = Projectile.knockBack;

                SoundEngine.PlaySound(ProtonCannon.Fire, Projectile.Center);

                Vector2 perturbedSpeed = shootVelocity.RotatedByRandom(MathHelper.ToRadians(0));
                Terraria.Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.RotatedRelativePoint(owner.MountedCenter, reverseRotation: true), perturbedSpeed, ModContent.ProjectileType<ProtonCannonProjectile>(), damage, knockBack, owner.whoAmI);


                // 如果子弹计数器达到最大值，重置状态
               
        }

        // ... existing code ...
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
// ... existing code ...
         public static float ChargingMultiplier(float Counter, Player player,float maxChargeCounter)
        {
            // 计算蓄力倍率
            float t = (float)(Counter / maxChargeCounter);
            float chargeMultiplier = (float)(Math.Sqrt(48 * t + 16) - 3);

            // 如果在水中，额外增加20%伤害
            if (player.wet)
            {
                chargeMultiplier *= 1.2f;
            }

            return chargeMultiplier;
        }
    }

    public class ProtonCannonProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // 自定义AI逻辑
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            target.immune[Projectile.owner] = 4;
            

            
        }
    }
    

    public static class ProtonCannon
    {
        public static readonly int FullChargeFrames = 60; // 1秒
        public static readonly int AftershotCooldownFrames = 30; // 0.5秒
        public static readonly SoundStyle Charge = new SoundStyle("ExpansionKele/Content/Audio/Charge");
        public static readonly SoundStyle Fire = SoundID.Item12;
    }
    

    public static class PlayerExtensions
    {
       public static bool CantUseHoldout(this Player player, bool needsToHold = true)
	{
		if (player != null && player.active && !player.dead && !(!player.channel && needsToHold) && !player.CCed)
		{
           // Main.NewText($"{player != null} {player.active} {!player.dead} {!(!player.channel && needsToHold)} {!player.CCed}");
			return player.noItems;
		}
		return true;
	}
    }
}