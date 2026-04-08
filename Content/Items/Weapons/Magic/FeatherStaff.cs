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
    public class FeatherStaff : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Magic";
        public int rotation = 0;
        public const int ANGLE_DIVISION =12;

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(33, 40);
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FeatherProjectile>();
            Item.shootSpeed = 12f;
            Item.mana = 6;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 mousePosition = Main.MouseWorld;           
            // 半径200像素
            float radius = 128;
            float angleInRadians = rotation * 2f*MathHelper.Pi / ANGLE_DIVISION;

            Vector2 shootDirection = new Vector2((float)Math.Cos(angleInRadians), (float)Math.Sin(angleInRadians));
            
                
                // 计算发射位置：在鼠标周围半径200格的圆上
                Vector2 spawnPosition = mousePosition + shootDirection * radius;
                
                Vector2 shootVelocity = -shootDirection * Item.shootSpeed;
                
                
                // 发射羽毛弹幕
                Projectile.NewProjectile(source, spawnPosition, shootVelocity, type, damage, knockback, player.whoAmI);
            rotation += 1;
            if(rotation==12){
                rotation = 0;
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Feather, 10)
                .AddIngredient(ItemID.Cloud, 50)
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}