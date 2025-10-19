using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;
using Terraria.DataStructures;
using System;





namespace ExpansionKele.Content.StaryMelee
{
    public class StarySwordD : ModItem
    {
        public override string LocalizationCategory => "StaryMelee";
         private const int LeftClickDamage = 23;
        private const float LeftClickKnockBack = 8f;
        private const float LeftClickShootSpeed = 10f;
        private const int LeftClickUseTime = 20;
        private const int LeftClickUseAnimation = LeftClickUseTime;

        private const int constcrit = 5;
        private const int constrare = 3;
        private const string setNameOverride="星元剑D";

        private const string introduction ="星元剑C的升级版，近战可造成两次伤害，第二次基于目标血量造成额外伤害，远程射弹可造成爆炸,爆炸基于指数造成伤害,";

        

        //private int cooldownTicks = 7;
        private int BoostDuration;

        private const int BoostTime=20;

        private int defenseBoost=10;

        private int lifeRegenBoost=6*20;

        private float speedBoost=0.1f;

        private float enduranceBoost=0.07f;


        // public override Color? GetAlpha(Color lightColor) {
		// 	return Color.Red;
		// }

        public override void SetStaticDefaults()
	{
		//ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
	}

        public override void SetDefaults()
        {
        //Item.SetNameOverride(setNameOverride);
        Item.width = 80;
		Item.height = 80;
		Item.damage = 19;
        if(ExpansionKele.calamity!=null){
            Item.damage=(int)(Item.damage*1.25);
        }
		Item.DamageType = DamageClass.Melee;
		Item.useAnimation = 20;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 20;
		Item.useTurn = false;//自动转向
		Item.knockBack = 8f;
		Item.UseSound = SoundID.Item1;
		Item.autoReuse = true;
        Item.value = Item.sellPrice(silver:(int)(Item.damage*0.3f));
        Item.rare = ItemRarityID.Orange; // 稀有度
        Item.shoot = ModContent.ProjectileType<ColaProjectile>(); // 射弹类型
        Item.shootSpeed =  10f; // 射弹速度
        Item.crit = constcrit;
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
        public override void AddRecipes()
        {
            // 创建 GaSniperA 武器的合成配方  
            Recipe recipe = Recipe.Create(ModContent.ItemType<StarySwordD>()); // 替换为 GaSniperD 的类型  
            recipe.AddIngredient(ModContent.ItemType<StarySwordC>(), 1); ////group用group,Ingredient用Ingredient
            recipe.AddIngredient(ItemID.SoulofNight, 7); // 暗影之魂*7
            recipe.AddIngredient(ItemID.SoulofLight, 7); // 光明之魂*7
            recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
            recipe.Register(); // 注册配方  
        }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
            BoostDuration = BoostTime;
            if(target.lifeMax<=8000)
            {
                BoostDuration=(int)(BoostTime/5);
            }
            player.statDefense += defenseBoost;

            // 增加玩家的移动速度15%
            player.moveSpeed += speedBoost;

            player.endurance += enduranceBoost;

            // 增加玩家的回血速率 +10Hp/s
            player.lifeRegen += lifeRegenBoost;

            
            target.AddBuff(ModContent.BuffType<ArmorPodwered>(), 82);
            target.AddBuff(BuffID.OnFire, 82);
            int lifePercentageDamage = (int)(target.lifeMax * 0.006); // 例如，10%生命值
            damageDone+=lifePercentageDamage;
            Vector2 position = player.Center;
    Vector2 velocity = player.DirectionTo(target.Center).SafeNormalize(Vector2.UnitX) * 10f; // 调整速度和方向
    Terraria.Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity, ModContent.ProjectileType<LifePercentageProjectile>(), damageDone, 0f, player.whoAmI, target.whoAmI);
            
        }
    }
}



