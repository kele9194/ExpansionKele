using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Localization;
using ExpansionKele.Content.Items.Weapons.Ranged;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.RangedProj;
using ExpansionKele.Content.Items.Placeables;
using System;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class LightGap : ModItem,IChargeableItem
    {
        public static int MaxAmmoDir=30;//直射子弹
        public static int MaxAmmoSca=60;//散射子弹数
        public static int MaxNum=60;//前面两者公约数
        public int baseProgress = 0;
        public int progress = 0;
        public float Maxangle=15f;
        public override string LocalizationCategory => "Items.Weapons.Ranged";
        public static LocalizedText BaseTooltip { get; private set; }

        public override void SetStaticDefaults() {
			BaseTooltip = this.GetLocalization("Tooltip");
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
		}
        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(13,16);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 36;
            Item.height = 24;
            Item.useTime = 7;
            Item.useAnimation = 7;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LightGapProjectile>();
            Item.shootSpeed = 30f;
        }
        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15f, 4f); // 持握偏移
		}



        
         public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 BaseOffset1 = new Vector2(21f,-4f+4f);
            Vector2 BaseOffset2 = new Vector2(21f,4f+4f);//偏移
            Vector2 NewPos1 = PvUtils.PositionSet(position,BaseOffset1,velocity);
            Vector2 NewPos2 = PvUtils.PositionSet(position,BaseOffset2,velocity);//计算子弹出点

            baseProgress=baseProgress%(2*MaxNum);//设置轮转值
            progress=MaxNum-(int)MathF.Abs(baseProgress-MaxNum);//设置来回
            
            if(baseProgress<MaxNum){
                baseProgress+=MaxNum/MaxAmmoDir;
            Projectile projectile = Projectile.NewProjectileDirect(source, NewPos1, velocity, type, damage, knockback, player.whoAmI);
            Projectile projectile2 = Projectile.NewProjectileDirect(source, NewPos2, velocity, type, damage, knockback, player.whoAmI);
            }
            else{
                baseProgress+=MaxNum/MaxAmmoSca;
                float ammoScaProgress=1-MathF.Abs(2f*progress/MaxNum-1);//设置散射来回
            Projectile projectile = Projectile.NewProjectileDirect(source, NewPos1, velocity, type, damage, knockback, player.whoAmI);
            Projectile projectile2 = Projectile.NewProjectileDirect(source, NewPos2, velocity, type, damage, knockback, player.whoAmI);
            Projectile projectile3 = Projectile.NewProjectileDirect(source, NewPos1, velocity.RotatedBy(MathHelper.ToRadians(Maxangle*ammoScaProgress)), type, damage, knockback, player.whoAmI);
            Projectile projectile4 = Projectile.NewProjectileDirect(source, NewPos2, velocity.RotatedBy(-MathHelper.ToRadians(Maxangle*ammoScaProgress)), type, damage, knockback, player.whoAmI);
            
            }
            
            

            
            return false; // 返回 false 以防止默认的发射行为
        }
         
         public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }

        public override void AddRecipes()  
	{  
    
        // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(this.Type); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ModContent.ItemType<ChromiumBar>(), 5); // 添加任意金锭组，要求7个 
	recipe.AddIngredient(ItemID.MeteoriteBar, 5);
    recipe.AddTile(TileID.Anvils);
    recipe.Register(); // 注册配方  
	}

        public float GetCurrentCharge()
        {
            return progress;
        }

        public float GetMaxCharge()
        {
            return MaxNum;
        }
    }
    /*ModifyHitNPC：在计算伤害之前调用，适合用于修改实际的伤害值和其他属性。
OnHitNPC：在伤害已经计算并应用之后调用，适合用于添加额外的效果或逻辑。*/
}

