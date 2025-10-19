using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using ExpansionKele.Content.Buff;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Weapons
{
    public class FrostRayGun : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetDefaults()
        {
            //Item.SetNameOverride("冷冻射线枪");
            Item.damage = ExpansionKele.ATKTool(180,default);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 38;
            Item.height = 24;
            Item.useTime = Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FrostRayProjectile>();
            Item.shootSpeed = 30f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // tooltips.Add(new TooltipLine(Mod, "Introduction", Language.GetText("Mods.ExpansionKele.Items.FrostRayGun.Introduction").Value));
        }

        public override void AddRecipes()
        {
            Recipe.Create(ModContent.ItemType<FrostRayGun>())
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ItemID.LunarBar, 1)
                .AddIngredient(ItemID.IceBlock, 50)
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    
}