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
    public class FeatherPlusProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.MagicProj";
        private static Asset<Texture2D> _cachedTexture;
        
        public override void Load()
        {
            // 预加载纹理
            _cachedTexture=TextureAssets.Projectile[Projectile.type];
        }
        
        public override void Unload()
        {
            _cachedTexture=null;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.ArmorPenetration = 10;
        }

        public override void AI()
        {
            Texture2D texture = _cachedTexture.Value;
            // 添加羽毛粒子效果
            if (Main.rand.NextBool(4))
            {
                
                //Vector2 offset = new Vector2((float)Math.Cos(Projectile.rotation), (float)Math.Sin(Projectile.rotation)) * new Vector2(texture.Width * 0.5f, texture.Height * 0.5f).Length();
                Dust dust = Dust.NewDustDirect(Projectile.position, texture.Width, texture.Height, DustID.BlueFairy, 0f, 0f, 100, default(Color), 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            Projectile.rotation=Projectile.velocity.ToRotation()+MathHelper.PiOver4;

            // 逐渐减速
            Projectile.velocity *= 0.99f;
        }

        public override void OnKill(int timeLeft)
        {
            Texture2D texture = _cachedTexture.Value;
            // 消失时产生羽毛粒子效果
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, texture.Width, texture.Height, DustID.BlueFairy, 0f, 0f, 100, default(Color), 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }
        // public override bool PreDraw(ref Color lightColor)
        // {
        //     Texture2D texture = _cachedTexture.Value;
            
        //     // 将绘制位置向上偏移，使碰撞箱位于贴图底部
        //     Vector2 drawOffset = new Vector2(0, -texture.Height);
        //     Vector2 drawPosition = Projectile.position + drawOffset;
            
        //     Main.EntitySpriteDraw(
        //         texture,
        //         drawPosition - Main.screenPosition,
        //         null,
        //         lightColor,
        //         Projectile.rotation,
        //         new Vector2(texture.Width * 0.5f, texture.Height*0.5f),
        //         Projectile.scale,
        //         SpriteEffects.None,
        //         0
        //     );
            
        //     return false;
        // }
    }
}