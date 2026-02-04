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
	// This is an example showing how to create a weapon that fires custom ammunition
	// The most important property is "Item.useAmmo". It tells you which item to use as ammo.
	// You can see the description of other parameters in the ExampleGun class and at https://github.com/tModLoader/tModLoader/wiki/Item-Class-Documentation
	public class GaSniperB : GaSniperAbs, IChargeableItem
	{
		public override string LocalizationCategory => "StarySniper";
		
		public override int BaseDamage => 77;
		public override int Width => 72;
		public override int Height => 28;
		public override int UseTime => 97;
		public override Vector2 HoldoutOffsetValue => new Vector2(-14f, -2f);
		public override string introduction => "该狙击步枪A型号升级版,使用火枪子弹,钨/银子弹时,会转化为高速子弹。";

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

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes()  
	{  
    // 创建 GaSniperB 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperB>()); // 替换为 GaSniperB 的类型  
    recipe.AddRecipeGroup("ExpansionKele:AnyDemoniteBar", 7); // 添加任意魔锭组，要求7个
	recipe.AddRecipeGroup("ExpansionKele:AnyShadowScale", 7);//添加任意暗影文化组，要求7个
	recipe.AddIngredient(ModContent.ItemType<GaSniperA>(), 1);
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  
	
	  // 此方法可以调整武器在玩家手中的位置。调整这些值直到与图形效果匹配。  
        public override Vector2? HoldoutOffset() {  
            return HoldoutOffsetValue; // 持有偏移量。  
        }  
		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {  
            base.ModifyTooltips(tooltips);
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