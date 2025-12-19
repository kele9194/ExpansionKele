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
	    // 添加静态字段存储RecipeGroup的ID
        private static int _primaryBarsID = -1;
        private static int _secondaryBarsID = -1;
        private static int _tertiaryBarsID = -1;
        private static int _beforeSecondaryBarsID = -1;
        private static int _beforeTertiaryBarsID = -1;
        private static int _anyCopperBarsID = -1;
        private static int _anyIronBarsID = -1;
        private static int _anyDemoniteBarID = -1;
        private static int _anyShadowScaleID = -1;
        private static int _anyEvilPowdersID = -1;
        private static int _anySoulID = -1;
        
        // 添加公共静态属性用于安全访问RecipeGroup ID
        public static int PrimaryBarsID => _primaryBarsID;
        public static int SecondaryBarsID => _secondaryBarsID;
        public static int TertiaryBarsID => _tertiaryBarsID;
        public static int BeforeSecondaryBarsID => _beforeSecondaryBarsID;
        public static int BeforeTertiaryBarsID => _beforeTertiaryBarsID;
        public static int AnyCopperBarsID => _anyCopperBarsID;
        public static int AnyIronBarsID => _anyIronBarsID;
        public static int AnyDemoniteBarID => _anyDemoniteBarID;
        public static int AnyShadowScaleID => _anyShadowScaleID;
        public static int AnyEvilPowdersID => _anyEvilPowdersID;
        public static int AnySoulID => _anySoulID;

        // 添加枚举来表示不同的RecipeGroup
        public enum KLGroupID
        {
            PrimaryBars,
            SecondaryBars,
            TertiaryBars,
            BeforeSecondaryBars,
            BeforeTertiaryBars,
            AnyCopperBars,
            AnyIronBars,
            AnyDemoniteBar,
            AnyShadowScale,
            AnyEvilPowders,
            AnySoul
        }
        
    public override void AddRecipeGroups() {  
    // 创建一级锭组（钴锭和钯金锭）  
    RecipeGroup primaryBarGroup = new RecipeGroup(  
        () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CobaltBar)} / {Lang.GetItemNameValue(ItemID.PalladiumBar)}",  
        ItemID.CobaltBar,  
        ItemID.PalladiumBar  
    );  
    _primaryBarsID = RecipeGroup.RegisterGroup("ExpansionKele:PrimaryBars", primaryBarGroup);  

    // 创建二级锭组（秘银锭和山铜锭）  
    RecipeGroup secondaryBarGroup = new RecipeGroup(  
        () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MythrilBar)} / {Lang.GetItemNameValue(ItemID.OrichalcumBar)}",  
        ItemID.MythrilBar,  
        ItemID.OrichalcumBar // 确保这里是山铜锭的 ItemID  
    );  
    _secondaryBarsID = RecipeGroup.RegisterGroup("ExpansionKele:SecondaryBars", secondaryBarGroup);  

    // 创建三级锭组（精金锭和钛金锭）  
    RecipeGroup tertiaryBarGroup = new RecipeGroup(  
        () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.AdamantiteBar)} / {Lang.GetItemNameValue(ItemID.TitaniumBar)}",  
        ItemID.AdamantiteBar,  
        ItemID.TitaniumBar  
    );  
    _tertiaryBarsID = RecipeGroup.RegisterGroup("ExpansionKele:TertiaryBars", tertiaryBarGroup);  
    
    // 定义一个配方组，用于任意金锭  
    RecipeGroup goldBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.GoldBar), new int[] { ItemID.GoldBar, ItemID.PlatinumBar });  
    _beforeSecondaryBarsID = RecipeGroup.RegisterGroup("ExpansionKele:BeforeSecondaryBars", goldBars);  

    // 定义一个配方组，用于任意银锭  
    RecipeGroup silverBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.SilverBar), new int[] { ItemID.SilverBar, ItemID.TungstenBar });  
    _beforeTertiaryBarsID = RecipeGroup.RegisterGroup("ExpansionKele:BeforeTertiaryBars", silverBars);  

    // 定义一个配方组，用于任意铜锭  
    RecipeGroup copperBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.CopperBar), new int[] { ItemID.CopperBar, ItemID.TinBar });  
    _anyCopperBarsID = RecipeGroup.RegisterGroup("ExpansionKele:AnyCopperBars", copperBars);  

    // 定义一个配方组，用于任意铁锭  
    RecipeGroup ironBars = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.IronBar), new int[] { ItemID.IronBar, ItemID.LeadBar });  
    _anyIronBarsID = RecipeGroup.RegisterGroup("ExpansionKele:AnyIronBars", ironBars);  
    
    // 定义任意魔矿  
    RecipeGroup demoniteBars = new RecipeGroup(() =>   
    Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.DemoniteBar),   
    new int[] { ItemID.DemoniteBar, ItemID.CrimtaneBar }  
    );  
    _anyDemoniteBarID = RecipeGroup.RegisterGroup("ExpansionKele:AnyDemoniteBar", demoniteBars); // 使用正确的变量名  

    // 定义任何暗影鳞片  
    RecipeGroup shadowScale = new RecipeGroup(() =>   
    Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.ShadowScale),   
    new int[] { ItemID.ShadowScale, ItemID.TissueSample }  
    );  
    _anyShadowScaleID = RecipeGroup.RegisterGroup("ExpansionKele:AnyShadowScale", shadowScale); // 使用正确的变量名  

     // 定义一个配方组，用于任意邪恶粉末
    RecipeGroup EvilPowders = new RecipeGroup(() => Language.GetText("LegacyMisc.37") + " " + Lang.GetItemName(ItemID.VilePowder), new int[] { ItemID.VilePowder, ItemID.ViciousPowder });  
    _anyEvilPowdersID = RecipeGroup.RegisterGroup("ExpansionKele:AnyEvilPowders", EvilPowders);  

    RecipeGroup soul = new RecipeGroup(() =>   
    Language.GetText("LegacyMisc.37") + " " + "魂(新三王)",   
    new int[] { ItemID.SoulofMight, ItemID.SoulofFright,ItemID.SoulofSight }  
    );  
    _anySoulID = RecipeGroup.RegisterGroup("ExpansionKele:AnySoul", soul); // 使用正确的变量名  
    
    }

        // 添加静态方法用于获取RecipeGroup ID，提供智能提示和类型安全
        /// <summary>
        /// 获取一级锭组（钴锭和钯金锭）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetPrimaryBarsID() => _primaryBarsID;

        /// <summary>
        /// 获取二级锭组（秘银锭和山铜锭）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetSecondaryBarsID() => _secondaryBarsID;

        /// <summary>
        /// 获取三级锭组（精金锭和钛金锭）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetTertiaryBarsID() => _tertiaryBarsID;

        /// <summary>
        /// 获取金锭组（金锭和铂金锭）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetBeforeSecondaryBarsID() => _beforeSecondaryBarsID;

        /// <summary>
        /// 获取银锭组（银锭和钨锭）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetBeforeTertiaryBarsID() => _beforeTertiaryBarsID;

        /// <summary>
        /// 获取铜锭组（铜锭和锡锭）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetAnyCopperBarsID() => _anyCopperBarsID;

        /// <summary>
        /// 获取铁锭组（铁锭和铅锭）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetAnyIronBarsID() => _anyIronBarsID;

        /// <summary>
        /// 获取恶魔矿组（恶魔矿和猩红矿）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetAnyDemoniteBarID() => _anyDemoniteBarID;

        /// <summary>
        /// 获取暗影鳞片组（暗影鳞片和组织样本）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetAnyShadowScaleID() => _anyShadowScaleID;

        /// <summary>
        /// 获取邪恶粉末组（邪气粉和恶毒粉）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetAnyEvilPowdersID() => _anyEvilPowdersID;

        /// <summary>
        /// 获取新三王魂组（力量之魂、恐惧之魂、视域之魂）的ID
        /// </summary>
        /// <returns>RecipeGroup ID</returns>
        public static int GetAnySoulID() => _anySoulID;
        
        /// <summary>
        /// 根据枚举获取对应的RecipeGroup ID
        /// </summary>
        /// <param name="groupId">RecipeGroup枚举</param>
        /// <returns>RecipeGroup ID</returns>
        public static int GetRecipeGroupID(KLGroupID groupId)
        {
            switch (groupId)
            {
                case KLGroupID.PrimaryBars: return _primaryBarsID;
                case KLGroupID.SecondaryBars: return _secondaryBarsID;
                case KLGroupID.TertiaryBars: return _tertiaryBarsID;
                case KLGroupID.BeforeSecondaryBars: return _beforeSecondaryBarsID;
                case KLGroupID.BeforeTertiaryBars: return _beforeTertiaryBarsID;
                case KLGroupID.AnyCopperBars: return _anyCopperBarsID;
                case KLGroupID.AnyIronBars: return _anyIronBarsID;
                case KLGroupID.AnyDemoniteBar: return _anyDemoniteBarID;
                case KLGroupID.AnyShadowScale: return _anyShadowScaleID;
                case KLGroupID.AnyEvilPowders: return _anyEvilPowdersID;
                case KLGroupID.AnySoul: return _anySoulID;
                default: return -1;
            }
        }
        
        /// <summary>
        /// 获取RecipeGroup的名称字符串
        /// </summary>
        /// <param name="groupId">RecipeGroup枚举</param>
        /// <returns>RecipeGroup名称字符串</returns>
        public static string GetRecipeGroupName(KLGroupID groupId)
        {
            switch (groupId)
            {
                case KLGroupID.PrimaryBars: return "ExpansionKele:PrimaryBars";
                case KLGroupID.SecondaryBars: return "ExpansionKele:SecondaryBars";
                case KLGroupID.TertiaryBars: return "ExpansionKele:TertiaryBars";
                case KLGroupID.BeforeSecondaryBars: return "ExpansionKele:BeforeSecondaryBars";
                case KLGroupID.BeforeTertiaryBars: return "ExpansionKele:BeforeTertiaryBars";
                case KLGroupID.AnyCopperBars: return "ExpansionKele:AnyCopperBars";
                case KLGroupID.AnyIronBars: return "ExpansionKele:AnyIronBars";
                case KLGroupID.AnyDemoniteBar: return "ExpansionKele:AnyDemoniteBar";
                case KLGroupID.AnyShadowScale: return "ExpansionKele:AnyShadowScale";
                case KLGroupID.AnyEvilPowders: return "ExpansionKele:AnyEvilPowders";
                case KLGroupID.AnySoul: return "ExpansionKele:AnySoul";
                default: return "";
            }
        }
        
        /// <summary>
        /// 检查指定物品是否属于一级锭组（钴锭或钯金锭）
        /// </summary>
        /// <param name="itemType">物品类型ID</param>
        /// <returns>如果物品属于该组则返回true</returns>
        public static bool IsPrimaryBar(int itemType) => IsValidItemForGroup(itemType, _primaryBarsID);

        /// <summary>
        /// 检查指定物品是否属于二级锭组（秘银锭或山铜锭）
        /// </summary>
        /// <param name="itemType">物品类型ID</param>
        /// <returns>如果物品属于该组则返回true</returns>
        public static bool IsSecondaryBar(int itemType) => IsValidItemForGroup(itemType, _secondaryBarsID);
        
        /// <summary>
        /// 检查指定物品是否属于三级锭组（精金锭或钛金锭）
        /// </summary>
        /// <param name="itemType">物品类型ID</param>
        /// <returns>如果物品属于该组则返回true</returns>
        public static bool IsTertiaryBar(int itemType) => IsValidItemForGroup(itemType, _tertiaryBarsID);
        
        /// <summary>
        /// 检查指定物品是否属于铁锭组（铁锭或铅锭）
        /// </summary>
        /// <param name="itemType">物品类型ID</param>
        /// <returns>如果物品属于该组则返回true</returns>
        public static bool IsAnyIronBar(int itemType) => IsValidItemForGroup(itemType, _anyIronBarsID);
        
        /// <summary>
        /// 检查指定物品是否属于铜锭组（铜锭或锡锭）
        /// </summary>
        /// <param name="itemType">物品类型ID</param>
        /// <returns>如果物品属于该组则返回true</returns>
        public static bool IsAnyCopperBar(int itemType) => IsValidItemForGroup(itemType, _anyCopperBarsID);
        
        /// <summary>
        /// 内部辅助方法，检查物品是否属于指定的RecipeGroup
        /// </summary>
        /// <param name="itemType">物品类型ID</param>
        /// <param name="groupId">RecipeGroup ID</param>
        /// <returns>如果物品属于该组则返回true</returns>
        private static bool IsValidItemForGroup(int itemType, int groupId)
        {
            if (groupId < 0 || groupId >= RecipeGroup.recipeGroups.Count) 
                return false;
                
            RecipeGroup group = RecipeGroup.recipeGroups[groupId];
            return group != null && group.ValidItems.Contains(itemType);
        }
    }
}  
	