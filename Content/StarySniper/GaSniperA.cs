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
	
	public class GaSniperA : ModItem
	{
        public override string LocalizationCategory => "StarySniper";
		public override void SetDefaults() {
			//Item.SetNameOverride("SG星元狙击步枪-A");
			Item.width = 64; 
			Item.height = 25; 

			Item.autoReuse = true;
			Item.damage = Item.damage = ExpansionKele.ATKTool(65,default);
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 6f; 
			Item.noMelee = true; 
			Item.shootSpeed = 16f; 
			Item.useAnimation = 97; 
			Item.useTime = 97;
			Item.UseSound = ExpansionKele.SniperSound; 
			Item.useStyle = ItemUseStyleID.Shoot; 
			Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 

			Item.shoot = ProjectileID.Bullet;
			Item.useAmmo = AmmoID.Bullet;

			
			// Custom ammo and shooting homing projectiles
			////Item.shoot = ModContent.ProjectileType<Projectiles.ExampleHomingProjectile>();
			////Item.useAmmo = ModContent.ItemType<ExampleCustomAmmo>(); // Restrict the type of ammo the weapon can use, so that the weapon cannot use other ammos
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
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
            return new Vector2(-12f, -2f); // 持有偏移量。  
        }  
		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {  
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
			focustime=0;
			return true;
		}
	   private float focustime;
        private float focusbonus;
	   public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {  
            // 检查当前使用的弹药类型  
            // 如果弹药是火枪子弹、钨子弹或银子弹，转换为高速子弹  
            if (type == ProjectileID.Bullet ) {  
                type = ProjectileID.BulletHighVelocity; // 转换为高速子弹  
            }  
            if (player.velocity == Vector2.Zero)
            {
                damage=(int)(damage*1.2*(1+focusbonus));
            }
            else 
            {
                damage=(int)(damage*(1+focusbonus));
            }
	   }
       public override void UpdateInventory(Player player)
        {
            
            if(focustime<300&&player.HeldItem.type==Item.type){
                focustime++;
            }
            focusbonus=Math.Min(focustime/Item.useAnimation-1,2);
			if (focustime == 3 * Item.useAnimation||focustime ==299)
            {
                // 播放Item75声音效果
                SoundEngine.PlaySound(SoundID.Item75, player.position);
            }
            
            base.UpdateInventory(player);
            // 检查玩家是否站在不动
            
        }
	}
}
