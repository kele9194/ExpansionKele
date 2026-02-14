using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj{
    public abstract class EnergySwordProjectileLinear : ModProjectile
    {
        // 能量剑基础属性

        protected abstract float TextureScaleMultiplier { get; } // 贴图倍率
        protected float BaseScaleAdder = 1f;
        protected float BaseScaleMultiplier = 0.6f;
        protected float DustPositionMultiplier => 84f * TextureScaleMultiplier; // 放大3倍
        protected float DustRandomPositionMin => 20f * TextureScaleMultiplier; // 放大3倍
        protected float DustRandomPositionMax => 80f * TextureScaleMultiplier; // 放大3倍
        protected float DustSizeMultiplier => 1.2f * TextureScaleMultiplier; // 放大3倍
        protected float CollisionConeLength => 94f * TextureScaleMultiplier; // 放大3倍 - 影响碰撞范围
        protected float EnchantmentVisualDistance => 70f * TextureScaleMultiplier; // 放大3倍 - 影响附魔视觉效果距离
        protected float EnchantmentVisualSize => 60f * TextureScaleMultiplier; // 放大3倍 - 影响附魔视觉效果大小
        protected float CutTilesDistance => 60f * TextureScaleMultiplier; // 放大3倍 - 影响切割瓦片距离
        protected float DrawScaleMultiplier => 1.1f * TextureScaleMultiplier; // 放大3倍 - 影响绘制时的整体尺寸
        protected virtual float DrawScaleFrontPart { get; } = 0.975f; // 移除额外放大 - 保持原始比例关系
        protected virtual float DrawThinMiddleScale { get; } = 0.8f; // 移除额外放大 - 保持原始比例关系
        protected virtual float DrawThinBottomScale { get; } = 0.6f; // 移除额外放大 - 保持原始比例关系
        protected virtual float SparkleBaseOffset { get; } = 6f; //移除额外放大 - 保持原始比例关系
        protected virtual float SparkleFrontOffset { get; } = 4f; // 移除额外放大 - 保持原始比例关系
        protected virtual float SparkleYStart { get; } = 3f; // 移除额外放大 - 保持原始比例关系
        protected virtual float ProjectileWidthHeight => 75f * TextureScaleMultiplier*2; // 修复：改为计算属性

        // 颜色定义
        protected abstract Color backDarkColor { get; } // 最深蓝色（后部）
        protected abstract Color middleMediumColor { get; } // 中等亮度蓝色（中部）
        protected abstract Color frontLightColor { get; } // 最亮蓝色（前部）
        public static Asset<Texture2D> _cachedTexture;

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

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)ProjectileWidthHeight; // 使用放大后的值
            Projectile.height = (int)ProjectileWidthHeight; // 使用放大后的值
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = true; // 允许与物块碰撞
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1; // 自定义AI
            Projectile.noEnchantmentVisuals = true;
        }

        // 平射AI逻辑
                public override void AI()
        {
            Projectile.localAI[0]++;
            
            // 设置弹幕朝向为其移动方向
            Projectile.rotation = Projectile.velocity.ToRotation(); // 加π/4使剑刃朝前
            
            // 删除渐变增大的缩放控制，使用固定缩放
            // float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1];
            // float scaleMulti = BaseScaleMultiplier;
            // float scaleAdder = BaseScaleAdder;
            // Projectile.scale = scaleAdder + percentageOfLife * scaleMulti;
            Projectile.scale = BaseScaleAdder * Projectile.ai[2]; // 使用固定基础缩放乘以AI参数

            // 粒子效果 - 沿着飞行路径生成
            if (Main.rand.NextFloat() * 2f < Projectile.Opacity)
            {
                Vector2 dustPosition = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 dustVelocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
                
                Color dustColor = Color.Lerp(backDarkColor, Color.White, Main.rand.NextFloat() * 0.3f);
                Dust coloredDust = Dust.NewDustPerfect(dustPosition, DustID.FireworksRGB, dustVelocity, 100, dustColor, 0.4f);
                coloredDust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
                coloredDust.noGravity = true;
            }

            if (Main.rand.NextFloat() * 1.5f < Projectile.Opacity)
            {
                Vector2 sidePosition = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15f, 15f);
                Dust.NewDustPerfect(sidePosition, DustID.TintableDustLighted, Projectile.velocity * 0.1f, 100, backDarkColor * Projectile.Opacity, DustSizeMultiplier * Projectile.Opacity);
            }

            // 生命周期控制
            if (Projectile.localAI[0] >= Projectile.ai[1])
            {
                Projectile.Kill();
            }

            // 附魔视觉效果
            Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center, new Vector2(EnchantmentVisualSize * Projectile.scale, EnchantmentVisualSize * Projectile.scale));
            Projectile.EmitEnchantmentVisualsAt(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 简单的矩形碰撞检测
            return projHitbox.Intersects(targetHitbox);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 创建自定义颜色的粒子效果
            CreateCustomHitParticles(target.Hitbox);
            hit.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // 创建自定义颜色的粒子效果
            CreateCustomHitParticles(target.Hitbox);
            info.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
        }
        
        private void CreateCustomHitParticles(Rectangle hitbox)
        {
            // 自定义颜色 - 这里使用青色作为示例，您可以修改为任何颜色
            Color customColor = backDarkColor; // 与武器主色调匹配
            
            for (int i = 0; i < 5; i++) // 创建多个粒子
            {
                Vector2 spawnPos = Main.rand.NextVector2FromRectangle(hitbox);
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                
                // 创建自定义颜色的尘埃粒子
                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.TintableDustLighted, velocity, 100, customColor, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
            
            // 如果想要类似星星的效果，可以添加以下代码
            for (int i = 0; i < 3; i++)
            {
                Vector2 spawnPos = Main.rand.NextVector2FromRectangle(hitbox);
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                
                // 创建星星形状的粒子
                Dust starDust = Dust.NewDustPerfect(spawnPos, DustID.GemSapphire, velocity, 100, customColor, 0.8f);
                starDust.noGravity = true;
                starDust.noLight = false;
            }
        }

        public override void CutTiles()
        {
            // 沿着飞行路径切割瓦片
            Vector2 startPosition = Projectile.Center - Projectile.velocity * 10f;
            Vector2 endPosition = Projectile.Center + Projectile.velocity * 10f;
            float width = Projectile.width * Projectile.scale;
            Utils.PlotTileLine(startPosition, endPosition, width, DelegateMethods.CutTiles);
        }

                public override bool PreDraw(ref Color lightColor)
        {
            Vector2 position = Projectile.Center - Main.screenPosition;
            Texture2D texture = _cachedTexture.Value;
            Rectangle sourceRectangle = texture.Frame(1, 4, 0, 0); // 使用第一帧作为主体
            Vector2 origin = sourceRectangle.Size() / 2f;
            float scale = Projectile.scale * DrawScaleMultiplier;
            SpriteEffects spriteEffects = SpriteEffects.None;
            float percentageOfLife = 1f;
            // 删除渐变效果，使用固定值
            // float lerpTime = Utils.Remap(percentageOfLife, 0f, 0.6f, 0f, 1f) * Utils.Remap(percentageOfLife, 0.6f, 1f, 1f, 0f);
            float lerpTime = 1f; // 固定为最大亮度
            float lightingColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
            // 提高环境光照影响
            lightingColor = Utils.Remap(lightingColor, 0.2f, 1f, 0.3f, 1.2f); // 增加最小值和最大值

            Color whiteTimesLerpTime = Color.White * lerpTime * 0.8f; // 增加白色叠加强度
            whiteTimesLerpTime.A = (byte)(whiteTimesLerpTime.A * (1f - lightingColor));
            Color faintLightingColor = whiteTimesLerpTime * lightingColor * 0.7f; // 增加淡化层亮度
            faintLightingColor.G = (byte)(faintLightingColor.G * lightingColor);
            faintLightingColor.B = (byte)(faintLightingColor.R * (0.25f + lightingColor * 0.75f));

            // 后部层 - 增加亮度
            Main.EntitySpriteDraw(texture, position, sourceRectangle, backDarkColor * lightingColor * lerpTime * 1.3f, Projectile.rotation, origin, scale, spriteEffects, 0f);
            // 淡化层 - 增加亮度
            Main.EntitySpriteDraw(texture, position, sourceRectangle, faintLightingColor * 0.3f, Projectile.rotation, origin, scale, spriteEffects, 0f);
            // 中部层 - 增加亮度
            Main.EntitySpriteDraw(texture, position, sourceRectangle, middleMediumColor * lightingColor * lerpTime * 0.6f, Projectile.rotation, origin, scale, spriteEffects, 0f);
            // 前部层 - 增加亮度
            Main.EntitySpriteDraw(texture, position, sourceRectangle, frontLightColor * lightingColor * lerpTime * 0.8f, Projectile.rotation, origin, scale * DrawScaleFrontPart, spriteEffects, 0f);
            
            // 细线装饰层（使用第四帧）- 增加亮度
            Rectangle lineFrame = texture.Frame(1, 4, 0, 3);
            Main.EntitySpriteDraw(texture, position, lineFrame, middleMediumColor * 0.9f * lerpTime, Projectile.rotation, origin, scale, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, lineFrame, middleMediumColor * 0.7f * lerpTime, Projectile.rotation, origin, scale * DrawThinMiddleScale, spriteEffects, 0f);
            Main.EntitySpriteDraw(texture, position, lineFrame, middleMediumColor * 0.5f * lerpTime, Projectile.rotation, origin, scale * DrawThinBottomScale, spriteEffects, 0f);

            // 前端星光效果 - 增加亮度
            Vector2 drawPos = position + Projectile.rotation.ToRotationVector2() * ((float)texture.Width * 0.3f - SparkleFrontOffset) * scale;
            DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos, new Color(196, 247, 255, 0) * lerpTime * 0.8f, frontLightColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, 0f, new Vector2(2f, Utils.Remap(percentageOfLife, 0f, 1f, 4f, 1f)) * scale, Vector2.One * scale);

            return false;
        }
        protected static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness)
        {
            Texture2D sparkleTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Color bigColor = shineColor * opacity * 0.5f;
            bigColor.A = 0;
            Vector2 origin = sparkleTexture.Size() / 2f;
            Color smallColor = drawColor * 0.5f;
            float lerpValue = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);
            Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X) * lerpValue;
            Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y) * lerpValue;
            bigColor *= lerpValue;
            smallColor *= lerpValue;
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, 0f + rotation, origin, scaleUpDown, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, 0f + rotation, origin, scaleUpDown * 0.6f, dir);
        }
    }
}