using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent;
//using Terraria.ID;
//using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles

{       
    public class SharkyBulletPlus : ModProjectile
    {
        public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30; // The length of old position to be recorded
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
		}

        
        public override void SetDefaults()
        {
            Projectile.width = 12; // 弹丸宽度
            Projectile.height = 12; // 弹丸高度
            Projectile.aiStyle = 1;
            Projectile.friendly = true; // 弹丸是否对敌人友好
            Projectile.hostile = false; // 弹丸是否对玩家友好
            Projectile.penetrate = -1; // 弹丸穿透次数
            Projectile.timeLeft = 600; // 弹丸存在时间
            Projectile.alpha = 255; // 弹丸透明度
            Projectile.light = 1f; // 弹丸发光强度
            Projectile.ignoreWater = true; // 忽略水
            Projectile.tileCollide = true; // 是否与方块碰撞
            Projectile.extraUpdates = 12; // 弹丸更新频率
            Projectile.DamageType = DamageClass.Ranged;
            AIType = ProjectileID.Bullet;
            //AIType = ProjectileID.BulletHighVelocity;
            //Main.NewText($"SharkyBullet AIType: {AIType}");
        }
public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 当子弹碰到地面时，直接销毁子弹
            Projectile.Kill();
            return false;
        }
public override bool PreDraw(ref Color lightColor) {
    Texture2D texture = TextureAssets.Projectile[Type].Value;

    // Redraw the projectile with the color not influenced by light
    Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
    for (int k = 0; k < Projectile.oldPos.Length; k++) {
        Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
        Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
        Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
    }

    return true;
}
public override void OnKill(int timeLeft) {
    // This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
    Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
    SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
}
        

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            // 施加自定义减益效果
            //target.AddBuff(ModContent.BuffType<BleedBlue>(), 600);
            target.AddBuff(ModContent.BuffType<ArmorPodwered>(), 820); // 600 ticks = 10 seconds
            if(ExpansionKele.calamity!=null)
            {
                target.AddBuff(ExpansionKele.calamity.Find<ModBuff>("MarkedforDeath").Type, 401);
                target.AddBuff(ExpansionKele.calamity.Find<ModBuff>("MiracleBlight").Type, 401);
            }
            // 获取发射子弹的玩家对象
    Player player = Main.player[Projectile.owner];

    // 计算已损生命值
    float missingHealth = player.statLifeMax2 - player.statLife;
    // 计算需要恢复的生命值（已损生命值的4%）
    float healthToRestore = missingHealth * 0.08f;
    // 恢复生命值
    player.statLife += (int)healthToRestore;
    // 确保生命值不超过最大生命值
    player.statLife = Math.Min(player.statLife, player.statLifeMax2);
    // 更新玩家的生命值显示
    player.HealEffect((int)healthToRestore, true);
        }
    }
}