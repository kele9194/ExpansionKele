using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class PearlWoodSpear : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetDefaults()
        {
            //Item.SetNameOverride("珍珠木长矛");
            Item.width = 76;
            Item.height = 76;
            Item.damage = ExpansionKele.ATKTool(120,150);
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 0, 50, 0);
            Item.rare = ItemRarityID.Green;
            Item.knockBack = 5f;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PearlWoodSpearProjectile>();
            Item.shootSpeed = 4f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 添加自定义tooltip
            // TooltipLine tooltip = new TooltipLine(Mod, "ReflectInfo", "旋转使用,攻击时有概率反弹敌对弹幕\n使用时自身承受伤害大幅减少\n"+
            // "旋转攻击反弹弹幕回复生命值")
            // {
            //     OverrideColor = new Color(100, 200, 255)
            // };
            // tooltips.Add(tooltip);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Pearlwood, 12)
                .AddIngredient(ItemID.LifeCrystal, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class PearlWoodSpearProjectile : ModProjectile
    {
        public float BaseRadius;
        public float distanceFromPlayerAll = 20f;
        private const float PROJECTILE_REFLECT_HEAL_PERCENT = 0.015f; // 每次反弹恢复2%最大生命值
        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 获取贴图尺寸并计算BaseRadius
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int textureLength = Math.Max(texture.Width, texture.Height);
            BaseRadius = textureLength * 1.4142f / 2f+distanceFromPlayerAll*1.4142f; // 图片最大尺寸 * sqrt(2) / 2
            
            // ... existing code ...
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.noKnockback=true;

            // 获取该抛射体对应的物品使用时间
            int useTime = 16;
            Item item = player.HeldItem;
            if (item.type == ModContent.ItemType<PearlWoodSpear>())
            {
                useTime = item.useTime;
            }

            // 计算旋转角度
            float rotationSpeed = MathHelper.TwoPi / useTime;
            float currentRotation = rotationSpeed * Projectile.ai[1];
            
            // 更新旋转角度
            Projectile.ai[1]++;
            
            // 设置抛射体位置和旋转
            Vector2 direction = player.Center.DirectionTo(Main.MouseWorld);
            float distanceFromPlayer = distanceFromPlayerAll;
            Projectile.Center = player.Center + currentRotation.ToRotationVector2() * distanceFromPlayer;
            Projectile.rotation = currentRotation + MathHelper.PiOver4;
            
            // 设置玩家面向和物品旋转
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            player.itemRotation = (Projectile.velocity * player.direction).ToRotation();
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.MulticustomDamageReduction(0.40f);
            
            // 检查是否完成一次完整旋转
            if (Projectile.ai[1] >= useTime)
            {
                Projectile.Kill();
            }
            
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
                                    DustID.MagicMirror,
                                    0,
                                    0,
                                    100,
                                    default(Color),
                                    1.2f
                                );
                                dust.noGravity = true;
                                dust.velocity *= 2f;
                            }
                            
                            // 每成功反弹一次弹幕恢复2%最大生命值
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

            Main.EntitySpriteDraw(texture, position, null, lightColor, Projectile.rotation, origin, Projectile.scale*1f, effects, 0);

            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 使用线段碰撞检测
            float spinning = Projectile.rotation - MathHelper.PiOver4; // 使用当前旋转角度
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
            return false;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 可以在这里添加特殊效果
        }
    }
}