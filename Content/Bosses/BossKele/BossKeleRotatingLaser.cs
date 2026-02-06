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
    public class BossKeleRotatingLaser : ModProjectile
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
            Projectile.width = 8; // 增加激光宽度
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = -1;
            Projectile.scale = 1f;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.tileCollide = false; // 穿透物块
            Projectile.knockBack = 6f; // 击退力度
            Projectile.extraUpdates = 1; // 增加更新频率
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // 设置造成防御损伤
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                calamity.Call("SetDefenseDamageProjectile", Projectile, true);
            }
            
            // 添加击中特效
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.8f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }
            
            // 添加额外的蓝色闪光效果
            Dust flashDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, Color.Cyan, 2f);
            flashDust.noGravity = true;
            flashDust.velocity *= 0.5f;
        }

        public override void AI()
        {
            // 根据移动方向设置旋转角度
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
            // 添加激光粒子效果
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
                
                // 添加额外的能量粒子
                Dust energyDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, Color.LightBlue, 1f);
                energyDust.noGravity = true;
                energyDust.velocity *= 0.1f;
            }
            
            // 旋转激光的特殊效果 - 随时间改变颜色
            if (Projectile.timeLeft % 20 == 0)
            {
                Dust colorDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, Color.Purple, 1.5f);
                colorDust.noGravity = true;
                colorDust.velocity *= 0.3f;
            }
            
            // 淡入淡出效果
            if (Projectile.timeLeft > 270)
            {
                Projectile.alpha = (int)(255 * (1 - (300 - Projectile.timeLeft) / 30f));
            }
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
            
            // 添加多层发光效果
            for (int i = 0; i < 5; i++)
            {
                Vector2 drawOffset = new Vector2(Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                Color glowColor = new Color(100, 150, 255, 80) * 0.3f;
                Main.EntitySpriteDraw(texture, drawPos + drawOffset, null, glowColor, 
                    Projectile.rotation, drawOrigin, Projectile.scale * 1.5f, SpriteEffects.None, 0);
            }
            
            // 主要绘制 - 渐变蓝色
            float alphaFactor = (255 - Projectile.alpha) / 255f;
            Color laserColor = new Color(80, 180, 255, 200) * alphaFactor;
            Main.EntitySpriteDraw(texture, drawPos, null, laserColor, 
                Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
                
            return false; // 阻止默认绘制
        }

        public override void OnKill(int timeLeft)
        {
            // 激光消散效果
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 1.8f;
            }
            
            // 添加紫色能量消散效果
            for (int i = 0; i < 5; i++)
            {
                Dust energyDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, Color.Purple, 1.2f);
                energyDust.noGravity = true;
                energyDust.velocity *= 1f;
            }
            
            // 播放消散音效
            SoundEngine.PlaySound(SoundID.Item110 with { Pitch = 0.3f }, Projectile.Center);
        }
    }
}