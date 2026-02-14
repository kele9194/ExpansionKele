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
using ReLogic.Content;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class ColaProjectile : ModProjectile
    {
        public static readonly Color ColaColor = new Color(214, 123, 44);
        
        // 使用标准的Texture属性
        public override string Texture => "ExpansionKele/Content/Projectiles/MeleeProj/ColaProjectile";

        // 使用Asset<Texture2D>进行优化
        private static Asset<Texture2D> _projectileTexture;
        private static Asset<Texture2D> _explosionTexture;

        public override void Load()
        {
            // 预加载纹理资源，提高性能
            _projectileTexture = ModContent.Request<Texture2D>(Texture);
            _explosionTexture = ModContent.Request<Texture2D>("ExpansionKele/Content/Projectiles/MeleeProj/ColaExplosion");
        }

        public override void Unload()
        {
            // 清理资源引用
            _projectileTexture = null;
            _explosionTexture = null;
        }

        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            // ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.timeLeft = 200;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            AIType = ProjectileID.Bullet;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Vector2 target = Main.MouseWorld - Projectile.Center;
            target.Normalize();
            target *= 24f;
            Projectile.velocity = target;
            Projectile.damage = (int)(Projectile.damage * 1.25);
        }

        public override void AI()
        {
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }
            
            Lighting.AddLight(Projectile.Center, 1f, 0.8f, 0.4f);

            int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 0, ColaColor, 1f);
            Dust dust = Main.dust[dustIndex];
            dust.noGravity = true;
            dust.scale = 1.5f;
            dust.fadeIn = 1f;
            dust.noLight = false;
            dust.color = ColaColor;
            Lighting.AddLight(dust.position, 1f, 0.8f, 0.4f);
            
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用预加载的纹理资源
            if (_projectileTexture?.Value != null)
            {
                Texture2D texture = _projectileTexture.Value;
                Main.spriteBatch.Draw(
                    texture, 
                    Projectile.Center - Main.screenPosition, 
                    null, 
                    lightColor, 
                    Projectile.rotation, 
                    new Vector2(texture.Width / 2, texture.Height / 2), 
                    Projectile.scale, 
                    SpriteEffects.None, 
                    0
                );
                return false;
            }
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ArmorPodweredLower>(), 82);
            if (ExpansionKele.calamity != null)
            {
                target.AddBuff(ExpansionKele.calamity.Find<ModBuff>("MarkedforDeath").Type, 100);
            }
            
            int explosionDamage = (int)(Projectile.damage * 0.6 * Math.Pow(2, Owner.GetTotalDamage(DamageClass.Melee).ApplyTo(1) - 1) / (Owner.GetTotalDamage(DamageClass.Melee).ApplyTo(1)));
            Terraria.Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Mod.Find<ModProjectile>("ColaExplosion").Type, explosionDamage, Projectile.knockBack, Projectile.owner);
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        }

        public class ColaExplosion : ModProjectile
        {
            // 内部类也使用标准Texture属性
            public override string Texture => "ExpansionKele/Content/Projectiles/MeleeProj/ColaExplosion";

            public override void SetDefaults()
            {
                Projectile.width = 20;
                Projectile.height = 20;
                Projectile.friendly = true;
                Projectile.hostile = false;
                Projectile.penetrate = -1;
                Projectile.tileCollide = false;
                Projectile.timeLeft = 100;
                Projectile.DamageType = DamageClass.Melee;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.localNPCHitCooldown = 6;
            }

            public override void AI()
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, ColaProjectile.ColaColor, 1f);
                }

                Projectile.alpha += 60;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }

            public override bool PreDraw(ref Color lightColor)
            {
                // 为爆炸效果也使用预加载的纹理
                if (ColaProjectile._explosionTexture?.Value != null)
                {
                    Texture2D texture = ColaProjectile._explosionTexture.Value;
                    Color drawColor = lightColor * ((255 - Projectile.alpha) / 255f);
                    Main.spriteBatch.Draw(
                        texture,
                        Projectile.Center - Main.screenPosition,
                        null,
                        drawColor,
                        Projectile.rotation,
                        new Vector2(texture.Width / 2, texture.Height / 2),
                        Projectile.scale,
                        SpriteEffects.None,
                        0
                    );
                    return false;
                }
                return true;
            }

            public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
            {
                // 可以在这里添加额外效果
            }
        }
    }
}