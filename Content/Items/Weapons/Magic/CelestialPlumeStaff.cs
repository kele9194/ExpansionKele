using System;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Projectiles.MagicProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    public class CelestialPlumeStaff : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Magic";
        public int rotation = 0;
        public const int ANGLE_DIVISION = 12;

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(100, 115);
            Item.DamageType = DamageClass.Magic;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FeatherPlusProjectile>();
            Item.shootSpeed = 10f;
            Item.mana = 10;
            Item.crit =5;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 mousePosition = Main.MouseWorld;
            float radius = 160;
            float angleInRadians = rotation * 2f * MathHelper.Pi / ANGLE_DIVISION;

            Vector2 shootDirection = new Vector2((float)Math.Cos(angleInRadians), (float)Math.Sin(angleInRadians));
            Vector2 verticalDirection = new Vector2(-shootDirection.Y, shootDirection.X);
            
            Vector2 shootVelocity = -shootDirection * Item.shootSpeed;
            for(int i = 0; i < 3; i++){
                Vector2 spawnPosition = mousePosition + shootDirection * radius+verticalDirection*(-10+10*i);
                Projectile.NewProjectile(source, spawnPosition, shootVelocity, type, damage, knockback, player.whoAmI);
            }
            
            
            
            Vector2 oppositeDirection = -shootDirection;
            Vector2 oppositeSpawnPosition = mousePosition + oppositeDirection * radius;
            Vector2 oppositeShootVelocity = -oppositeDirection * Item.shootSpeed;
            
            Projectile.NewProjectile(source, oppositeSpawnPosition, oppositeShootVelocity, ModContent.ProjectileType<GiantFeatherProjectile>(), 2*damage, knockback, player.whoAmI);
            
            rotation += 1;
            if (rotation == ANGLE_DIVISION)
            {
                rotation = 0;
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FeatherStaff>(), 1)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddIngredient(ItemID.GiantHarpyFeather, 1)             
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}