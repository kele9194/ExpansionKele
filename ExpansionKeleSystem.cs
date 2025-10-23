using ExpansionKele.Content.Items.OtherItem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele
{
    public class WoodDemonInstrumentRecipe : ModSystem
    {
        
        public override void AddRecipes()
        {
            Recipe.Create(ItemID.WormScarf)
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.ShadowScale, 5)
                .AddIngredient(ItemID.DemoniteBar, 5)
                .AddTile(TileID.Anvils)
                .Register();

            Recipe.Create(ItemID.BrainOfConfusion)
                .AddIngredient(ModContent.ItemType<BrainOfMonster>(), 1)
                .AddIngredient(ItemID.CrimtaneBar, 6)
                .AddIngredient(ItemID.TissueSample, 6)
                .AddTile(TileID.Anvils)
                .Register();

            Recipe.Create(ItemID.Extractinator)
                .AddIngredient(ItemID.Wire, 50)
                .AddRecipeGroup("ExpansionKele:AnyIronBars", 10)
                .AddIngredient(ItemID.Lever, 3)
                .AddIngredient(ItemID.Switch, 3)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();

            // 狱石锭由陨石锭*1和黑曜石*1在熔炉合成
            Recipe.Create(ItemID.HellstoneBar)
                .AddIngredient(ItemID.MeteoriteBar, 1)
                .AddIngredient(ItemID.Obsidian, 1)
                .AddTile(TileID.Furnaces)
                .Register();

            // 蘑菇锤炼机由叶绿锭，灵气*2和蘑菇*15在秘银砧合成
            Recipe.Create(ItemID.Autohammer)
                .AddIngredient(ItemID.ChlorophyteBar, 1)
                .AddIngredient(ItemID.GlowingMushroom, 15)
                .AddIngredient(ItemID.SoulofLight, 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
             var config = ExpansionKeleConfig.Instance;

            if (ExpansionKele.calamity != null && config.EnableCalamityRecipes)
            {
            
            Recipe recipe = Recipe.Create(ItemID.ArmorPolish); 
            recipe.AddIngredient(ExpansionKele.calamity.Find<ModItem>("AncientBoneDust").Type, 3);      
            recipe.AddIngredient(ItemID.Bone, 10);      
            recipe.AddTile(TileID.Anvils);         
            recipe.Register();

            Recipe recipeI = Recipe.Create(ItemID.Bezoar); 
            recipeI.AddIngredient(ExpansionKele.calamity.Find<ModItem>("MurkyPaste").Type, 3);      
            recipeI.AddIngredient(ItemID.Stinger, 5);      
            recipeI.AddTile(TileID.Anvils);         
            recipeI.Register();

            Recipe.Create(ItemID.FrozenTurtleShell)
            .AddIngredient(ItemID.TurtleShell, 2)
            .AddIngredient(ExpansionKele.calamity.Find<ModItem>("EssenceofEleum").Type, 4)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.Vitamins)
            .AddIngredient(ItemID.BottledWater, 1)
            .AddIngredient(ItemID.Waterleaf, 3)
            .AddIngredient(ItemID.Blinkroot, 3) 
            .AddIngredient(ItemID.Daybloom, 3) 
            .AddIngredient(ExpansionKele.calamity.Find<ModItem>("BloodOrb").Type, 5)
            .AddTile(TileID.AlchemyTable)         
            .Register();

            Recipe recipe2 = Recipe.Create(ItemID.AdhesiveBandage); 
            recipe2.AddIngredient(ItemID.Silk, 10);
            recipe2.AddIngredient(ItemID.Gel, 10); 
            recipe2.AddIngredient(ItemID.HealingPotion, 1);   
            recipe2.AddTile(TileID.Anvils);         
            recipe2.Register();

            Recipe.Create(ItemID.AdhesiveBandage)
            .AddIngredient(ItemID.Silk, 10)
            .AddIngredient(ItemID.Gel, 10)
            .AddIngredient(ItemID.HealingPotion, 1) 
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.Nazar)
            .AddIngredient(ItemID.SoulofNight, 5)
            .AddIngredient(ItemID.Lens, 3)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.Megaphone)
            .AddIngredient(ItemID.Wire, 5)
            .AddRecipeGroup("ExpansionKele:PrimaryBars", 3)
            .AddIngredient(ItemID.Ruby, 1)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.PocketMirror)
            .AddIngredient(ItemID.Glass, 5)
            .AddIngredient(ItemID.GoldBar, 3)
            .AddIngredient(ItemID.CrystalShard, 2)
            .AddIngredient(ItemID.SoulofNight, 2)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.FastClock)
            .AddIngredient(ItemID.Timer1Second, 1)
            .AddIngredient(ItemID.PixieDust, 5)
            .AddIngredient(ItemID.SoulofLight, 5)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.TrifoldMap)
            .AddIngredient(ItemID.Silk, 10)
            .AddIngredient(ItemID.SoulofLight, 3)
            .AddIngredient(ItemID.SoulofNight, 3)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.CobaltShield)
            .AddRecipeGroup("ExpansionKele:PrimaryBars", 5)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.BandofRegeneration)
            .AddIngredient(ItemID.Shackle, 1)
            .AddIngredient(ItemID.LifeCrystal, 1)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.BlackLens)
            .AddIngredient(ItemID.Lens, 1)
            .AddIngredient(ItemID.BlackDye, 1)
            .AddTile(TileID.DyeVat)         
            .Register();

            Recipe.Create(ItemID.BugNet)
            .AddIngredient(ItemID.Cobweb, 30)
            .AddRecipeGroup("ExpansionKele:AnyCopperBars", 3)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.CelestialMagnet)
            .AddIngredient(ItemID.TreasureMagnet, 1)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddTile(TileID.Anvils)         
            .Register();
            
            Recipe.Create(ItemID.GuideVoodooDoll)
            .AddIngredient(ItemID.Leather, 2)
            .AddRecipeGroup("ExpansionKele:AnyEvilPowders", 10)
            .AddTile(TileID.Hellforge)         
            .Register();

            Recipe.Create(ItemID.IceMachine)
            .AddIngredient(ItemID.IceBlock, 10)
            .AddIngredient(ItemID.SnowBlock, 5)
            .AddRecipeGroup("ExpansionKele:AnyIronBars", 5)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.WoodenBoomerang)
            .AddIngredient(ItemID.Wood, 7)
            .AddTile(TileID.WorkBenches)         
            .Register();

            
            Recipe.Create(ItemID.IceBoomerang)
            .AddIngredient(ItemID.WoodenBoomerang, 1)
            .AddIngredient(ItemID.IceBlock, 20)
            .AddIngredient(ItemID.SnowBlock, 10)
            .AddIngredient(ItemID.Shiverthorn, 1)
            .AddTile(TileID.IceMachine)         
            .Register();

            Recipe.Create(ItemID.LavaFishingHook)
            .AddIngredient(ItemID.Seashell, 1)
            .AddIngredient(ItemID.HellstoneBar, 10)
            .AddTile(TileID.Hellforge)         
            .Register();

            Recipe.Create(ItemID.LeafWand)
            .AddIngredient(ItemID.Wood, 10)
            .AddTile(TileID.LivingLoom)         
            .Register();

            Recipe.Create(ItemID.LivingMahoganyWand)
            .AddIngredient(ItemID.RichMahogany, 10)
            .AddTile(TileID.LivingLoom)         
            .Register();

            Recipe.Create(ItemID.MagicQuiver)
            .AddIngredient(ItemID.EndlessQuiver, 1)
            .AddIngredient(ItemID.PixieDust, 5)
            .AddIngredient(ItemID.Lens, 3)
            .AddIngredient(ItemID.SoulofLight, 5)
            .AddTile(TileID.CrystalBall)         
            .Register();

            Recipe.Create(ItemID.MetalDetector)
            .AddIngredient(ItemID.Wire, 10)
            .AddIngredient(ItemID.SpelunkerGlowstick, 5)
            .AddRecipeGroup("ExpansionKele:AnyCopperBars", 5)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.SkyMill)
            .AddIngredient(ItemID.SunplateBlock, 10)
            .AddIngredient(ItemID.Cloud, 5)
            .AddIngredient(ItemID.RainCloud, 3)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.Shuriken,50)
            .AddRecipeGroup("ExpansionKele:AnyIronBars", 1)
            .AddTile(TileID.Anvils)         
            .Register();

            Recipe.Create(ItemID.StaffofRegrowth)
            .AddIngredient(ItemID.RichMahogany, 10)
            .AddIngredient(ItemID.JungleSpores, 5)
            .AddIngredient(ItemID.JungleRose, 1)
            .AddTile(TileID.WorkBenches)         
            .Register();

            Recipe.Create(ItemID.TempleKey)
            .AddIngredient(ItemID.JungleSpores, 15)
            .AddIngredient(ItemID.RichMahogany, 10)
            .AddIngredient(ItemID.SoulofLight, 5)
            .AddIngredient(ItemID.SoulofNight, 5)
            .AddTile(TileID.MythrilAnvil)         
            .Register();

            Recipe.Create(ItemID.ThrowingKnife,50)
            .AddRecipeGroup("ExpansionKele:AnyIronBars", 1)
            .AddTile(TileID.Anvils)         
            .Register();



            }
        }


    }
}