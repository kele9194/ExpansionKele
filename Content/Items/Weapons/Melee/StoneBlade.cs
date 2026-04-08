using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class StoneBlade : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Melee";

        public static int Rotation =1;
        public override void SetStaticDefaults()
        {
            
        }
        public override bool MeleePrefix()
        {
            return true;
        }

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.knockBack = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<StoneBladeHeld>();
            Item.value=ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.shootSpeed = 10;
            Item.scale*=1;
            Item.channel = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI,Rotation);
            Rotation *= -1;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.StoneBlock, 50)
            .AddIngredient(ItemID.Wood, 10)
            .Register();
        }
    }

    public class StoneBladeHeld : ModProjectile{
        public override string Texture => this.GetRelativeTexturePath("./StoneBlade");
        private static Asset<Texture2D> _cachedTexture;
        public float counter = 0;
        public float scale = 1;
        public float alpha = 0;
        public bool init = true;
        public bool shoot = true;
        
        public override void Load()
        {
            // 预加载纹理
            _cachedTexture=ModContent.Request<Texture2D>(Texture);

        }
        
        public override void Unload()
        {
            // 可选：清空引用
            _cachedTexture = null;
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
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
            Projectile.hide=true;
        }
        public override void AI(){
            
            //考虑未来是否需要换成更安全的函数
            Player owner = Main.player[Projectile.owner];
            //获得玩家在更新数考虑的情况下的总帧数
            float MaxUpdateTimes = owner.itemTimeMax * Projectile.MaxUpdates;
            //获取挥舞进度
            float progress = (counter / MaxUpdateTimes);
            counter++;
            if (init)
            {
                Projectile.scale *= owner.HeldItem.scale;
                init = false;
            }
            Projectile.timeLeft = 3;
            alpha = 1;
            scale = 1f;
            float swingAngle;
            float cosValue = -(float)Math.Cos(Math.Pow(progress,0.5) * Math.PI);
            swingAngle = MathHelper.PiOver2 * cosValue;

            //，在不设置射弹速度时，Projectile.velocity此时为一个x+0/-0,y-0的奇怪系统，不能指示方向

            Projectile.rotation = Projectile.velocity.ToRotation() + swingAngle * Projectile.ai[0];
            Projectile.Center = owner.MountedCenter;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)(Math.PI * 0.5f));

            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            if (counter > MaxUpdateTimes)
            {
                owner.itemTime = 1;
                owner.itemAnimation = 1;
                Projectile.Kill();
            }


        }
        //使用该函数防止由于AI速度进行更新
        public override bool ShouldUpdatePosition()
        {
            return false;
        }

            public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            int direction = (int)(Projectile.ai[0]);
            //=45度向右或者-135度向左
            float rot = direction > 0 ? Projectile.rotation + MathHelper.PiOver4 : Projectile.rotation + MathHelper.Pi * 0.75f;
            Vector2 origin = direction > 0 ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);
            SpriteEffects effect = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            

            Main.EntitySpriteDraw(tex, Projectile.Center +owner.gfxOffY * Vector2.UnitY - Main.screenPosition, null, lightColor * alpha , rot, origin, Projectile.scale * scale , effect);

            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            float weaponLength = tex.Width * (float)Math.Sqrt(2) * Projectile.scale;
            Vector2 bladeTip = Projectile.Center + Projectile.rotation.ToRotationVector2() * weaponLength;
            
            float collisionPoint = 0;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, bladeTip, 6, ref collisionPoint))
            {
                return true;
            }
            
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void CutTiles()
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            float weaponLength = tex.Width * (float)Math.Sqrt(2) * Projectile.scale * scale;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * weaponLength, 54, DelegateMethods.CutTiles);
        }
        

    }
    
}