using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ExpansionKele.Content.Projectiles.RangedProj;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class M37Shotgun : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {   //只有不使用子弹和箭才才使用该专家武器词条
            //ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(40,46);               // 伤害25
            Item.DamageType = DamageClass.Ranged;   // 远程伤害类型
            Item.width = 60;                        // 物品宽度
            Item.height = 24;
            Item.useTime = 30;                      // 使用时间30
            Item.useAnimation = 30;                 // 动画持续时间
            Item.useStyle = ItemUseStyleID.Shoot;   // 使用样式为射击
            Item.noMelee = true;                    // 关闭近战攻击判定
            Item.knockBack = 1f;                    // 击退值
            Item.value = Item.buyPrice(silver: 50); // 价值
            Item.rare = ItemRarityID.Blue;          // 稀有度
            Item.UseSound = SoundID.Item36;         // 射击音效
            Item.autoReuse = false;                 // 不自动重用
            Item.shoot = ProjectileID.Bullet;       // 默认发射物
            Item.shootSpeed = 8f;                   // 初始弹速
            Item.useAmmo = AmmoID.Bullet;           // 弹药类型为子弹
        }

        public override bool AltFunctionUse(Player player)
        {
            return true; // 允许右键切换模式
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 左键点击发射霰弹，右键点击发射独头弹
            bool isSlugMode = player.altFunctionUse == 2; // 右键点击时为独头弹模式

            if (!isSlugMode) // 左键：散弹模式
            {
                // 发射5发弹丸，偏转角度分别为-10°、-5°、0°、+5°、+10°
                float[] angles = { -8f, -4f, 0f, 4f, 8f };

                for (int i = 0; i < angles.Length; i++)
                {
                    float angle = angles[i];
                    
                    // 对于非0度的弹丸，添加±2度的随机偏转
                    if (Math.Abs(angle) > 0.1f)
                    {
                        angle += Main.rand.NextFloat(-2f, 2f);
                    }
                    
                    // 将角度转换为弧度并应用到速度向量
                    float radians = MathHelper.ToRadians(angle);
                    Vector2 rotatedVelocity = velocity.RotatedBy(radians);
                    
                    // 计算发射位置的小偏移，避免所有弹丸完全重叠，暂时不设置偏移
                    Vector2 offset = Vector2.UnitX.RotatedBy(radians) * 0f;
                    
                    Projectile.NewProjectile(
                        source,
                        position + offset,
                        rotatedVelocity,
                        type,
                        damage,
                        knockback,
                        player.whoAmI
                    );
                }
            }
            else // 右键：独头弹模式
            {
                // 应用伤害和击退修正
                int adjustedDamage = (int)(damage * 4f); // 400% 伤害
                float adjustedKnockback = knockback * 3f; // 3倍击退
                
                // 创建带修正的弹丸，使用相同的弹丸类型但通过ai0参数标记为独头弹模式
                int proj = Projectile.NewProjectile(
                    source,
                    position,
                    velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-2f, 2f))), // ±2度偏差
                    type,
                    adjustedDamage,
                    adjustedKnockback,
                    player.whoAmI,
                    0f, // ai0 = 0 (不再使用特殊值标记)
                    0f   // ai1 = 0
                );

                // 设置独头弹的特殊属性
                if (Main.projectile.IndexInRange(proj))
                {
                    Main.projectile[proj].scale = 1.5f; // 1.5倍大小
                    Main.projectile[proj].ArmorPenetration += 20; // 护甲穿透+20
                    
                    // 使用新方法标记独头弹模式
                    Main.projectile[proj].GetGlobalProjectile<M37SlugModeGlobalProjectile>().IsM37SlugMode = true;
                }
            }

            return false; // 阻止默认射击行为
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10f, -2f); // 持有偏移量
        }

        public override void AddRecipes()
        {
            // 示例合成配方，可以根据需要修改
            CreateRecipe()
                .AddIngredient(ItemID.IllegalGunParts, 3)
                .AddIngredient(ItemID.Shotgun, 1)
                .AddIngredient(ItemID.ChlorophyteBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class M37SlugModeGlobalProjectile : GlobalProjectile
    {
        // 更清晰地标识是否为M37独头弹模式
        public bool IsM37SlugMode { get; set; } = false;
        
        public override bool InstancePerEntity => true;
        
        public override void SetDefaults(Projectile projectile)
        {
            // 初始化时不需要做任何事
        }

        public override void AI(Projectile projectile)
        {
            // 检查是否是M37独头弹模式的弹丸
            if (IsM37SlugMode)
            {
                // 添加一些视觉效果，如轨迹
                if (Main.rand.NextBool(3))
                {
                    var dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 
                        DustID.Smoke, 0f, 0f, 100, default, 1f);
                    dust.noGravity = true;
                    dust.velocity *= 0.5f;
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 检查是否是M37独头弹模式的弹丸
            if (IsM37SlugMode)
            {
                // // 每穿透一个目标，速度减少50%
                // projectile.velocity *= 0.5f;
                // Main.NewText("修改成功");
            }
        }
    }
}