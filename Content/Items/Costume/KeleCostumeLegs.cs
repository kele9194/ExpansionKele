using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;


namespace ExpansionKele.Content.Items.Costume
{
    [AutoloadEquip(EquipType.Legs)]
    public class KeleCostumeLegs : ModItem
    {
        public override string LocalizationCategory => "Items.Costume";
        public override void SetStaticDefaults()
        {
            ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = false;
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("可乐的测试时装腿部");
            Item.width = 18;
            Item.height = 18;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 2);
            Item.vanity = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<KeleCostumeLegs>());
            //recipe.AddTile(TileID.WorkBenches); // 设置制作台为工作台（或其他你希望的制作台）
            recipe.Register();
        }
    }
}