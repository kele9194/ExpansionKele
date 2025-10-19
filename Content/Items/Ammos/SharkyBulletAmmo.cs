using ExpansionKele.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Ammos
{
    public class SharkyBulletAmmo : ModItem
    {
        public override string LocalizationCategory => "Items.Ammos";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 9999; // 设置弹药的伤害值
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true; // 标记为消耗品
            Item.knockBack = 1.5f;
            Item.value = 100; // 设置弹药的价值
            Item.rare = ItemRarityID.Purple; // 设置弹药的稀有度
            Item.shoot = ModContent.ProjectileType<SharkyBullet>(); // 设置弹道类型为 SharkyBullet
            Item.shootSpeed = 5f; // 设置弹道的速度
            Item.ammo = AmmoID.Bullet; // 设置弹药类型为子弹
        }
    }
}
