using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles.RangedProj;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Weapons.Generic;
using ExpansionKele.Content.Items.OtherItem;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class SeitaadBallista : ModItem,IChargeableItem
    {
        // EMP弹丸冷却计时器
        private int empCooldown = 0;
        // 冷却时间（3 * Item.useTime）
        private const int EMP_COOLDOWN_TIME = 3;

        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            // 基本属性设置
            Item.damage = ExpansionKele.ATKTool(90,120); // 修改为80点伤害
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SeitaadBallistaProjectile>(); // 默认弹丸类型
            Item.shootSpeed = 12f;
        }

        public override Vector2? HoldoutOffset() {  
            return new Vector2(-16f, 4f); // 持有偏移量。  
        }  

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EMPPistol>(), 1) // 添加EMPPistol
                .AddIngredient(ModContent.ItemType<ElectricBallista>(), 1) // 添加ElectricBallista
                .AddIngredient(ModContent.ItemType<GolemShard>(), 3) // 添加石巨人碎片
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }


        public override void UpdateInventory(Player player)
        {
            // 当物品被持有时更新冷却
            if (empCooldown > 0)
            {
                empCooldown--;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 判断是否应该发射EMP弹丸
            if (empCooldown <= 0)
            {
                // 发射EMP弹丸
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<EMPBallProjectile>(), damage, knockback, player.whoAmI);
                // 重置冷却计时器
                empCooldown = EMP_COOLDOWN_TIME * Item.useTime;
            }
            else
            {
                // 发射标准电球弹丸
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SeitaadBallistaProjectile>(), damage, knockback, player.whoAmI);
            }
            
            return false; // 我们手动创建了弹丸，不需要默认行为
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
        }

        public float GetCurrentCharge()
        {
            return empCooldown;
        }

        public float GetMaxCharge()
        {
            return EMP_COOLDOWN_TIME * Item.useTime;
        }
    }
}