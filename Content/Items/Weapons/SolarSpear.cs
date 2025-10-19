using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Weapons
{
    public class SolarSpear : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = ExpansionKele.ATKTool(180, 210);
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Yellow;
            Item.knockBack = 7f;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SolarSpearProjectile>();
            Item.shootSpeed = 5f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // // 添加自定义tooltip
            // tooltips.Add(new TooltipLine(Mod, "SolarSpearInfo", Language.GetText("Mods.ExpansionKele.Items.SolarSpear.Info1").Value));
            // tooltips.Add(new TooltipLine(Mod, "SolarSpearInfo2", Language.GetText("Mods.ExpansionKele.Items.SolarSpear.Info2").Value));
            // tooltips.Add(new TooltipLine(Mod, "SolarSpearInfo3", Language.GetText("Mods.ExpansionKele.Items.SolarSpear.Info3").Value));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SigLightSpear>())
                .AddIngredient(ModContent.ItemType<PearlWoodSpear>())
                .AddIngredient(ItemID.FragmentSolar, 6)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class SolarSpearProjectile : ModProjectile
    {
        // 阶段 0: 冲刺阶段
        // 阶段 1: 旋转攻击阶段
        private const int PHASE_DASH = 0;
        private const int PHASE_SPIN = 1;

        // 生命恢复常量
        private const float PROJECTILE_REFLECT_HEAL_PERCENT = 0.01f;  // 反弹弹幕恢复1%生命值
        private const float DASH_ENEMY_PENETRATE_HEAL_PERCENT = 0.01f; // 冲刺穿透敌人恢复1%生命值

        public int phase;
        public int timer;
        public float dashSpeed = 40f;
        public float BaseRadius;
        
        // 冲刺相关变量
        private Vector2 dashVelocity = Vector2.Zero;
        private bool isDashing = false;
        private bool dashBlocked = false;
        
        // 旋转攻击相关变量
        private float rotationSpeed;
        private float currentRotation;
        
        // 冲刺击中计数
        private int dashHitCount = 0;
        private List<int> hitNPCs = new List<int>(); // 用于跟踪已经击中的NPC

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 初始化阶段为冲刺阶段
            phase = PHASE_DASH;
            timer = 0;
            Projectile.ai[0] = PHASE_DASH;
            Projectile.ai[1] = 0;
            
            // 获取贴图尺寸并计算BaseRadius
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int textureLength = Math.Max(texture.Width, texture.Height);
            BaseRadius = textureLength * 1.4142f / 2f; // 图片最大尺寸 * sqrt(2) / 2
            
            // 初始化冲刺击中计数器
            dashHitCount = 0;
            hitNPCs.Clear();
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.noKnockback = true;

            // 获取该抛射体对应的物品使用时间
            int useTime = 24;
            Item item = player.HeldItem;
            if (item.type == ModContent.ItemType<SolarSpear>())
            {
                useTime = item.useTime;
            }

            // 更新阶段和计时器
            phase = (int)Projectile.ai[0];
            timer = (int)Projectile.ai[1]++;

            // 每个阶段持续时间
            int halfUseTime = useTime / 2;

            if (phase == PHASE_DASH)
            {
                HandleDashPhase(player, useTime, halfUseTime);
            }
            else if (phase == PHASE_SPIN)
            {
                HandleSpinPhase(player, useTime, halfUseTime);
            }

            // 检查是否完成当前阶段，切换到下一阶段
            if (timer >= halfUseTime - 1)
            {
                if (phase == PHASE_DASH)
                {
                    // 从冲刺阶段切换到旋转阶段
                    Projectile.ai[0] = PHASE_SPIN;
                    Projectile.ai[1] = 0;
                    Projectile.netUpdate = true;
                    
                    // 重置冲刺击中计数器
                    dashHitCount = 0;
                    hitNPCs.Clear();
                }
                else if (phase == PHASE_SPIN)
                {
                    // 旋转阶段结束后销毁抛射体，完成一个完整的使用周期
                    Projectile.Kill();
                }
            }
        }

        private void HandleDashPhase(Player player, int useTime, int halfUseTime)
        {
            if (timer == 0)
            {
                // 开始冲刺
                // 移除玩家的坐骑和钩爪
                player.mount?.Dismount(player);
                player.RemoveAllGrapplingHooks();

                // 计算冲刺方向（朝向鼠标）
                Vector2 direction = player.Center.DirectionTo(Main.MouseWorld);
                dashVelocity = direction * dashSpeed;
                isDashing = true;
                dashBlocked = false;

                // 设置抛射体初始位置
                Projectile.Center = player.Center;

                // 播放冲刺音效
                SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
            }

            if (isDashing && !dashBlocked)
            {
                // 冲刺期间无敌
                player.immune = true;
                player.immuneTime = halfUseTime+1;
                var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
                reductionPlayer.MulticustomDamageReduction(0.01f);
                
                // 检查下一个位置是否会碰撞到方块
                Vector2 nextPosition = Projectile.Center + dashVelocity;
                Point nextTilePoint = (nextPosition + dashVelocity.SafeNormalize(Vector2.Zero) * player.width / 2).ToTileCoordinates();
                if (WorldGen.SolidOrSlopedTile(nextTilePoint.X, nextTilePoint.Y))
                {
                    // 如果被阻挡，则停止冲刺
                    dashBlocked = true;
                    isDashing = false;
                    
                    // 减速并恢复重力
                    dashVelocity *= 0.2f;
                    player.gravity = Player.defaultGravity;
                }
                else
                {
                    // 更新抛射体位置
                    Projectile.velocity = dashVelocity;

                    // 让玩家跟随抛射体
                    player.Center = Projectile.Center;
                }

                // 添加冲刺尾焰
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(
                        player.position,
                        player.width,
                        player.height,
                        DustID.SolarFlare,
                        -dashVelocity.X * 0.1f,
                        -dashVelocity.Y * 0.1f,
                        100,
                        default(Color),
                        1.5f
                    );
                    dust.noGravity = true;
                    dust.scale = 2.5f;
                }

                // 冲刺过程中允许微调方向
                if (timer > 0 && timer < halfUseTime - 1 && !dashBlocked)
                {
                    float rotationStrength = (float)Math.PI / 40f * (float)Math.Pow((double)((float)timer / (halfUseTime - 1)), 2.0);
                    float currentRotation = dashVelocity.ToRotation();
                    float idealRotation = player.Center.DirectionTo(Main.MouseWorld).ToRotation();
                    dashVelocity = currentRotation.AngleTowards(idealRotation, rotationStrength).ToRotationVector2();
                    dashVelocity *= dashSpeed * (0.24f + 0.76f * (float)Math.Sin((double)((float)Math.PI * timer / (halfUseTime - 1))));
                }
            }

            // 冲刺持续时间占总使用时间的一半
            if (timer >= halfUseTime - 1 && !dashBlocked)
            {
                isDashing = false;

                // 冲刺完成后减速
                dashVelocity *= 0.2f;

                // 恢复重力
                player.gravity = Player.defaultGravity;
            }

            // 在冲刺阶段保持长矛朝向
            Vector2 vector = dashVelocity.SafeNormalize(Vector2.Zero);
            Projectile.rotation = dashVelocity.ToRotation() + MathHelper.PiOver4;
            player.ChangeDir(dashVelocity.X > 0 ? 1 : -1);
            player.itemRotation = (dashVelocity * player.direction).ToRotation();
            Projectile.Center = player.Center;
        }

        private void HandleSpinPhase(Player player, int useTime, int halfUseTime)
        {
            if (timer == 0)
            {
                // 初始化旋转参数
                rotationSpeed = MathHelper.TwoPi / halfUseTime;
                currentRotation = 0f;
            }
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.MulticustomDamageReduction(0.5f);
            player.gravity=0;


            // 计算旋转角度
            currentRotation = rotationSpeed * timer;

            // 设置抛射体位置和旋转
            Vector2 direction = player.Center.DirectionTo(Main.MouseWorld);
            float distanceFromPlayer = 0f;
            Projectile.Center = player.Center + currentRotation.ToRotationVector2() * distanceFromPlayer;
            Projectile.rotation = currentRotation + MathHelper.PiOver4;

            // 设置玩家面向和物品旋转
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            player.itemRotation = (Projectile.velocity * player.direction).ToRotation();

            // 检查附近的敌对弹幕并尝试反弹
            TryReflectProjectiles(player);
        }

        private void TryReflectProjectiles(Player player)
        {
            // 获取武器纹理尺寸
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float weaponRadius = Math.Max(texture.Width, texture.Height) / 2f * Projectile.scale;
            Vector2 weaponCenter = Projectile.Center;

            // 遍历所有弹幕
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.hostile && !proj.friendly)
                {
                    // 计算弹幕中心到武器中心的距离
                    float distance = Vector2.Distance(proj.Center, weaponCenter);

                    // 检查弹幕是否在武器范围内
                    if (distance <= weaponRadius + Math.Max(proj.width, proj.height) / 2f)
                    {
                        // 50%概率反弹弹幕
                        if (Main.rand.NextBool(2))
                        {
                            // 将敌对弹幕转换为友方弹幕
                            proj.hostile = false;
                            proj.friendly = true;

                            // 反转弹幕速度方向
                            proj.velocity = -proj.velocity;

                            // 设置弹幕伤害为武器伤害的一半
                            proj.damage = Projectile.damage;

                            // 设置弹幕所有者为玩家
                            proj.owner = Projectile.owner;

                            // 添加视觉效果
                            for (int j = 0; j < 10; j++)
                            {
                                Dust dust = Dust.NewDustDirect(
                                    proj.position,
                                    proj.width,
                                    proj.height,
                                    DustID.SolarFlare,
                                    0,
                                    0,
                                    100,
                                    default(Color),
                                    1.2f
                                );
                                dust.noGravity = true;
                                dust.velocity *= 2f;
                            }
                            
                            // 每反弹一次弹幕恢复2%最大生命值
                            int healAmount = (int)(player.statLifeMax2 * PROJECTILE_REFLECT_HEAL_PERCENT);
                            player.Heal(healAmount);
                        }
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 position = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (phase == PHASE_DASH)
            {
                // 冲刺阶段使用线段碰撞检测，线段中心在Projectile.Center
                float collisionPoint = 0f;
                Vector2 dashDirection = dashVelocity.SafeNormalize(Vector2.Zero);
                Vector2 spearStart = Projectile.Center - dashDirection * (BaseRadius / 2f); // 线段起点
                Vector2 spearEnd = Projectile.Center + dashDirection * (BaseRadius / 2f);   // 线段终点
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), spearStart, spearEnd, 22f * Projectile.scale, ref collisionPoint))
                {
                    return true;
                }
            }
            else if (phase == PHASE_SPIN)
            {
                // 旋转阶段使用线段碰撞检测
                float spinning = currentRotation; // 使用当前旋转角度
                float staffRadiusHit = BaseRadius; // 旋转半径
                float useless = 0f;
                Vector2 spinDirection = spinning.ToRotationVector2();
                if (Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(), 
                    targetHitbox.Size(), 
                    Projectile.Center + spinDirection * -staffRadiusHit, 
                    Projectile.Center + spinDirection * staffRadiusHit, 
                    22f * Projectile.scale, 
                    ref useless))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool ShouldUpdatePosition() => phase == PHASE_DASH && timer > 0 && !dashBlocked;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 冲刺阶段每击中一个敌人计数
            if (phase == PHASE_DASH && !dashBlocked)
            {
                // 检查是否已经击中过这个NPC，避免重复计数
                if (!hitNPCs.Contains(target.whoAmI))
                {
                    hitNPCs.Add(target.whoAmI);
                    dashHitCount++;
                    
                    // 每穿透一个敌人恢复1.5%最大生命值
                    Player player = Main.player[Projectile.owner];
                    int healAmount = (int)(player.statLifeMax2 * DASH_ENEMY_PENETRATE_HEAL_PERCENT);
                    player.Heal(healAmount);
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 冲刺阶段造成2倍伤害
            if (phase == PHASE_DASH && !dashBlocked)
            {
                modifiers.FinalDamage *= 2f;
            }
        }
    }
}