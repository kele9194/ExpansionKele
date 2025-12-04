using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class SpectralCurtainCannon : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        private const int constDamage = 85;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("幽幕加农炮");
            Item.damage = ExpansionKele.ATKTool(constDamage,default);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 32;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SpectralCurtainCannonProj>();
            Item.shootSpeed = 30f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            
        }
        

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<SpectralCurtainCannon>());
            recipe.AddIngredient(ModContent.ItemType<IronCurtainCannon>());
            recipe.AddIngredient(ItemID.Ectoplasm, 3);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }

    
    }