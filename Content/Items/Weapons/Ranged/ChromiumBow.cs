using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class ChromiumBow : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(10,12);                        // 基础伤害值
            Item.DamageType = DamageClass.Ranged;    // 远程伤害类型
            Item.width = 20;                         // 物品宽高
            Item.height = 40;
            Item.useTime = 20;                       // 使用时间（帧数）
            Item.useAnimation = 20;                  // 动画持续时间
            Item.useStyle = ItemUseStyleID.Shoot;    // 使用样式为射击
            Item.noMelee = true;                     // 关闭近战攻击判定
            Item.knockBack = 1;                      // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); // 稀有度
            Item.UseSound = SoundID.Item5;           // 射击音效
            Item.autoReuse = false;                  // 自动重用
            Item.shoot = ProjectileID.WoodenArrowFriendly; // 默认发射物
            Item.shootSpeed = 8f;                    // 初始弹速
            Item.useAmmo = AmmoID.Arrow;             // 弹药类型为箭类
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float speed = velocity.Length(); // 获取原始速度大小
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX); // 获取方向向量
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X); // 垂直于运动方向的向量（向上）

            // 计算并排位置偏移（垂直于运动方向）
            float offsetDistance = 8f; // 并排间距
            
            // 左箭位置（向左偏移）
            Vector2 leftPosition = position + perpendicular * offsetDistance;
            // 中间箭位置（原位）
            Vector2 centerPosition = position;
            // 右箭位置（向右偏移）
            Vector2 rightPosition = position - perpendicular * offsetDistance;

            // ... existing code ...
            // 发射三支箭（并排）
            int proj1=Projectile.NewProjectile(source, leftPosition, velocity, type, damage, knockback, player.whoAmI);
            int proj2=Projectile.NewProjectile(source, centerPosition, velocity, type, damage, knockback, player.whoAmI);
            int proj3=Projectile.NewProjectile(source, rightPosition, velocity, type, damage, knockback, player.whoAmI);

            // 处理三个弹丸的无敌帧设置
            int[] projectiles = {proj1, proj2, proj3};
            foreach(int projIndex in projectiles)
            {
                Main.projectile[projIndex].ArmorPenetration=10;
                // 检查是否使用了静态无敌帧，如果是则切换到局部无敌帧
                if (Main.projectile[projIndex].usesIDStaticNPCImmunity)
                {
                    Main.projectile[projIndex].usesLocalNPCImmunity = true;
                    Main.projectile[projIndex].localNPCHitCooldown = Main.projectile[projIndex].idStaticNPCHitCooldown;
                    // 关闭静态无敌帧，开启局部无敌帧
                    Main.projectile[projIndex].usesIDStaticNPCImmunity = false;
                    
                    // 使用静态无敌帧的冷却值作为局部无敌帧冷却值
                    
                }
                else
                {
                    // 如果已有局部无敌帧设置且不是默认值-2，则保持不变；否则启用局部无敌帧
                    if (!Main.projectile[projIndex].usesLocalNPCImmunity && Main.projectile[projIndex].localNPCHitCooldown == -2)
                    {
                        Main.projectile[projIndex].usesLocalNPCImmunity = true;
                        Main.projectile[projIndex].localNPCHitCooldown = -1;
                    }
                }
            }
            // ... existing code ...
            return false; // 返回false以防止原版箭矢也被发射
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Items.Placeables.ChromiumBar>(), 12) // 使用12个铬钢锭
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}