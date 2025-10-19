//using ExpansionKele.Common;
//using ExampleMod.Content.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content  
{  
    public class ExpansionKeleRecipe : ModSystem  
	{  
    public override void AddRecipeGroups() {  
    // 创建一级锭组（钴锭和钯金锭）  
    RecipeGroup primaryBarGroup = new RecipeGroup(  
        () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CobaltBar)} / {Lang.GetItemNameValue(ItemID.PalladiumBar)}",  
        ItemID.CobaltBar,  
        ItemID.PalladiumBar  
    );  
    RecipeGroup.RegisterGroup("ExpansionKele:PrimaryBars", primaryBarGroup);  

    // 创建二级锭组（秘银锭和山铜锭）  
    RecipeGroup secondaryBarGroup = new RecipeGroup(  
        () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MythrilBar)} / {Lang.GetItemNameValue(ItemID.OrichalcumBar)}",  
        ItemID.MythrilBar,  
        ItemID.OrichalcumBar // 确保这里是山铜锭的 ItemID  
    );  
    RecipeGroup.RegisterGroup("ExpansionKele:SecondaryBars", secondaryBarGroup);  

    // 创建三级锭组（精金锭和钛金锭）  
    RecipeGroup tertiaryBarGroup = new RecipeGroup(  
        () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.AdamantiteBar)} / {Lang.GetItemNameValue(ItemID.TitaniumBar)}",  
        ItemID.AdamantiteBar,  
        ItemID.TitaniumBar  
    );  
    RecipeGroup.RegisterGroup("ExpansionKele:TertiaryBars", tertiaryBarGroup);  
    
    // 定义一个配方组，用于任意金锭  
    RecipeGroup goldBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.GoldBar), new int[] { ItemID.GoldBar, ItemID.PlatinumBar });  
    RecipeGroup.RegisterGroup("ExpansionKele:BeforeSecondaryBars", goldBars);  

    // 定义一个配方组，用于任意银锭  
    RecipeGroup silverBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.SilverBar), new int[] { ItemID.SilverBar, ItemID.TungstenBar });  
    RecipeGroup.RegisterGroup("ExpansionKele:BeforeTertiaryBars", silverBars);  

    // 定义一个配方组，用于任意铜锭  
    RecipeGroup copperBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.CopperBar), new int[] { ItemID.CopperBar, ItemID.TinBar });  
    RecipeGroup.RegisterGroup("ExpansionKele:AnyCopperBars", copperBars);  

    // 定义一个配方组，用于任意铁锭  
    RecipeGroup ironBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.IronBar), new int[] { ItemID.IronBar, ItemID.LeadBar });  
    RecipeGroup.RegisterGroup("ExpansionKele:AnyIronBars", ironBars);  
    
    // 定义任意魔矿  
    RecipeGroup demoniteBars = new RecipeGroup(() =>   
    Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.DemoniteBar),   
    new int[] { ItemID.DemoniteBar, ItemID.CrimtaneBar }  
    );  
    RecipeGroup.RegisterGroup("ExpansionKele:AnyDemoniteBar", demoniteBars); // 使用正确的变量名  

    // 定义任何暗影鳞片  
    RecipeGroup shadowScale = new RecipeGroup(() =>   
    Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.ShadowScale),   
    new int[] { ItemID.ShadowScale, ItemID.TissueSample }  
    );  
    RecipeGroup.RegisterGroup("ExpansionKele:AnyShadowScale", shadowScale); // 使用正确的变量名  

     // 定义一个配方组，用于任意银锭  
    RecipeGroup EvilPowders = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.VilePowder), new int[] { ItemID.VilePowder, ItemID.ViciousPowder });  
    RecipeGroup.RegisterGroup("ExpansionKele:AnyEvilPowders", EvilPowders);  

    RecipeGroup soul = new RecipeGroup(() =>   
    Language.GetText("LegacyMisc.37") + " " + "魂(新三王)",   
    new int[] { ItemID.SoulofMight, ItemID.SoulofFright,ItemID.SoulofSight }  
    );  
    RecipeGroup.RegisterGroup("ExpansionKele:AnySoul", soul); // 使用正确的变量名  
    
    }




    }
}  
	
