using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Bosses.ShadowOfRevenge;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class ShadowSilk : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.maxStack = 1;
            Item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            // 确保复仇之影未被召唤，并且只能在夜晚或地下使用
            return !NPC.AnyNPCs(ModContent.NPCType<ShadowOfRevenge>()) && 
                   (Main.dayTime == false);
        }

        public override bool? UseItem(Player player)
        {

                // 召唤复仇之影
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<ShadowOfRevenge>());
            return true;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.SoulofNight, 5) // 暗影鳞片作为暗影之魂的替代
                .AddTile(TileID.WorkBenches)
                .Register();
                
        }
    }
}