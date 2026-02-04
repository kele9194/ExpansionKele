using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using System;
using ReLogic.Content; // 添加这个using语句

namespace ExpansionKele.Content.Projectiles
{
    public class ColaProjectileLower : ModProjectile
    {
        public static readonly Color ColaColor = new Color(214, 123, 44);
        public override string Texture => "ExpansionKele/Content/Projectiles/ColaProjectileLower";
        
        // 添加Asset<Texture2D>字段进行优化
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

        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            //ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
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
            Projectile.extraUpdates = 3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Vector2 target = Main.MouseWorld - Projectile.Center;
            target.Normalize();
            target *= 12f;
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

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 0, ColaColor, 1f);
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
            if (_cachedTexture?.Value != null)
            {
                Texture2D texture = _cachedTexture.Value;
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
            target.immune[Projectile.owner] = 4;
        }
    }
}