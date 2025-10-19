
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles
{
public class SpectralCurtainCannonProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 0; // 使用自定义AI而不是重力弹道
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.knockBack = 2f;
        }

        public override void AI()
        {
            if (Projectile.timeLeft <= 595) // 前5帧不追踪 (600-5=595)
            {
                ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 30f, 640f, 10f);
            }
            // 添加追踪AI
            ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 30f, 640f, 10f);
            
            // 旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 添加粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 生成蓝色爆炸效果
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SpectralCurtainCannonExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
    }

    public class SpectralCurtainCannonExplosion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 205; // 较小的爆炸范围
            Projectile.height = 205;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40; // 持续时间更长
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.knockBack = 0f;
        }

        public override void AI()
        {
            // 减速区域效果
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.Hitbox.Intersects(Projectile.Hitbox))
                {
                    // 对NPC施加减速效果
                    npc.velocity *= 0.985f; // 减速至90%
                }
            }

            // 玩家在区域内获得buff
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && player.Hitbox.Intersects(Projectile.Hitbox))
                {
                    // 提供生命恢复和伤害加成
                    player.lifeRegen += 4; // 4点生命恢复
                    
                    // 应用伤害增益（25%乘算增伤）
                    player.GetDamage(DamageClass.Generic) += 0.25f;
                }
            }

            // 添加粒子效果
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }

                public override bool PreDraw(ref Color lightColor)
        {
            // 使用默认纹理绘制蓝色脉动光圈
            // 计算透明度，从200开始逐渐变为0
            int alpha = (int)(200 * (Projectile.timeLeft / 40f));
            Color color = Color.Blue * (alpha / 255f);
            
            // 获取默认纹理
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, color, 0f, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}