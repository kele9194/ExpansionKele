using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Localization;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Weapons
{
    /// <summary>
    /// 赛格光矛 - 一种近战武器
    /// 使用时会向前冲刺，冲刺时造成2倍伤害，冲刺时无敌，冲刺敌人回复生命值
    /// </summary>
    public class SigLightSpear : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        /// <summary>
        /// 设置物品的基础属性
        /// 包括伤害、使用时间、稀有度等
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("赛格光矛");
            Item.width = 40;
            Item.height = 40;
            Item.damage = ExpansionKele.ATKTool(100,125);
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 2, 50, 0);
            Item.rare = ItemRarityID.Orange;
            Item.knockBack = 6f;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LightSpearProjectile>();
            Item.shootSpeed = 5f;
        }

        /// <summary>
        /// 允许使用物品的副功能（右键）
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <returns>是否允许使用副功能</returns>
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        /// <summary>
        /// 添加物品合成配方
        /// 可以使用金锭或铂金锭合成
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 5)
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddIngredient(ItemID.LifeCrystal, 2)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.PlatinumBar, 5)
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddIngredient(ItemID.LifeCrystal, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
        
        /// <summary>
        /// 修改物品的提示信息
        /// 添加关于冲刺功能的说明
        /// </summary>
        /// <param name="tooltips">提示信息列表</param>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // // 添加自定义tooltip
            // TooltipLine tooltip = new TooltipLine(Mod, "DashInfo", Language.GetText("Mods.ExpansionKele.Items.SigLightSpear.DashInfo").Value)
            // {
            //     OverrideColor = new Color(100, 200, 255) // 设置为蓝色以突出显示
            // };
            // tooltips.Add(tooltip);
        }
    }

    /// <summary>
    /// 赛格光矛弹幕 - 处理光矛的冲刺逻辑和碰撞检测
    /// </summary>
    public class LightSpearProjectile : ModProjectile
    {
        // 添加BaseRadius变量用于碰撞检测
        public float BaseRadius;
        
        // 生命恢复常量
        private const float DASH_ENEMY_PENETRATE_HEAL_PERCENT = 0.0175f; // 穿刺敌人恢复2%生命值

        public int phase;
        public int timer;
        public float dashSpeed = 40f;
        // 存储冲刺方向和速度
        private Vector2 dashVelocity = Vector2.Zero;
        // 标记是否处于冲刺状态
        private bool isDashing = false;
        // 标记冲刺是否被阻挡
        private bool dashBlocked = false;
        
        // 冲刺击中计数
        private List<int> hitNPCs = new List<int>(); // 用于跟踪已经击中的NPC
        
        /// <summary>
        /// 设置弹幕的基础属性
        /// </summary>
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
            Projectile.localNPCHitCooldown = 2;
        }

        /// <summary>
        /// 弹幕生成时的初始化处理
        /// </summary>
        /// <param name="source">生成来源</param>
        public override void OnSpawn(IEntitySource source)
        {
            // 获取贴图尺寸并计算BaseRadius
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int textureLength = Math.Max(texture.Width, texture.Height);
            BaseRadius = textureLength * 1.4142f / 2f; // 图片最大尺寸 * sqrt(2) / 2
            
            // 初始化冲刺击中计数器
            hitNPCs.Clear();
            
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
        }

        /// <summary>
        /// 弹幕AI逻辑处理
        /// 包括冲刺阶段和休息阶段
        /// </summary>
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.noKnockback=true;

            // AI阶段:
            // 0 - 冲刺阶段 (Dash Phase)
            // 1 - 休息阶段 (Rest Phase)
            phase = (int)Projectile.ai[0];
            timer = (int)Projectile.ai[1]++;

            // 获取该抛射体对应的物品使用时间
            int useTime = 20;
            Item item = player.HeldItem;
            if (item.type == ModContent.ItemType<SigLightSpear>())
            {
                useTime = item.useTime;
            }

            // 冲刺和休息各占一半时间
            int halfUseTime = useTime / 2;

            if (phase == 0)
            {
                // 冲刺阶段 (Dash Phase) - 抛射体朝鼠标方向快速移动，玩家跟随
                if (timer == 0)
                {
                    // 移除玩家的坐骑和钩爪
                    player.mount?.Dismount(player);
                    player.RemoveAllGrapplingHooks();
                    
                    // 计算冲刺方向（朝向鼠标）
                    Vector2 direction = player.Center.DirectionTo(Main.MouseWorld);
                    dashVelocity = direction * dashSpeed; // 冲刺速度为25
                    isDashing = true;
                    dashBlocked = false;
                    
                    // 设置抛射体初始位置
                    Projectile.Center = player.Center;
                    
                    // 给玩家添加50%伤害减免
                    var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
                    reductionPlayer.MulticustomDamageReduction(0.5f);
                    
                    // 禁用重力影响
                    player.gravity = 0f;
                }

                if (isDashing && !dashBlocked)
                {
                    player.immune = true;
                    player.immuneTime = 2;
                    // 检查下一个位置是否会碰撞到方块
                    Vector2 nextPosition = Projectile.Center + dashVelocity;
                    Point nextTilePoint = (nextPosition + dashVelocity.SafeNormalize(Vector2.Zero) * player.width/2).ToTileCoordinates();
                    if (WorldGen.SolidOrSlopedTile(nextTilePoint.X, nextTilePoint.Y))
                    {
                        // 如果被阻挡，则停止冲刺
                        dashBlocked = true;
                        isDashing = false;
                        Projectile.ai[0] = 1; // 进入休息阶段
                        Projectile.ai[1] = 0;
                        Projectile.netUpdate = true;
                        
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
                    
                    // 添加蓝色冲刺尾焰
                    if (Main.rand.NextBool(2))
                    {
                        Dust dust = Dust.NewDustDirect(
                            player.position,
                            player.width,
                            player.height,
                            DustID.BlueFlare,
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
                        float rotationStrenght = (float)Math.PI / 40f * (float)Math.Pow((double)((float)timer / (halfUseTime - 1)), 2.0);
                        float currentRotation = dashVelocity.ToRotation();
                        float idealRotation = player.Center.DirectionTo(Main.MouseWorld).ToRotation();
                        dashVelocity = currentRotation.AngleTowards(idealRotation, rotationStrenght).ToRotationVector2();
                        dashVelocity *= dashSpeed * (0.24f + 0.76f * (float)Math.Sin((double)((float)Math.PI * timer / (halfUseTime - 1))));
                    }
                }

                // 冲刺持续时间占总使用时间的一半
                if (timer >= halfUseTime - 1 && !dashBlocked)
                {
                    Projectile.ai[0] = 1; // 进入休息阶段
                    Projectile.ai[1] = 0;
                    Projectile.netUpdate = true;
                    isDashing = false;
                    
                    // 冲刺完成后减速
                    dashVelocity *= 0.2f;
                    
                    // 恢复重力
                    player.gravity = Player.defaultGravity;
                }
                
                // 在冲刺阶段保持长矛朝向
                Vector2 vector = dashVelocity.SafeNormalize(Vector2.Zero);

                // 旋转角度
                Projectile.rotation = dashVelocity.ToRotation() + MathHelper.PiOver4;
                player.ChangeDir(dashVelocity.X > 0 ? 1 : -1);
                player.itemRotation = (dashVelocity * player.direction).ToRotation();

                // 位置调整
                Projectile.Center = player.Center;
            }
            else if (phase == 1)
            {
                // 休息阶段 (Rest Phase) - 玩家手持长矛对准鼠标，但不执行冲刺
                Vector2 vector = player.MountedCenter.DirectionTo(Main.MouseWorld);
                
                // 旋转角度
                Projectile.rotation = vector.ToRotation() + MathHelper.PiOver4;
                player.ChangeDir(vector.X > 0 ? 1 : -1);
                player.itemRotation = (vector * player.direction).ToRotation();

                // 位置调整
                Projectile.Center = player.MountedCenter;

                // 休息阶段持续剩余时间
                if (timer >= useTime-halfUseTime-1)
                {
                    Projectile.Kill();
                }
            }
        }

        /// <summary>
        /// 绘制弹幕
        /// </summary>
        /// <param name="lightColor">光照颜色</param>
        /// <returns>是否继续绘制</returns>
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 position = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }

        /// <summary>
        /// 是否应该更新弹幕位置
        /// </summary>
        /// <returns>是否更新位置</returns>
        public override bool ShouldUpdatePosition() => phase == 0 && timer > 0 && !dashBlocked; // 只在冲刺阶段且未被阻挡时更新位置

        /// <summary>
        /// 击中NPC时的处理
        /// </summary>
        /// <param name="target">目标NPC</param>
        /// <param name="hit">击中信息</param>
        /// <param name="damageDone">造成的伤害</param>
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 冲刺阶段每击中一个敌人计数
            if (phase == 0 && !dashBlocked)
            {
                // 检查是否已经击中过这个NPC，避免重复计数
                if (!hitNPCs.Contains(target.whoAmI))
                {
                    hitNPCs.Add(target.whoAmI);
                    
                    // 每穿透一个敌人恢复2%最大生命值
                    Player player = Main.player[Projectile.owner];
                    int healAmount = (int)(player.statLifeMax2 * DASH_ENEMY_PENETRATE_HEAL_PERCENT);
                    player.Heal(healAmount);
                }
            }
        }
        
        /// <summary>
        /// 检测弹幕与目标的碰撞
        /// </summary>
        /// <param name="projHitbox">弹幕碰撞框</param>
        /// <param name="targetHitbox">目标碰撞框</param>
        /// <returns>是否发生碰撞</returns>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            Vector2 dashDirection = dashVelocity.SafeNormalize(Vector2.Zero);
            Vector2 spearStart = Projectile.Center - dashDirection * (BaseRadius / 2f); // 线段起点
            Vector2 spearEnd = Projectile.Center + dashDirection * (BaseRadius / 2f);   // 线段终点
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), spearStart, spearEnd, 22f * Projectile.scale, ref collisionPoint))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 修改对NPC的伤害
        /// </summary>
        /// <param name="target">目标NPC</param>
        /// <param name="modifiers">伤害修饰符</param>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if(phase == 0 && !dashBlocked)
            {
                modifiers.FinalDamage *= 2f;
            }
        }
    }
}