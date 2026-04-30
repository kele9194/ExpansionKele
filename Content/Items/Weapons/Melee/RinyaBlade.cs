using System;
using ExpansionKele.Content.Audio;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.GenericProj;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class RinyaBlade : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Melee";

        public static int Rotation = 1;
        
        public override void SetStaticDefaults()
        {
            
        }
        
        public override bool MeleePrefix()
        {
            return true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(15,18);
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.noUseGraphic = true;
            Item.knockBack = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<RinyaInvProj>();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.shootSpeed = 10;
            Item.scale = 1;
            Item.channel = true;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(
                player.GetSource_FromThis(),
                player.position,
                Vector2.Zero,
                ModContent.ProjectileType<RinyaInvProj>(),
                damage,
                0f,
                player.whoAmI,
                Rotation
            );
            Rotation *= -1;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<StoneBlade>(), 1)
                .AddIngredient(ItemID.Amethyst, 10)
                .AddIngredient(ItemID.GoldBar, 5)
                .Register();
            
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<StoneBlade>(), 1)
                .AddIngredient(ItemID.Amethyst, 10)
                .AddIngredient(ItemID.PlatinumBar, 5)
                .Register();
        }
    }

    public class RinyaInvProj : InvisiblePlayerProjectile
    {
        protected override int MaxLifetime => 3600;
        
        private float counter = 0f;
        public static int maxSwing = 6;
        private int previousSwingPhase = -1;
        private float rotationDirection = 1f;

        protected override void SetInvisibleDefaults(){
            Projectile.DamageType = DamageClass.Melee;
        }
        
        protected override void UpdatePlayerLogic()
        {
            Player owner = OwnerPlayer;
            if (owner == null || !owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            
            if (!owner.channel)
            {
                Projectile.Kill();
                return;
            }
            
            float MaxUpdateTimes = owner.itemTimeMax *Projectile.MaxUpdates;
            float progress = (counter / MaxUpdateTimes);
            
            int swingPhase = (int)progress;
            float phaseProgress = progress - swingPhase;
            
            Vector2 mouseWorldPos = Main.MouseWorld;
            Vector2 directionToMouse = (mouseWorldPos - owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            
            if (counter%MaxUpdateTimes == 0)
            {
                Vector2 spawnPosition = owner.MountedCenter;
                Vector2 velocity =directionToMouse * 10f;
            
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<RinyaBladeHeld>(),
                Projectile.damage,
                Projectile.knockBack,
                owner.whoAmI,
                rotationDirection,
                swingPhase
            );
                
            }
            
            previousSwingPhase = swingPhase;
            
            counter++;
            
            if (progress > maxSwing)
            {
                Projectile.Kill();
            }
        }
        
        
        protected override void OnPlayerProjectileKilled(int timeLeft)
        {
            Player owner = OwnerPlayer;
            if (owner != null && owner.active && !owner.dead)
            {
                owner.itemTime = 1;
                owner.itemAnimation = 1;
            }
        }
    }

    public class RinyaBladeHeld : ModProjectile
    {
        public override string Texture => this.GetRelativeTexturePath("./RinyaBlade");
        private static Asset<Texture2D> _cachedTexture;
        public float scale = 1;
        public float alpha = 0;
        public bool init = true;
        public float scaleBigger=1.3f;
        private float counter = 0f;
        
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
            // Main.projFrames[Projectile.type] = 1;
            // ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            // ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
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
            Projectile.extraUpdates = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.hide = true;
        }
        
        public override void AI()
        {
            
            Player owner = Main.player[Projectile.owner];
            float MaxUpdateTimes = owner.itemTimeMax *Projectile.MaxUpdates;
            float progress = (counter / MaxUpdateTimes);
            counter++;
            
            
            if (owner == null || !owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            
            
            if (init)
            {
                float volumeScale = MathHelper.Lerp(0.3f, 1f, (1+Projectile.ai[1] / 2f)/(1+RinyaInvProj.maxSwing/2f));
                SoundStyle swingSound = ExpansionKeleSounds.SwingSound;
                swingSound.Volume = 0.5f * volumeScale;
                SoundEngine.PlaySound(swingSound, Projectile.position);
                Projectile.scale *= owner.HeldItem.scale;
                init = false;
            }
            
            Projectile.timeLeft = 3;
            alpha = 1;
            

            scale = 1+(scaleBigger-1)* Projectile.ai[1] / 2;
            
            float swingAngle = MathHelper.PiOver2 * -(float)Math.Cos(Math.Pow(progress, 0.5) * Math.PI);
            float direction = Projectile.ai[0]*(Projectile.ai[1]%2==0?-1:1);
            Projectile.rotation = Projectile.velocity.ToRotation() + swingAngle * direction;
            Projectile.Center = owner.MountedCenter;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)(Math.PI * 0.5f));

            owner.direction = Projectile.velocity.X > 0 ? 1 : -1;
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            if (!owner.channel||counter > MaxUpdateTimes)
            {
                Projectile.Kill();
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage*=1+(scaleBigger-1)* Projectile.ai[1] / 2;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            float direction = Projectile.ai[0]*(Projectile.ai[1]%2==0?-1:1);
            float rot = direction > 0 ? Projectile.rotation + MathHelper.PiOver4 : Projectile.rotation + MathHelper.Pi * 0.75f;
            Vector2 origin = direction > 0 ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);
            SpriteEffects effect = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            Main.EntitySpriteDraw(tex, Projectile.Center + owner.gfxOffY * Vector2.UnitY - Main.screenPosition, null, lightColor * alpha, rot, origin, Projectile.scale * scale, effect);

            return false;
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            float weaponLength = tex.Width * (float)Math.Sqrt(2) * Projectile.scale*scale;
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