using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Terraria.NPC;
using Terraria.DataStructures;
using Terraria.Audio;
using ReLogic.Content;
namespace ExpansionKele.Content.Projectiles
{ 

public class IronCurtainCannonProjectile : ModProjectile
    {
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // 添加重力效果
             Projectile.velocity.Y += 0.1f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 生成红色脉动光圈
            Terraria.Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IronCurtainCannonExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        public override void OnKill(int timeLeft)
        {
            // 生成红色脉动光圈
            if (Projectile.owner == Main.myPlayer)
            {
                Terraria.Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IronCurtainCannonExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            SoundEngine.PlaySound(new SoundStyle("ExpansionKele/Content/Audio/IronCurtainExplosion") with { Volume = 30f, PitchVariance = 0.1f }, Projectile.Center);
        }
    }
        public class IronCurtainCannonExplosion : ModProjectile
    {
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
        public override void SetDefaults()
        {
            Projectile.width = 256;
            Projectile.height = 256;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8; // 5的局部无敌帧
        }

       public override void AI()
{
    
        // 播放爆炸音效
        


    for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (player.active && player.Hitbox.Intersects(Projectile.Hitbox))
            {
                // 增加耐力
                player.endurance += 0.5f;

                // 确保耐力不超过最大值
                if (player.endurance > 1f)
                {
                    player.endurance = 1f;
                }
            }
        }
}

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制红色脉动光圈
            Texture2D texture = _cachedTexture.Value;
            // 计算透明度，从240开始逐渐变为0
            int alpha = (int)(240 * (Projectile.timeLeft / 60f));
            Color color = Color.Red * (alpha / 255f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, color, 0f, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
    

}