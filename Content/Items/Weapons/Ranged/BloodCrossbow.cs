using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using System;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class BloodCrossbow : BaseCrossbow
    {
        protected override int GetBaseDamage()
        {
            return ExpansionKele.ATKTool(30, 36);
        }

        protected override float GetKnockback()
        {
            return 3f;
        }

        public override void HoldItem(Player player)
        {
            player.lifeRegenTime += 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 6)
                .AddIngredient(ItemID.TissueSample, 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}