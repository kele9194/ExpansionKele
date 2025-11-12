using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExpansionKele.Content.Items.Tiles
{
    public class ChromiumOreTile : ModTile
    {
        public override string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 390;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 975;
            Main.tileMergeDirt[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(124, 252, 0), name); 
            
            HitSound = SoundID.Tink;
            DustType = DustID.Stone;
            MinPick = 75; // 需要至少75%的镐力才能挖掘
            RegisterItemDrop(ModContent.ItemType<Items.Placeables.ChromiumOre>());
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            if (fail)
                num = 1;
            else
                num = 3;
        }
    }
}