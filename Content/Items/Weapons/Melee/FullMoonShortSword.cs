using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 望月短剑 - 近战武器
    /// 发射追踪弹幕，命中敌人后失去穿透性并继续前进一小段距离，随后返回玩家
    /// </summary>
    public class FullMoonShortsword : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override bool MeleePrefix() => true;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            // Item.SetNameOverride("望月短剑");
            Item.damage = ExpansionKele.ATKTool(40, 48);
            Item.DamageType = DamageClass.Melee;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.knockBack = 4f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FullMoonShortSwordProjectile>();
            Item.shootSpeed = 3.6f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 发射短剑弹幕
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // 同时发射追踪月亮弹幕
            Vector2 targetPosition = Main.MouseWorld;
            Vector2 direction = (targetPosition - position).SafeNormalize(Vector2.UnitX);
            Vector2 shootVelocity = direction * Item.shootSpeed * 6f; // 稍慢一点的速度
            
            Projectile.NewProjectile(source, position, shootVelocity, ModContent.ProjectileType<FullMoonShortSwordMoonProj>(), damage, knockback, player.whoAmI);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FullMoonBar>(), 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}