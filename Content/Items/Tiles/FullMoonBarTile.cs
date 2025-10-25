using ExpansionKele.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExpansionKele.Content.Items.Tiles
{
    public class FullMoonBarTile : ModTile
    {
        public override string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Main.tileShine[Type] = 1100;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(200, 200, 200), Language.GetText("FullMoonBar")); // localized text for "Metal Bar"

            // 注册该瓦片破坏后掉落的物品
            RegisterItemDrop(ModContent.ItemType<FullMoonBar>());
        }
    }
}