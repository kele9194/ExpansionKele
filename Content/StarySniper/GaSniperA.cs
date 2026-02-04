using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria.Audio;
using ExpansionKele.Content.Customs;


namespace ExpansionKele.Content.StarySniper
{
	
	public class GaSniperA : GaSniperAbs, IChargeableItem
	{
        public override string LocalizationCategory => "StarySniper";
		
		public override int BaseDamage => 65;
		public override int Width => 64;
		public override int Height => 25;
		public override int UseTime => 97;
		public override Vector2 HoldoutOffsetValue => new Vector2(-12f, -2f);
		public override string introduction => "一把高伤害低射速的反器材狙击步枪，使用火枪子弹，钨/银子弹时，会转化为高速子弹。";

		public override void SetDefaults() {
			base.SetDefaults();
		}
        public override bool AltFunctionUse(Player player)
        {
            return false;
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = false;
            
        }

        // 请看 Content/ExampleRecipes.cs 了解配方创建的详细解释
        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperA>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 7); // 添加任意金锭组，要求7个  
    recipe.AddRecipeGroup("ExpansionKele:BeforeTertiaryBars", 7); // 添加任意银锭组，要求7个  
	
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  
	
	  // 此方法可以调整武器在玩家手中的位置。调整这些值直到与图形效果匹配。  
        public override Vector2? HoldoutOffset() {  
            return HoldoutOffsetValue; // 持有偏移量。  
        }  
		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {  
            base.ModifyTooltips(tooltips);
            // 添加自定义的 tooltip  
            // TooltipLine line = new TooltipLine(Mod, "GaSniperATooltip", "一把高伤害低射速的反器材狙击步枪，使用火枪子弹，钨/银子弹时，会转化为高速子弹。");  
            // tooltips.Add(line);  
			// tooltips.Add(new TooltipLine(Mod, "FocusAbility", "专注机制：武器在手中不使用时会积累专注值，增加伤害，满级时有提示音效"));
            
            // // 添加辅助瞄准说明
            // tooltips.Add(new TooltipLine(Mod, "LaserAbility", "拥有镭射激光辅助瞄准（可在模组设置中开关）"));
        }  

        // 修改发射统计数据的方法。  
        //public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {  
            // 每个从这把枪发射的弹丸都有1/3的概率成为自定义弹丸。  
       //     if (Main.rand.NextBool(3)) {  
         //       type = ModContent.ProjectileType<ExampleInstancedProjectile>(); // 替换为你的自定义弹丸类型。  
        //    }  
       // } 
	   public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
	}
}