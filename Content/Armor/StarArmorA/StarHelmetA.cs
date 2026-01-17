using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Threading;
using System.IO;

namespace ExpansionKele.Content.Armor.StarArmorA
{
    public class StarHelmetA : StarHelmetAbs
    {
        public override int Index => 0; // A 对应索引 0
        public static string setNameOverride = "星元头盔A";

        protected override int GetBodyType()
        {
            return ModContent.ItemType<StarBreastplateA>();
        }

        protected override int GetLegsType()
        {
            return ModContent.ItemType<StarLeggingsA>();
        }
		protected override int ItemType => ModContent.ItemType<StarHelmetA>();

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetA>());
            recipe.AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 12);
            recipe.AddRecipeGroup("ExpansionKele:BeforeTertiaryBars", 6);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}