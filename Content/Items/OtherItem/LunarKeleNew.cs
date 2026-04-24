using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Bosses.BossKeleNew;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class LunarKeleNew : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override string Texture => this.GetRelativeTexturePath("./LunarKele");
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.maxStack = 1;
            Item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<BossKeleNew>());
        }

        public override bool? UseItem(Player player)
        {
            BossSpawnUtils.ItemSpawnBoss(player, ModContent.NPCType<BossKeleNew>());
            return true;
        }
        
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<LunarKeleNew>());
            recipe.AddIngredient(ItemID.LunarBar, 2);
            recipe.AddIngredient(ItemID.Ambrosia, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}