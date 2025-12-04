using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.Tiles;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Placeables
{
    public class FullMoonOre : ModItem
    {
        public override string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 60; // 排序优先级高于 ExampleBar。
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月矿");
            Item.DefaultToPlaceableTile(ModContent.TileType<FullMoonOreTile>());
            Item.width = 16;
            Item.height = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
        }

        public override void AddRecipes()
        {
            Recipe recipe;
            // 检查灾厄模组是否被加载
            if (ModLoader.HasMod("CalamityMod"))
            {
                // 检查是否存在 Shadow of Revenge NPC 或 Boss
            bool hasShadowOfRevenge = ModContent.TryFind<ModNPC>("ExpansionKele", "Shadowofrevenge", out _);
            
            // 如果存在 Shadow of Revenge，则不添加合成表
            if (hasShadowOfRevenge)
                return;
            if (ModLoader.HasMod("CalamityMod"))
            {
                // 灾厄存在时：1秘银矿 + 1暗影之魂 = 1望月矿
                recipe = CreateRecipe(4);
                recipe.AddRecipeGroup("ExpansionKele:SecondaryBars", 1);
                recipe.AddIngredient(ItemID.SoulofNight, 1);
                recipe.AddTile(TileID.MythrilAnvil);
            }
            else
            {
                // 灾厄不存在时：1神圣锭 + 4暗影之魂 = 4望月矿
                recipe = CreateRecipe(4);
                recipe.AddIngredient(ItemID.HallowedBar, 1);
                recipe.AddIngredient(ItemID.SoulofNight, 1);
                recipe.AddTile(TileID.AdamantiteForge);
            }  
            recipe.Register();
        }
    }
}}