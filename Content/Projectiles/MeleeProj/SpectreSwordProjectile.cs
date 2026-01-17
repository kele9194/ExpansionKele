using System;
using System.Threading;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Weapons.Melee; // 添加对新Player类的引用
using ExpansionKele.Global;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class SpectreSwordProjectile : ModProjectile
    {
        private float damageMultiplier = 1f;

        private float BaseScaleMultiplier = 0.6f;
        
        public static float baseTextureScaleMultiplier = 1.5f;
        public static float SecondTextureScaleMultiplier = 1f;
        private static float TextureScaleMultiplier = baseTextureScaleMultiplier*SecondTextureScaleMultiplier; // 贴图倍率
        private float BaseScaleAdder = 1f;
        private float DustPositionMultiplier = 84f * TextureScaleMultiplier; // 放大3倍
        private float DustRandomPositionMin = 20f * TextureScaleMultiplier; // 放大3倍
        private float DustRandomPositionMax = 80f * TextureScaleMultiplier; // 放大3倍
        private float DustSizeMultiplier = 1.2f * TextureScaleMultiplier; // 放大3倍
        private float CollisionConeLength = 94f * TextureScaleMultiplier; // 放大3倍 - 影响碰撞范围
        private float EnchantmentVisualDistance = 70f * TextureScaleMultiplier; // 放大3倍 - 影响附魔视觉效果距离
        private float EnchantmentVisualSize = 60f*TextureScaleMultiplier; // 放大3倍 - 影响附魔视觉效果大小
        private float CutTilesDistance = 60f * TextureScaleMultiplier; // 放大3倍 - 影响切割瓦片距离
        private float DrawScaleMultiplier = 1.1f * TextureScaleMultiplier; // 放大3倍 - 影响绘制时的整体尺寸
        private float DrawScaleFrontPart = 0.975f; // 移除额外放大 - 保持原始比例关系
        private float DrawThinMiddleScale = 0.8f; // 移除额外放大 - 保持原始比例关系
        private float DrawThinBottomScale = 0.6f; // 移除额外放大 - 保持原始比例关系
        private float SparkleBaseOffset = 6f; //移除额外放大 - 保持原始比例关系
        private float SparkleFrontOffset = 4f ; // 移除额外放大 - 保持原始比例关系
        private float SparkleYStart = 3f ; // 移除额外放大 - 保持原始比例关系
        private float ProjectileWidthHeight = 16f ; // 移除额外放大 - 保持原始比例关系
        // 新增：定义挥舞角度的常量
        private float SwingAngleMultiplier = 1f; // 可以调整这个值来改变挥舞角度，例如1.5f会增加50%的角度
        
        
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
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = 2700; // 放大3倍 - 检查距离也相应放大
            Projectile.usesOwnerMeleeHitCD = true;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.aiStyle = -1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            Player player = Main.player[Projectile.owner];
            
            // 获取蓄力玩家组件
            SpectreSwordPlayer swordPlayer = player.GetModPlayer<SpectreSwordPlayer>();
            // 更新贴图缩放倍数和伤害倍数
            SecondTextureScaleMultiplier = swordPlayer.spectreSwordCharge;
            damageMultiplier = swordPlayer.spectreSwordDamageCharge;
            TextureScaleMultiplier = baseTextureScaleMultiplier * SecondTextureScaleMultiplier;
            // 重新计算所有依赖于TextureScaleMultiplier的变量
            UpdateScaledVariables();

            float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1];
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation;

            float scaleMulti = BaseScaleMultiplier;
            float scaleAdder = BaseScaleAdder;

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = scaleAdder + percentageOfLife * scaleMulti;

            // Spawn dust effects during swing
            float dustRotation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f * SwingAngleMultiplier; // 添加角度乘数
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

            Projectile.scale *= Projectile.ai[2];

            if (Projectile.localAI[0] >= Projectile.ai[1])
            {
                // 项目结束时重置蓄力
                swordPlayer.ResetCharge();
                Projectile.Kill();
            }

            for (float i = -MathHelper.PiOver4 * SwingAngleMultiplier; i <= MathHelper.PiOver4 * SwingAngleMultiplier; i += MathHelper.PiOver2 * SwingAngleMultiplier) // 调整附魔视觉效果的角度范围
            {
                Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center + (Projectile.rotation + i).ToRotationVector2() * EnchantmentVisualDistance * Projectile.scale, new Vector2(EnchantmentVisualSize * Projectile.scale, EnchantmentVisualSize * Projectile.scale));
                Projectile.EmitEnchantmentVisualsAt(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
            }
        }

        // 更新所有依赖于TextureScaleMultiplier的变量
        private void UpdateScaledVariables()
        {
            DustPositionMultiplier = 84f * TextureScaleMultiplier; // 放大3倍
            DustRandomPositionMin = 20f * TextureScaleMultiplier; // 放大3倍
            DustRandomPositionMax = 80f * TextureScaleMultiplier; // 放大3倍
            DustSizeMultiplier = 1.2f * TextureScaleMultiplier; // 放大3倍
            CollisionConeLength = 94f * TextureScaleMultiplier; // 放大3倍 - 影响碰撞范围
            EnchantmentVisualDistance = 70f * TextureScaleMultiplier; // 放大3倍 - 影响附魔视觉效果距离
            EnchantmentVisualSize = 60f * TextureScaleMultiplier; // 放大3倍 - 影响附魔视觉效果大小
            CutTilesDistance = 60f * TextureScaleMultiplier; // 放大3倍 - 影响切割瓦片距离
            DrawScaleMultiplier = 1.1f * TextureScaleMultiplier; // 放大3倍 - 影响绘制时的整体尺寸
        }

        // ... existing code ...


        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float coneLength = CollisionConeLength * Projectile.scale;
            float collisionRotation = MathHelper.Pi * 2f / 25f * Projectile.ai[0];
            float maximumAngle = MathHelper.PiOver4 * SwingAngleMultiplier; // 应用角度乘数到碰撞检测
            float coneRotation = Projectile.rotation + collisionRotation;

            if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle))
            {
                return true;
            }

            float backOfTheSwing = Utils.Remap(Projectile.localAI[0], Projectile.ai[1] * 0.3f, Projectile.ai[1] * 0.5f, 1f, 0f);
            if (backOfTheSwing > 0f)
            {
                float coneRotation2 = coneRotation - MathHelper.PiOver4 * Projectile.ai[0] * backOfTheSwing * SwingAngleMultiplier; // 应用角度乘数到后半段挥舞

                if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation2, maximumAngle))
                {
                    return true;
                }
            }

            return false;
        }

        public override void CutTiles()
        {
            Vector2 starting = (Projectile.rotation - MathHelper.PiOver4 * SwingAngleMultiplier).ToRotationVector2() * CutTilesDistance * Projectile.scale; // 应用角度乘数到切瓦片
            Vector2 ending = (Projectile.rotation + MathHelper.PiOver4 * SwingAngleMultiplier).ToRotationVector2() * CutTilesDistance * Projectile.scale; // 应用角度乘数到切瓦片
            float width = CutTilesDistance * Projectile.scale;
            Utils.PlotTileLine(Projectile.Center + starting, Projectile.Center + ending, width, DelegateMethods.CutTiles);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= damageMultiplier;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.FinalDamage *= damageMultiplier;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 替换为自定义粒子效果
            // ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
            //     new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox) },
            //     Projectile.owner);

            // 创建自定义颜色的粒子效果
            CreateCustomHitParticles(target.Hitbox);

            hit.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // 替换为自定义粒子效果
            // ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
            //     new ParticleOrchestraSettings { PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox) },
            //     Projectile.owner);

            // 创建自定义颜色的粒子效果
            CreateCustomHitParticles(target.Hitbox);

            info.HitDirection = (Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1);
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

        // ... existing code ...
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 position = Projectile.Center - Main.screenPosition;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle sourceRectangle = texture.Frame(1, 4);
            Vector2 origin = sourceRectangle.Size() / 2f;
            float scale = Projectile.scale * DrawScaleMultiplier;
            SpriteEffects spriteEffects = ((!(Projectile.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
            float percentageOfLife = Projectile.localAI[0] / Projectile.ai[1];
            float lerpTime = Utils.Remap(percentageOfLife, 0f, 0.6f, 0f, 1f) * Utils.Remap(percentageOfLife, 0.6f, 1f, 1f, 0f);
            float lightingColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
            lightingColor = Utils.Remap(lightingColor, 0.2f, 1f, 0f, 1f);

            // 使用新的主色调
            Color backDarkColor = new Color(6, 106, 255); // #066aff - 最深蓝色（后部）
            Color middleMediumColor = new Color(22, 173, 254); // #16adfe - 中等亮度蓝色（中部）
            Color frontLightColor = new Color(196, 247, 255); // #c4f7ff - 最亮蓝色（前部）

            Color whiteTimesLerpTime = Color.White * lerpTime * 0.5f;
            whiteTimesLerpTime.A = (byte)(whiteTimesLerpTime.A * (1f - lightingColor));
            Color faintLightingColor = whiteTimesLerpTime * lightingColor * 0.5f;
            faintLightingColor.G = (byte)(faintLightingColor.G * lightingColor);
            faintLightingColor.B = (byte)(faintLightingColor.R * (0.25f + lightingColor * 0.75f));

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
                float edgeRotation = Projectile.rotation + Projectile.ai[0] * i * (MathHelper.Pi * -2f) * 0.025f + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4 * SwingAngleMultiplier) * Projectile.ai[0]; // 应用角度乘数到火花效果
                Vector2 drawPos = position + edgeRotation.ToRotationVector2() * ((float)texture.Width * 0.5f - SparkleBaseOffset) * scale;
                DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos, new Color(22, 173, 254, 0) * lerpTime * (i / 9f), frontLightColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, edgeRotation, new Vector2(0f, Utils.Remap(percentageOfLife, 0f, 1f, SparkleYStart, 0f)) * scale, Vector2.One * scale);
            }

            // This draws a large star sparkle at the front of the projectile. - 在剑的前端绘制一个大的星形闪烁效果
            Vector2 drawPos2 = position + (Projectile.rotation + Utils.Remap(percentageOfLife, 0f, 1f, 0f, MathHelper.PiOver4 * SwingAngleMultiplier) * Projectile.ai[0]).ToRotationVector2() * ((float)texture.Width * 0.5f - SparkleFrontOffset) * scale; // 应用角度乘数到前部火花效果
            DrawPrettyStarSparkle(Projectile.Opacity, SpriteEffects.None, drawPos2, new Color(196, 247, 255, 0) * lerpTime * 0.5f, frontLightColor, percentageOfLife, 0f, 0.5f, 0.5f, 1f, 0f, new Vector2(2f, Utils.Remap(percentageOfLife, 0f, 1f, 4f, 1f)) * scale, Vector2.One * scale);

            return false;
        }
// ... existing code ...

        private static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness)
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
            // Bright, large part
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, 0f + rotation, origin, scaleUpDown, dir);
            // Dim, small part
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
            Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, 0f + rotation, origin, scaleUpDown * 0.6f, dir);
        }
    }
}