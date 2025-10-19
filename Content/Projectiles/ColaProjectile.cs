using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using System;
using ExpansionKele.Content.Buff;

namespace ExpansionKele.Content.Projectiles


{
    
    public class ColaProjectile : ModProjectile
    {
        public static Color  ColaColor = new Color(214, 123, 44);
        public override string Texture => "ExpansionKele/Content/Projectiles/ColaProjectile"; // Use texture of item as projectile texture

        private Player Owner => Main.player[Projectile.owner];
        //private float baseDamage;

        public override void SetStaticDefaults()
        {
            //ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10; // Hitbox width of projectile
            Projectile.height = 10; // Hitbox height of projectile
            Projectile.friendly = true; // Projectile hits enemies
            Projectile.timeLeft = 200; // Time it takes for projectile to expire
            Projectile.penetrate = -1; // Projectile penetrates infinitely
            Projectile.tileCollide = true; // Projectile collides with tiles
            Projectile.DamageType = DamageClass.Melee; // Projectile is a melee projectile
            AIType = ProjectileID.Bullet;
            Projectile.extraUpdates=2;
            Projectile.usesLocalNPCImmunity = true;  // 使用本地无敌帧系统
            Projectile.localNPCHitCooldown = 6; 
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Set the projectile's velocity towards the mouse cursor
            Vector2 target = Main.MouseWorld - Projectile.Center;
            target.Normalize();
            target *= 24f; // Speed of the projectile
            Projectile.velocity = target;

            // Get the base damage of the StarySword
            // ModItem starySword = ModContent.GetInstance<StarySword>();
            // baseDamage = starySword.Item.damage*(1+starySword.Item.damageGenericUp) * 1.25f;
            Projectile.damage = (int)(Projectile.damage*1.25);
        }

        public override void AI()
        {
            // Kill the projectile if the player dies or gets crowd controlled
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }
            Lighting.AddLight(Projectile.Center, 1f, 0.8f, 0.4f);

            // Dust trail effect
            
            int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 0, ColaColor, 1f);
            Dust dust = Main.dust[dustIndex];
            dust.noGravity = true; // 可选：使尘埃不受重力影响
            dust.scale = 1.5f; // 可选：调整尘埃的大小
            dust.fadeIn = 1f; // 设置尘埃的淡入效果
            dust.noLight = false; // 允许尘埃发光
            dust.color = ColaColor; // 设置尘埃的颜色
            Lighting.AddLight(dust.position, 1f, 0.8f, 0.4f);
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
    
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Play sound on collision
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }

            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale, SpriteEffects.None, 0);

            // Since we are doing a custom draw, prevent it from normally drawing
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Calculate explosion damage as 82% of the projectile's damage
            target.AddBuff(ModContent.BuffType<ArmorPodweredLower>(), 82);
            if(ExpansionKele.calamity!=null)
            {
                target.AddBuff(ExpansionKele.calamity.Find<ModBuff>("MarkedforDeath").Type, 100);
            }
            
            int explosionDamage = (int)(Projectile.damage * 0.6*Math.Pow(2,Owner.GetTotalDamage(DamageClass.Melee).ApplyTo(1)-1)/(Owner.GetTotalDamage(DamageClass.Melee).ApplyTo(1)));
            

            // Create an explosion effect
            Terraria.Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Mod.Find<ModProjectile>("ColaExplosion").Type, explosionDamage, Projectile.knockBack, Projectile.owner);

            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            
        }

        public class ColaExplosion : ModProjectile
    {
        public override string Texture => "ExpansionKele/Content/Projectiles/ColaExplosion"; // Use appropriate texture for explosion

        public override void SetDefaults()
        {
            Projectile.width = 20; // Width of the explosion
            Projectile.height = 20; // Height of the explosion
            Projectile.friendly = true; // Explosion hits enemies
            Projectile.hostile = false; // Explosion does not hit players
            Projectile.penetrate = -1; // Explosion penetrates infinitely
            Projectile.tileCollide = false; // Explosion does not collide with tiles
            Projectile.timeLeft = 100; // Time it takes for explosion to disappear
            //搞不懂，后面还有Projectile.alpha += 10;在60，10和30，5时额外两次，60，5为4次，30，10为额外0次
            Projectile.DamageType = DamageClass.Melee; // Explosion is a melee projectile
            Projectile.usesLocalNPCImmunity = true;  // 使用本地无敌帧系统
            Projectile.localNPCHitCooldown = 6; 
            
        }

        public override void AI()
        {
            // Dust effect for explosion
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, ColaProjectile.ColaColor, 1f);
            }

            // Fade out the explosion over time
            Projectile.alpha += 60;
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
        }
        

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Additional effects on hit can be added here if needed
            // target.immune[Projectile.owner] = 2;
            
        }
    }
}

    }