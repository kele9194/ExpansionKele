using System;
using System.Threading;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Weapons.Melee; // 添加对新Player类的引用
using ExpansionKele.Content.Projectiles.MeleeProj;
using ExpansionKele.Global;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class SpectreSwordProjectile : EnergySwordProjectile
    {
        private float damageMultiplier = 1f;
        public static float baseTextureScaleMultiplier = 1.5f;
        public static float SecondTextureScaleMultiplier = 1f;
        
        // 重写基础属性
        protected override float TextureScaleMultiplier => baseTextureScaleMultiplier * SecondTextureScaleMultiplier;
        protected override float SwingAngleMultiplier => 1f;
        protected override float DrawScaleFrontPart => 0.975f;
        protected override float DrawThinMiddleScale => 0.8f;
        protected override float DrawThinBottomScale => 0.6f;
        protected override float SparkleBaseOffset => 6f;
        protected override float SparkleFrontOffset => 4f;
        protected override float SparkleYStart => 3f;
        protected override float ProjectileWidthHeight => 16f;

        // 重写颜色定义
        protected override Color backDarkColor => new Color(6, 106, 255); // #066aff - 最深蓝色（后部）
        protected override Color middleMediumColor => new Color(22, 173, 254); // #16adfe - 中等亮度蓝色（中部）
        protected override Color frontLightColor => new Color(196, 247, 255); // #c4f7ff - 最亮蓝色（前部）

        public override void SetDefaults()
        {
            base.SetDefaults(); // 调用基类设置
            Projectile.ownerHitCheckDistance = 2700; // 特殊的检查距离
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // 获取蓄力玩家组件
            SpectreSwordPlayer swordPlayer = player.GetModPlayer<SpectreSwordPlayer>();
            // 更新贴图缩放倍数和伤害倍数
            SecondTextureScaleMultiplier = swordPlayer.spectreSwordCharge;
            damageMultiplier = swordPlayer.spectreSwordDamageCharge;
            
            // 调用基类AI逻辑
            base.AI();

            // 特殊的尘埃效果
            float dustRotation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f * SwingAngleMultiplier;
            Vector2 dustPosition = Projectile.Center + dustRotation.ToRotationVector2() * DustPositionMultiplier * Projectile.scale;
            Vector2 dustVelocity = (dustRotation + Projectile.ai[0] * MathHelper.PiOver2).ToRotationVector2();
            
            if (Main.rand.NextFloat() * 2f < Projectile.Opacity)
            {
                Color dustColor = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat() * 0.3f);
                Dust coloredDust = Dust.NewDustPerfect(Projectile.Center + dustRotation.ToRotationVector2() * (Main.rand.NextFloat() * DustRandomPositionMax * Projectile.scale + DustRandomPositionMin * Projectile.scale), DustID.FireworksRGB, dustVelocity * 1f, 100, dustColor, 0.4f);
                coloredDust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
                coloredDust.noGravity = true;
            }

            if (Main.rand.NextFloat() * 1.5f < Projectile.Opacity)
            {
                Dust.NewDustPerfect(dustPosition, DustID.TintableDustLighted, dustVelocity, 100, Color.Cyan * Projectile.Opacity, DustSizeMultiplier * Projectile.Opacity);
            }

            if (Projectile.localAI[0] >= Projectile.ai[1])
            {
                // 项目结束时重置蓄力
                swordPlayer.ResetCharge();
                Projectile.Kill();
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= damageMultiplier;
        }
        
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.FinalDamage *= damageMultiplier;
        }

        private void CreateCustomHitParticles(Rectangle hitbox)
        {
            // 自定义颜色 - 这里使用青色作为示例，您可以修改为任何颜色
            Color customColor = new Color(6, 106, 255); // 与武器主色调匹配
            
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

        // 重写PreDraw方法以使用特殊的颜色
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 position = Projectile.Center - Main.screenPosition;
            Texture2D texture = _cachedTexture.Value;
            Rectangle sourceRectangle = texture.Frame(1, 4);
            Vector2 origin = sourceRectangle.Size() / 2f;
            float scale = Projectile.scale * DrawScaleMultiplier;
            SpriteEffects spriteEffects = ((!(Projectile.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
            float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1];
            float lerpTime = Utils.Remap(percentageOfLife, 0f, 0.6f, 0f, 1f) * Utils.Remap(percentageOfLife, 0.6f, 1f, 1f, 0f);
            float lightingColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
            lightingColor = Utils.Remap(lightingColor, 0.2f, 1f, 0f, 1f);

            Color whiteTimesLerpTime = Color.White * lerpTime * 0.5f;
            whiteTimesLerpTime.A = (byte)(whiteTimesLerpTime.A * (1f - lightingColor));
            Color faintLightingColor = whiteTimesLerpTime * lightingColor * 0.5f;
            faintLightingColor.G = (byte)(faintLightingColor.G * lightingColor);
            faintLightingColor.B = (byte)(faintLightingColor.R * (0.25f + lightingColor * 0.75f));

            // 使用重写的颜色属性
            // Back part - 后部颜色，使用最深的蓝色
            Main.EntitySpriteDraw(texture, position, sourceRectangle, backDarkColor * lightingColor * lerpTime, Projectile.rotation + Projectile.ai[0] * MathHelper.PiOver4 * -1f * (1f - percentageOfLife), origin, scale, spriteEffects, 0f);
            // Very faint part affected by the light color - 很淡的部分，基于新的主色调
            Main.EntitySpriteDraw(texture, position, sourceRectangle, faintLightingColor * 0.15f, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, spriteEffects, 0f);
            // Middle part - 中部颜色，使用中等亮度的蓝色
            Main.EntitySpriteDraw(texture, position, sourceRectangle, middleMediumColor * lightingColor * lerpTime * 0.3f, Projectile.rotation, origin, scale, spriteEffects, 0f);
            // Front part - 前部颜色，使用最亮的蓝色
            Main.EntitySpriteDraw(texture, position, sourceRectangle, frontLightColor * lightingColor * lerpTime * 0.5f, Projectile.rotation, origin, scale * DrawScaleFrontPart, spriteEffects, 0f);
            // Thin top line (final frame) - 细线顶部，调整为基于新主色调
            Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), middleMediumColor * 0.6f * lerpTime, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, spriteEffects, 0f);
            // Thin middle line (final frame) - 细线中部，调整为基于新主色调
            Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), middleMediumColor * 0.5f * lerpTime, Projectile.rotation + Projectile.ai[0] * -0.05f, origin, scale * DrawThinMiddleScale, spriteEffects, 0f);
            // Thin bottom line (final frame) - 细线底部，调整为基于新主色调
            Main.EntitySpriteDraw(texture, position, texture.Frame(1, 4, 0, 3), middleMediumColor * 0.4f * lerpTime, Projectile.rotation + Projectile.ai[0] * -0.1f, origin, scale * DrawThinBottomScale, spriteEffects, 0f);

            // This draws some sparkles around the circumference of the swing. - 绘制挥舞周围的闪烁效果
            for (float i = 0f; i < 8f; i += 1f)
            {
                float edgeRotation = Projectile.rotation + Projectile.ai[0] * i * (MathHelper.Pi * -2f) * 0.025f + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4 * SwingAngleMultiplier) * Projectile.ai[0];
                Vector2 drawPos = position + edgeRotation.ToRotationVector2() * ((float)texture.Width * 0.5f - SparkleBaseOffset) * scale;
                DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos, new Color(22, 173, 254, 0) * lerpTime * (i / 9f), frontLightColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, edgeRotation, new Vector2(0f, Utils.Remap(percentageOfLife, 0f, 1f, SparkleYStart, 0f)) * scale, Vector2.One * scale);
            }

            // This draws a large star sparkle at the front of the projectile. - 在剑的前端绘制一个大的星形闪烁效果
            Vector2 drawPos2 = position + (Projectile.rotation + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4 * SwingAngleMultiplier) * Projectile.ai[0]).ToRotationVector2() * ((float)texture.Width * 0.5f - SparkleFrontOffset) * scale;
            DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos2, new Color(196, 247, 255, 0) * lerpTime * 0.5f, frontLightColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, 0f, new Vector2(2f, Utils.Remap(percentageOfLife, 0f, 1f, 4f, 1f)) * scale, Vector2.One * scale);

            return false;
        }
    }
}