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
    public class StarySwordA : ModItem
    {
        public override string Texture => this.GetRelativeTexturePath("./StarySwordSmall");
        //如果不删除原本的贴图有限使用原来的
        public override string LocalizationCategory => "StaryMelee";
        private const string setNameOverride="星元剑A";
        private const string introduction ="星元剑系列第一把武器,左键近战挥击可造成破甲I和着火减益";
        public override void SetStaticDefaults()
	{
		ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
	}

        public override void SetDefaults()
        {
        //Item.SetNameOverride(setNameOverride);
        Item.width = 60;
		Item.height = 60;
		Item.damage = 14;
        if(ExpansionKele.calamity!=null){
            Item.damage=(int)(Item.damage*1.25);
        }
		Item.DamageType = DamageClass.Melee;
		Item.useAnimation = 20 ;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 20;
		Item.useTurn = true;//自动转向
		Item.knockBack = 5f;
		Item.UseSound = SoundID.Item1;
		Item.autoReuse = true;
        Item.value = Item.sellPrice(silver:(int)(Item.damage*0.3f));
        Item.rare = ItemRarityID.White; // 稀有度
        Item.shoot = ModContent.ProjectileType<ColaProjectileLower>();
        Item.shootSpeed =  10f; // 射弹速度
        }

         public override Vector2? HoldoutOffset() {  
             return new Vector2(0, 0); // 持有偏移量。  
         }  

        
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false; // 返回 false 以防止默认行为
    }
    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
    target.immune[player.whoAmI] = 0;
}
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // TooltipLine line = new TooltipLine(Mod, setNameOverride, introduction);
            // tooltips.Add(line);
        }
    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(ModContent.BuffType<ArmorPodweredLower>(), 82);
            target.AddBuff(BuffID.OnFire, 82);
            
        }

        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarySwordA>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 8); // 添加任意金锭组，要求7个  
    recipe.AddRecipeGroup("ExpansionKele:BeforeTertiaryBars", 6); // 添加任意银锭组，要求7个  
	
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  
    }
}



