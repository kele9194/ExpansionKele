using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 近战挥舞武器射弹的抽象基类
    /// 提供通用的挥舞动画、绘制和碰撞检测逻辑
    /// </summary>
    public abstract class BaseSwordHeldProjectile : ModProjectile
    {
        /// <summary>
        /// 缓存的纹理资源
        /// </summary>
        protected static Asset<Texture2D> _cachedTexture;
        
        /// <summary>
        /// 挥舞计数器
        /// </summary>
        protected float counter = 0;
        
        /// <summary>
        /// 缩放系数
        /// </summary>
        protected float scale = 1f;
        
        /// <summary>
        /// 透明度值
        /// </summary>
        protected float alpha = 1f;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        protected bool init = true;
        
        /// <summary>
        /// 获取自定义纹理路径（由子类实现）
        /// </summary>
        public abstract override string Texture { get; }
        
        /// <summary>
        /// 获取武器长度系数（由子类实现，用于碰撞检测）
        /// </summary>
        protected virtual float WeaponLengthMultiplier => (float)Math.Sqrt(2);
        
        /// <summary>
        /// 获取挥舞角度调节系数（由子类实现）
        /// </summary>
        protected virtual float SwingAngleModifier => 0.75f;
        
        /// <summary>
        /// 获取碰撞检测宽度（由子类实现）
        /// </summary>
        protected virtual float CollisionWidth => 6f;
        
        /// <summary>
        /// 获取割草宽度（由子类实现）
        /// </summary>
        protected virtual float CutTilesWidth => 54f;

        public override void Load()
        {
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }
        
        public override void Unload()
        {
            _cachedTexture = null;
        }
        
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            float maxUpdateTimes = owner.itemTimeMax * Projectile.MaxUpdates;
            float progress = counter / maxUpdateTimes;
            
            counter++;
            
            if (init)
            {
                Projectile.scale *= owner.HeldItem.scale;
                init = false;
            }
            
            Projectile.timeLeft = 3;
            alpha = GetAlpha(progress);
            scale = GetScale(progress);
            
            float swingAngle = CalculateSwingAngle(progress);
            Projectile.rotation = Projectile.velocity.ToRotation() + swingAngle * Projectile.ai[0];
            Projectile.Center = owner.MountedCenter;
            
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 
                Projectile.rotation - (float)(Math.PI * 0.5f));
            
            UpdatePlayerItemTime(owner, maxUpdateTimes);
            
            OnAIUpdate(owner, progress);
        }
        
        /// <summary>
        /// 计算挥舞角度（使用余弦函数）
        /// </summary>
        protected virtual float CalculateSwingAngle(float progress)
        {
            float cosValue = -(float)Math.Cos(progress * Math.PI);
            return MathHelper.PiOver2 * cosValue * SwingAngleModifier;
        }
        
        /// <summary>
        /// 获取透明度值（可由子类重写）
        /// </summary>
        protected virtual float GetAlpha(float progress) => 1f;
        
        /// <summary>
        /// 获取缩放系数（可由子类重写）
        /// </summary>
        protected virtual float GetScale(float progress) => 1f;
        
        /// <summary>
        /// 更新玩家的物品时间
        /// </summary>
        protected virtual void UpdatePlayerItemTime(Player owner, float maxUpdateTimes)
        {
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            
            if (counter > maxUpdateTimes)
            {
                owner.itemTime = 1;
                owner.itemAnimation = 1;
                Projectile.Kill();
            }
        }
        
        /// <summary>
        /// AI 更新时的额外逻辑（由子类实现）
        /// </summary>
        protected virtual void OnAIUpdate(Player owner, float progress)
        {
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            int direction = (int)(Projectile.ai[0]);
            
            float rot = GetDrawingRotation(direction);
            Vector2 origin = GetDrawingOrigin(direction, tex);
            SpriteEffects effect = GetSpriteEffects(direction);
            
            Vector2 drawPosition = Projectile.Center + owner.gfxOffY * Vector2.UnitY - Main.screenPosition;
            
            Main.EntitySpriteDraw(tex, drawPosition, null, 
                lightColor * alpha, rot, origin, 
                Projectile.scale * scale, effect);
            
            return false;
        }
        
        /// <summary>
        /// 获取绘制旋转角度
        /// </summary>
        protected virtual float GetDrawingRotation(int direction)
        {
            return direction > 0 ? 
                Projectile.rotation + MathHelper.PiOver4 : 
                Projectile.rotation + MathHelper.Pi * 0.75f;
        }
        
        /// <summary>
        /// 获取绘制原点
        /// </summary>
        protected virtual Vector2 GetDrawingOrigin(int direction, Texture2D tex)
        {
            return direction > 0 ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);
        }
        
        /// <summary>
        /// 获取精灵效果
        /// </summary>
        protected virtual SpriteEffects GetSpriteEffects(int direction)
        {
            return direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            
            float weaponLength = tex.Width * WeaponLengthMultiplier * Projectile.scale * scale;
            Vector2 bladeTip = Projectile.Center + Projectile.rotation.ToRotationVector2() * weaponLength;
            
            float collisionPoint = 0;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), 
                targetHitbox.Size(), 
                Projectile.Center, 
                bladeTip, 
                CollisionWidth, 
                ref collisionPoint))
            {
                return true;
            }
            
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void CutTiles()
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            
            float weaponLength = tex.Width * WeaponLengthMultiplier * Projectile.scale * scale;
            Utils.PlotTileLine(Projectile.Center, 
                Projectile.Center + Projectile.rotation.ToRotationVector2() * weaponLength, 
                CutTilesWidth, 
                DelegateMethods.CutTiles);
        }
        
        /// <summary>
        /// 获取武器纹理（安全访问）
        /// </summary>
        protected Texture2D GetTexture()
        {
            return _cachedTexture?.Value;
        }
        
        /// <summary>
        /// 计算武器尖端位置
        /// </summary>
        protected Vector2 CalculateWeaponTip(Player owner, float additionalScale = 1f)
        {
            Texture2D tex = GetTexture();
            if (tex == null)
                return Projectile.Center;
                
            float weaponLength = tex.Width * WeaponLengthMultiplier * Projectile.scale * scale * additionalScale;
            return Projectile.Center + Projectile.rotation.ToRotationVector2() * weaponLength;
        }
    }
}