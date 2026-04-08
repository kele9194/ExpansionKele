using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MagicProj;
using InnoVault;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    public class Rinya : ModItem,IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons.Magic";

        

        // ... existing code ...
        public float GetCurrentCharge()
        {
            Player player = Main.LocalPlayer;
            if (player.active && player.HeldItem.type == Item.type)
            {
                foreach (var proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<RinyaProjectile>())
                    {
                        if (proj.ModProjectile is RinyaProjectile rinyaProj)
                        {
                            if (!rinyaProj.isReleased)
                            {
                                return rinyaProj.currentCharge * 100f;
                            }
                        }
                    }
                }
            }
            return 0f;
        }
// ... existing code ...

        public float GetMaxCharge()
        {
            return RinyaProjectile.MAX_CHARGE*100f;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(90, 110);
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item43;
            Item.shoot = ModContent.ProjectileType<RinyaHeld>();
            Item.shootSpeed = 10f;
            Item.mana = 10;
            Item.channel = true;
        }
    }
    public class MoonLordRinyaDrop : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.MoonLordCore)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Rinya>(), 4, 1, 1));
            }
        }
    }
        
// ... existing code ...
        

        public class RinyaHeld : ModProjectile
    {
        public override string Texture => this.GetRelativeTexturePath("./Rinya");
        
        private static Asset<Texture2D> _cachedTexture;

        [SyncVar]
        public Vector2 MousePosition;
        [SyncVar]
        public bool init=true;

        
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
            Projectile.extraUpdates = 2;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
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
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
            {
                overPlayers.Add(index);
            }


        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.timeLeft = 3;
            MousePosition = Main.MouseWorld;
            Projectile.Center = player.MountedCenter;
            Vector2 MouseVector = MousePosition-Projectile.Center;
            Projectile.rotation = MouseVector.ToRotation();
            player.direction = MouseVector.X > 0 ? 1 : -1;
            
            if (player.channel){
         
                
                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                if(init){
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    player.MountedCenter + Vector2.Normalize(MouseVector) * 240f, 
                    MouseVector.SafeNormalize(Vector2.Zero) * 10f, 
                    ModContent.ProjectileType<RinyaProjectile>(), 
                    (int)(Projectile.damage*0.8f), 
                    0f, 
                    Projectile.owner);
                    init=false;
                }
            }
            else{
                player.itemTime = 1;
                player.itemAnimation = 1;
                Projectile.Kill();
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation-MathHelper.PiOver2);
        }
        
        
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null)
                return true;
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = _cachedTexture.Value;
            Vector2 MouseVector = MousePosition-Projectile.Center;
            int direction=MouseVector.X > 0 ? 1 : -1;
            
            float rot = direction > 0 ? Projectile.rotation-MathHelper.PiOver4 : Projectile.rotation + MathHelper.Pi*5/4f ;
            Vector2 origin = direction > 0 ?  new Vector2(tex.Width/2f, tex.Height/2f) : new Vector2(tex.Width/2f, tex.Height/2f) ;
            SpriteEffects effect = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            Main.EntitySpriteDraw(tex, 
            Projectile.Center +owner.gfxOffY * Vector2.UnitY+Projectile.rotation.ToRotationVector2()*24f - Main.screenPosition, 
            null, 
            lightColor  , 
            rot, 
            origin, 
            Projectile.scale  , 
            effect,
            0);
            
            return false;
        }
        
        
    }
}

        
// ... existing code ...