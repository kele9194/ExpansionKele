using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using System;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class WoodenCrossbow : BaseCrossbow
    {
        protected override int GetBaseDamage()
        {
            return ExpansionKele.ATKTool(8, default);
        }

        protected override float GetKnockback()
        {
            return 2f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 12)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}