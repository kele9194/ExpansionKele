using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Projectiles.MagicProj;
using InnoVault;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    public class ThousandEyedSoulChase : ModItem,IChargeableItem
    {
        public float GetCurrentCharge()
        {
            Player player = Main.LocalPlayer;
            if (player.active && player.HeldItem.type == Item.type)
            {
                foreach (var proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ThousandEyedSoulChaseHeld>())
                    {
                        if (proj.ModProjectile is ThousandEyedSoulChaseHeld ThousandEyedSoulChaseHeldProj)
                        {

                                return ThousandEyedSoulChaseHeldProj.charge * 100f;
                            
                        }
                    }
                }
            }
            return 0f;
        }
// ... existing code ...

        public float GetMaxCharge()
        {
            return ThousandEyedSoulChaseHeld.MAX_CHARGE*100f;
        }
        public override string LocalizationCategory => "Items.Weapons.Magic";

        
        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(160, 190);
            Item.DamageType = DamageClass.Magic;
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 5f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item43;
            Item.shoot = ModContent.ProjectileType<ThousandEyedSoulChaseHeld>();
            Item.shootSpeed = 12f;
            Item.mana = 12;
            Item.channel = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShadowFlameBow, 1)
                .AddIngredient(ItemID.SpectreBar, 3)
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }


    public class ThousandEyedSoulChaseHeld : ModProjectile
    {
        public override string Texture => this.GetRelativeTexturePath("./ThousandEyedSoulChaseSheet");
        
        private static Asset<Texture2D> _cachedTexture;
        [SyncVar]
        public float charge = 0f;
        public const float MAX_CHARGE = 1f;


        [SyncVar]
        public Vector2 MousePosition;
        [SyncVar]
        public bool init = true;
        [SyncVar]
        public int frameCounter = 0;
        [SyncVar]
        public int currentFrame = 0;

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
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= 0f;
        }


        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float UseSpeedMul=UseTimeHelper.GetTotalUseMultiplier(player,player.HeldItem, true);
            Projectile.timeLeft = 3;
            MousePosition = Main.MouseWorld;
            Projectile.Center = player.MountedCenter;
            Vector2 MouseVector = MousePosition - Projectile.Center;
            Projectile.rotation = MouseVector.ToRotation();
            player.direction = MouseVector.X > 0 ? 1 : -1;

            if (player.channel)
            {
                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                charge +=0.02f*UseSpeedMul/Projectile.MaxUpdates;
                if (charge>= MAX_CHARGE)
                {
                    charge =MAX_CHARGE;
                }
                Projectile.frame = Math.Min((int)(charge*4/MAX_CHARGE), 3);

                
            }
            else
            {
                player.itemTime = 1;
                player.itemAnimation = 1;
                Projectile.Kill();
                if (init)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                        player.MountedCenter + Vector2.Normalize(MouseVector) * 12f,
                        MouseVector.SafeNormalize(Vector2.Zero) * player.HeldItem.shootSpeed,
                        ModContent.ProjectileType<MarkedArrow>(),
                        Projectile.damage,
                        0f,
                        Projectile.owner,
                        charge
                        );
                    init = false;
                }
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation-MathHelper.PiOver2);
        }

        

        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null)
                return true;

            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            Vector2 MouseVector = MousePosition - Projectile.Center;
            int direction = MouseVector.X > 0 ? 1 : -1;

            float rot = direction > 0 ? Projectile.rotation : Projectile.rotation;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 8f);
            SpriteEffects effect = direction > 0 ? SpriteEffects.None : SpriteEffects.None;

            Rectangle frameRect = new Rectangle(0, tex.Height / 4 * Projectile.frame, tex.Width, tex.Height / 4);

            Main.EntitySpriteDraw(tex, 
                Projectile.Center + owner.gfxOffY * Vector2.UnitY + Projectile.rotation.ToRotationVector2() * 24f - Main.screenPosition, 
                frameRect, 
                lightColor, 
                rot, 
                origin, 
                Projectile.scale, 
                effect, 
                0);
            
            return false;
        }
    }
}