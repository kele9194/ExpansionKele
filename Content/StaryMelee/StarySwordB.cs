using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;



namespace ExpansionKele.Content.StaryMelee
{
    public class StarySwordB : ModItem
    {
        public override string Texture => this.GetRelativeTexturePath("./StarySwordSmall");
        public override string LocalizationCategory => "StaryMelee";
        private const int constcrit = 2;
        private const string setNameOverride="星元剑B";
        private const string introduction ="星元剑A的升级版，右键可发射普通的可乐射弹";

        public override void SetStaticDefaults()
	{
		ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
	}

        public override void SetDefaults()
        {
            //Item.SetNameOverride(setNameOverride);
        Item.width = 60;
		Item.height = 60;
		Item.damage = Item.damage = ExpansionKele.ATKTool(default,22);
		Item.DamageType = DamageClass.Melee;
		Item.useAnimation = 20;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 20;
		Item.useTurn = true;//自动转向
		Item.knockBack = 5f;
		Item.UseSound = SoundID.Item1;
		Item.autoReuse = true;
        Item.value = ItemUtils.CalculateValueFromRecipes(this);
        Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
        Item.shoot = ModContent.ProjectileType<ColaProjectileLower>(); // 射弹类型
        Item.shootSpeed =  10f; // 射弹速度
        Item.crit= constcrit;
        }

         public override Vector2? HoldoutOffset() {  
             return new Vector2(0, 0); // 持有偏移量。  
         }  

        
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
            Terraria.Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false; // 返回 false 以防止默认行为
    }

    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
    // 临时移除敌人的免疫状态
    target.immune[player.whoAmI] = 0;
}
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // 添加自定义的 tooltip  
            // TooltipLine line = new TooltipLine(Mod, setNameOverride, introduction);
            // tooltips.Add(line);
        }


    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(ModContent.BuffType<ArmorPodweredLower>(), 82);
            target.AddBuff(BuffID.OnFire, 82);
            
        }

        public override void AddRecipes()  
	{  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarySwordB>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:AnyDemoniteBar", 6); // 添加任意魔锭组，要求7个
	recipe.AddRecipeGroup("ExpansionKele:AnyShadowScale", 8);//添加任意暗影文化组，要求7个
    recipe.AddIngredient(ModContent.ItemType<StarySwordA>(), 1);
	
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  
    }
}
