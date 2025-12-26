using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Microsoft.Xna.Framework.Input;
using Terraria;
using System.ComponentModel;

namespace ExpansionKele
{
    public class ExpansionKeleConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        
        //[Label("激光常开")]
        //[Tooltip("如果启用，激光将始终显示。")]
        [DefaultValue(true)] // 设置默认值
        //用这的，别用加载的
        public bool LaserAlwaysOn;

        //[Label("启用灾厄沉沦海版本删除的部分合成表(如果装载了灾厄)")]
        //[Tooltip("主要添加了部分灾厄沉沦海版本前部分原版物品的合成配方")]
        [DefaultValue(true)]
        public bool EnableCalamityRecipes;


        public static ExpansionKeleConfig Instance => ModContent.GetInstance<ExpansionKeleConfig>();

        [DefaultValue(false)]
        public bool FullMoonOreGenerated { get; set; }
        
        // 添加一个统一控制原版物品修改的配置选项
        // [Label("启用原版物品修改")]
        // [Tooltip("如果启用，将修改一些原版物品的属性（如泰拉靴、徽章等）")]
        [DefaultValue(true)]
        public bool EnableVanillaItemOverrides { get; set; }
        // 新增属性：控制是否显示详细工具提示
        [DefaultValue(false)]
        public bool EnableDetailedTooltips { get; set; }
        
        [DefaultValue(1.0f)]
        [Range(0.1f, 2.0f)]
        [Increment(0.01f)]
        [Slider]
        public float SpecificWeaponDamageMultiplier { get; set; } = 1.0f;
        
        [DefaultValue(false)]
        public bool EnableGlobalDamageMultiplierModification { get; set; }
    }
}