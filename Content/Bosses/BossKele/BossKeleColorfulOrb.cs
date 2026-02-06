using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.Audio;

namespace ExpansionKele.Content.Bosses.BossKele
{
    public class BossKeleColorfulOrb : ModProjectile
    {
        public override string LocalizationCategory=>"Bosses.BossKele";
    
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

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }
        

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = -1;
            Projectile.scale = 1f;
            
            // 设置碰撞箱大小
            Projectile.width = 16;
            Projectile.height = 16;
            
            // 设置穿透性
            Projectile.penetrate = 1;
            
            // 设置击退
            Projectile.knockBack = 5f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // 设置造成防御损伤
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                calamity.Call("SetDefenseDamageProjectile", Projectile, true);
            }
            
            // 添加击中效果
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }

        public override void AI()
        {
            // 根据移动方向设置旋转角度
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
            // 添加粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            
            // 添加淡出效果
            if (Projectile.timeLeft < 60)
            {
                Projectile.alpha += 5;
                if (Projectile.alpha > 255)
                    Projectile.alpha = 255;
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取投射物纹理
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            
            // 计算绘制位置
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // 添加发光效果
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = new Vector2(Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                Main.EntitySpriteDraw(texture, drawPos + drawOffset, null, new Color(255, 255, 255, 0) * 0.3f, 
                    Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            
            // 主要绘制
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor * ((255 - Projectile.alpha) / 255f), 
                Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
                
            return false; // 阻止默认绘制
        }

        public override void OnKill(int timeLeft)
        {
            // 死亡时产生爆炸效果
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity *= 3f;
            }
            
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);
        }
    }
}