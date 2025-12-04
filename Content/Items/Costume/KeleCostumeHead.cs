using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;


namespace ExpansionKele.Content.Items.Costume
{
    [AutoloadEquip(EquipType.Head)]
    public class KeleCostumeHead : ModItem
    {
        public override string LocalizationCategory => "Items.Costume";
        public override void SetStaticDefaults()
        {
            //Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(10, 4));//不需要
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("可乐的测试时装头部");
            Item.width = 18;
            Item.height = 18;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.vanity = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<KeleCostumeHead>());
            //recipe.AddTile(TileID.WorkBenches); // 设置制作台为工作台（或其他你希望的制作台）
            recipe.Register();
        }
        public class KeleCostumePlayer : ModPlayer
    {
        public bool BlockyAccessoryPrevious;
        public bool BlockyAccessory;
        public bool BlockyHideVanity;
        public bool BlockyForceVanity;
        public bool BlockyPower;
        public bool BlockyVanityEffects => BlockyForceVanity || (BlockyPower && !BlockyHideVanity);

        public override void ResetEffects()
        {
            BlockyAccessoryPrevious = BlockyAccessory;
            BlockyAccessory = BlockyHideVanity = BlockyForceVanity = BlockyPower = false;
        }

        public override void FrameEffects()
        {
            if (BlockyVanityEffects)
            {
                Player.head = EquipLoader.GetEquipSlot(Mod, nameof(KeleCostumeHead), EquipType.Head);
                Player.body = EquipLoader.GetEquipSlot(Mod, nameof(KeleCostumeBody), EquipType.Body);
                Player.legs = EquipLoader.GetEquipSlot(Mod, nameof(KeleCostumeLegs), EquipType.Legs);
            }
        }
    }
    }
}