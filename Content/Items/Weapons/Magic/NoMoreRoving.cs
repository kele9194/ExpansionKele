using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    public class NoMoreRoving : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("不再漫游");
            // Tooltip.SetDefault("抛出一个魔法地雷，地雷减速后部署并对敌人造成AOE伤害");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.damage = ExpansionKele.ATKTool(60,80);
            Item.knockBack = 3f;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 0;
            Item.shoot = ModContent.ProjectileType<NoMoreRovingProjectile>();
            Item.shootSpeed = 8f;
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj=Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            Main.projectile[proj].originalDamage=damage;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book,1)
                .AddIngredient(ItemID.Amethyst,3)
                .AddRecipeGroup("ExpansionKele:AnyDemoniteBar",8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

}