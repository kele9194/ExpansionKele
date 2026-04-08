using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class GiantFeatherProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.MagicProj";
        private static Asset<Texture2D> _cachedTexture;
        
        public override void Load()
        {
            // 预加载纹理
            _cachedTexture = TextureAssets.Projectile[Projectile.type];
        }
        
        public override void Unload()
        {
            _cachedTexture = null;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.light = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
            Projectile.ArmorPenetration = 15;
        }

        public override void AI()
        {
            Texture2D texture = _cachedTexture.Value;
            
            // 添加更密集的羽毛粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, texture.Width, texture.Height, DustID.BlueFairy, 0f, 0f, 100, default(Color), 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            
            // 偶尔产生额外的金色粒子
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, texture.Width, texture.Height, DustID.BlueFlare, 0f, 0f, 100, default(Color), 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 逐渐减速，但比小羽毛慢一些
            Projectile.velocity *= 0.995f;
        }

        public override void OnKill(int timeLeft)
        {
            Texture2D texture = _cachedTexture.Value;
            
            // 消失时产生更华丽的羽毛粒子效果
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, texture.Width, texture.Height, DustID.BlueFairy, 0f, 0f, 100, default(Color), 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
            
            // 添加一些金色粒子
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, texture.Width, texture.Height, DustID.GoldFlame, 0f, 0f, 100, default(Color), 1.3f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }
        }
    }
}