using ExpansionKele.Content.Projectiles.RangedProj;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class LaserCutter : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(420, 540); // 每道激光每帧的伤害
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 10;
            Item.useAnimation = 10; 
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true; // 不产生近战碰撞框
            Item.knockBack = 0.1f;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item12; // 激光声音
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FFLaser>();
            Item.shootSpeed = 15f; // 激光不需要速度
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 计算新的抛射体生成位置：玩家中心 + 方向 * 56像素
            Vector2 newSpawnPosition = player.MountedCenter + velocity.SafeNormalize(Vector2.UnitX) * 56f;
            
            // 计算射线的终点（足够远的距离）
            Vector2 endPosition = newSpawnPosition + velocity.SafeNormalize(Vector2.UnitX) * 2000f;
            
            // 查找射线上的第一个可攻击NPC
            int targetNPCIndex = -1;
            float closestDistance = float.MaxValue;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal || npc.life <= 0)
                    continue;
                
                // 使用Collision.CheckAABBvLineCollision直接检测射线与NPC碰撞框是否相交
                float collisionPoint = 0f;
                if (Collision.CheckAABBvLineCollision(npc.getRect().TopLeft(), npc.getRect().Size(), newSpawnPosition, endPosition, 0f, ref collisionPoint))
                {
                    if (collisionPoint < closestDistance)
                    {
                        closestDistance = collisionPoint;
                        targetNPCIndex = npc.whoAmI;
                    }
                }
            }
            
            // 创建弹幕并传入目标NPC索引
            Projectile projectile = Projectile.NewProjectileDirect(source, newSpawnPosition, velocity, type, damage, knockback, player.whoAmI,targetNPCIndex);
            
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LaserMachinegun, 1);
            recipe.AddIngredient(ItemID.FragmentVortex, 3);
            recipe.Register();
        }
    }
}