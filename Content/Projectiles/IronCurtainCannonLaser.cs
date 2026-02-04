using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Terraria.NPC;
using Terraria.DataStructures;
using ReLogic.Content;
namespace ExpansionKele.Content.Projectiles
{ 
public class IronCurtainCannonLaser : ModProjectile
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
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = 0; // 自定义AI
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1; // 无限穿透
        Projectile.timeLeft = 600; // 设置一个合理的生存时间
        Projectile.tileCollide = false; // 不与地形碰撞
        Projectile.ignoreWater = true; // 忽略水
        Projectile.extraUpdates = 1; // 增加更新频率
        Projectile.alpha = 0;
        AIType= ProjectileID.Bullet;
        Projectile.light = 1f;
    }

    public override void AI()
    {
        // 自定义AI逻辑，例如增加旋转效果
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        float speed = 30f; // 你可以根据需要调整速度
        Projectile.velocity.Normalize();
        Projectile.velocity *= speed;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        // 不要调用 base.OnHitNPC，以防止子弹消失
    }
    public override bool PreDraw(ref Color lightColor)
        {
       Texture2D texture = _cachedTexture.Value;
        // 计算绘制位置
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        Vector2 drawPos = Projectile.Center - Main.screenPosition;

        // 设置颜色
        Color drawColor = lightColor * (1f - Projectile.alpha / 255f);

        // 绘制子弹
        Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

        // 返回 false 以防止默认绘制
        return false;
    }
}
}