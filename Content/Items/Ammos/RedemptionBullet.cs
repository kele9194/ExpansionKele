using ExpansionKele.Content.Items.OtherItem;
using ExpansionKele.Content.Projectiles.RangedProj;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Ammos
{
    public class RedemptionBullet : ModItem
    {
        public override string LocalizationCategory => "Items.Ammos";
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 10; // 救赎弹伤害为10点
            Item.DamageType = DamageClass.Ranged; // 远程伤害
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true; // 标记为消耗品
            Item.knockBack = 1.0f;
            Item.value = Item.sellPrice(0, 0, 0, 5); // 设置弹药的价值
            Item.rare = ItemRarityID.Blue; // 设置弹药的稀有度
            Item.shoot = ModContent.ProjectileType<RedemptionBulletProjectile>(); // 设置弹道类型为救赎弹抛射体
            Item.shootSpeed = 15f; // 设置弹道的速度
            Item.ammo = AmmoID.Bullet; // 设置弹药类型为子弹
        }
        
        public override void AddRecipes()
        {
            CreateRecipe(200) // 用5个救赎碎片制作200发救赎弹
                .AddIngredient<RedemptionShard>(5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
}