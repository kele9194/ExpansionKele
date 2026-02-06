using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.Audio;

namespace ExpansionKele.Content.Bosses.BossKele
{
    public class BossKeleRedLaser : ModProjectile
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
            Projectile.penetrate = -1; // 无限穿透
            Projectile.tileCollide = false; // 穿透物块
            Projectile.width = 8; // 激光宽度
            Projectile.height = 8;
            Projectile.knockBack = 8f; // 击退力度
            Projectile.extraUpdates = 2; // 增加更新频率使移动更平滑
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // 直接设置Projectile造成防御损伤
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                calamity.Call("SetDefenseDamageProjectile", Projectile, true);
            }
            
            // 添加击中特效
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
        }

        public override void AI()
        {
            // 根据移动方向设置旋转角度
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
            // 添加红色激光粒子效果
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
                
                // 添加额外的红色发光粒子
                Dust glowDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, Color.Red, 1.2f);
                glowDust.noGravity = true;
                glowDust.velocity *= 0.1f;
            }
            
            // 激光淡入效果
            if (Projectile.timeLeft > 270)
            {
                Projectile.alpha = (int)(255 * (1 - (300 - Projectile.timeLeft) / 30f));
            }
            // 激光淡出效果
            else if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)(255 * (1 - Projectile.timeLeft / 30f));
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取投射物纹理
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            
            // 计算绘制位置
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // 添加激光发光效果
            for (int i = 0; i < 6; i++)
            {
                Vector2 drawOffset = new Vector2(Main.rand.Next(-1, 2), Main.rand.Next(-1, 2));
                Main.EntitySpriteDraw(texture, drawPos + drawOffset, null, new Color(255, 100, 100, 50) * 0.4f, 
                    Projectile.rotation, drawOrigin, Projectile.scale * 1.2f, SpriteEffects.None, 0);
            }
            
            // 主要激光绘制（更亮的红色）
            Color laserColor = new Color(255, 80, 80, 200) * ((255 - Projectile.alpha) / 255f);
            Main.EntitySpriteDraw(texture, drawPos, null, laserColor, 
                Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
                
            return false; // 阻止默认绘制
        }

        public override void OnKill(int timeLeft)
        {
            // 激光消散时的效果
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1.8f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            
            // 播放激光消散音效
            SoundEngine.PlaySound(SoundID.Item110 with { Pitch = 0.5f }, Projectile.Center);
        }
    }
}