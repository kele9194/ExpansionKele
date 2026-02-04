using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using ReLogic.Content;

namespace ExpansionKele.Content.Bosses.BossKele
{
    public class BossKeleColorfulOrb : ModProjectile
    {public override string LocalizationCategory=>"Bosses.BossKele";
    
        private Asset<Texture2D> _cachedTexture;
        
        public override void Load()
        {
            // 预加载纹理
            _cachedTexture=ModContent.Request<Texture2D>(Texture);
        }
        
        public override void Unload()
        {
            // 可选：清空引用
            _cachedTexture = null;
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
        }
        public override void OnSpawn(IEntitySource source)
        {
            Texture2D texture = _cachedTexture.Value;
            Projectile.width = texture.Width;
            Projectile.height = texture.Height;
        }

    //     public override void OnHitPlayer(Player target, Player.HurtInfo info)
    // {
    //     // 直接设置Projectile造成防御损伤
    //     if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
    //     {
    //         calamity.Call("SetDefenseDamageProjectile", Projectile, true);
    //     }
    // }

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
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = _cachedTexture.Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 position = Projectile.Center - Main.screenPosition;
            
            Main.EntitySpriteDraw(texture, position, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}