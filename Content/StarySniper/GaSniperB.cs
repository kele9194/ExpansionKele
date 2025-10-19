using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria.Audio;

namespace ExpansionKele.Content.StarySniper
{
	// This is an example showing how to create a weapon that fires custom ammunition
	// The most important property is "Item.useAmmo". It tells you which item to use as ammo.
	// You can see the description of other parameters in the ExampleGun class and at https://github.com/tModLoader/tModLoader/wiki/Item-Class-Documentation
	public class GaSniperB : ModItem
	{
		public override string LocalizationCategory => "StarySniper";
		public override void SetDefaults() {
			//Item.SetNameOverride("SG星元狙击步枪-B");
			Item.width = 72; // The width of item hitbox
			Item.height = 28; // The height of item hitbox

			Item.autoReuse = true;  // Whether or not you can hold click to automatically use it again.
			Item.damage = 77; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
			if(ExpansionKele.calamity != null)
			{
				Item.damage = (int)(Item.damage*1.25);
			}
			Item.DamageType = DamageClass.Ranged; // What type of damage does this item affect?
			Item.knockBack = 6f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
			Item.noMelee = true; // So the item's animation doesn't do damage.
			Item.rare =ItemRarityID.Blue; //2// The color that the item's name will be in-game.
			Item.shootSpeed = 16f; // The speed of the projectile (measured in pixels per frame.)
			Item.useAnimation = 97; // The length of the item's use animation in ticks (60 ticks == 1 second.)
			Item.useTime = 97; // The item's use time in ticks (60 ticks == 1 second.)
			Item.UseSound = ExpansionKele.SniperSound;  // The sound that this item plays when used.
			Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, shoot, etc.)
			Item.value = Item.buyPrice(silver:(int)(Item.damage/Item.useTime*10f));  // The value of the weapon in copper coins
			Item.crit=5;

			Item.shoot = ProjectileID.Bullet;
			//Item.shoot = ModContent.ProjectileType<Projectiles.YourBulletProjectile>(); // 替换为你的弹丸类型  
            //Item.useAmmo = ModContent.ItemType<YourCustomAmmo>(); // 替换为你自定义的弹药类型  
			Item.useAmmo = AmmoID.Bullet;

			
			// Custom ammo and shooting homing projectiles
			////Item.shoot = ModContent.ProjectileType<Projectiles.ExampleHomingProjectile>();
			////Item.useAmmo = ModContent.ItemType<ExampleCustomAmmo>(); // Restrict the type of ammo the weapon can use, so that the weapon cannot use other ammos
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperB>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:AnyDemoniteBar", 7); // 添加任意魔锭组，要求7个
	recipe.AddRecipeGroup("ExpansionKele:AnyShadowScale", 7);//添加任意暗影文化组，要求7个
	recipe.AddIngredient(ModContent.ItemType<GaSniperA>(), 1);
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  
	
	  // 此方法可以调整武器在玩家手中的位置。调整这些值直到与图形效果匹配。  
        public override Vector2? HoldoutOffset() {  
            return new Vector2(-14f, -2f); // 持有偏移量。  
        }  
		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {  
            // // 添加自定义的 tooltip  
            // TooltipLine line = new TooltipLine(Mod, "GaSniperBTooltip", "该狙击步枪A型号升级版,使用火枪子弹,钨/银子弹时,会转化为高速子弹。");  
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
