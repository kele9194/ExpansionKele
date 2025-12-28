using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExpansionKele.Content.Items.Tiles
{
    public class PsionicOreTile : ModTile
    {
        public override string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 810; // 幽能矿的优先级，参考月亮矿石
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1975;
            Main.tileMergeDirt[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(150, 0, 255), name); // 紫色
            
            HitSound = SoundID.Tink;
            DustType = DustID.PurpleTorch;
            MinPick = 200; // 需要至少200%的镐力才能挖掘，参考月亮矿石
            RegisterItemDrop(ModContent.ItemType<Items.Placeables.PsionicOre>());
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
        
        public override bool IsTileDangerous(int i, int j, Player player)
        {
            // 幽能矿通常不会造成伤害，但可以根据需要添加效果
            return false;
        }
    }
}