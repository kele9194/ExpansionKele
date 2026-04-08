using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Projectiles.SummonProj;
using ExpansionKele.Content.Customs;
using System;
using ExpansionKele.Content.Items.Weapons.Ranged;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Weapons.Summon
{
    public class RemoteGatling : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Summon";


        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(20, 25);
            Item.DamageType = DamageClass.Summon;
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.sentry = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<RemoteGatlingSentry>();
            Item.mana = 0;
            Item.autoReuse = true;


        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.FindSentryRestingSpot(type, out int XPosition, out int YPosition, out int YOffset);
            YOffset += -4;
            position = new Vector2((float)XPosition, (float)(YPosition - YOffset));
            int p = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            player.UpdateMaxTurrets();
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Gatling>(), 1)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}