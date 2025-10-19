using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles;
using Terraria.DataStructures;
using Mono.Cecil;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Weapons
{
    public class AAMissileLauncher : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public static LocalizedText BaseTooltip { get; private set; }

        public override void SetStaticDefaults() {
			BaseTooltip = this.GetLocalization("Tooltip");
		}
        public override void SetDefaults()
        {
            //Item.SetNameOverride("防空导弹发射器");
            Item.damage = ExpansionKele.ATKTool(default,180);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AAMissile>();
            Item.shootSpeed = 30f;
        }


        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // 这里可以添加其他武器伤害修改逻辑
        }

//         public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
// {
//     if (target.velocity.Y != 0)
//     {
//         hit.Damage = (int)(hit.Damage * 1.4f); // 额外40%伤害
//         Main.NewText($"Missile Launcher Damage: {hit.Damage}");
//     }
// }
        
         public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 创建导弹并设置初始速度
            Terraria.Projectile projectile = Terraria.Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return false; // 返回 false 以防止默认的发射行为
        }
         
         public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }

        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<AAMissileLauncher>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.MeteoriteBar, 5); // 添加任意金锭组，要求7个 
	recipe.AddIngredient(ItemID.ShroomiteBar, 5);
    recipe.AddTile(TileID.MythrilAnvil);
    recipe.Register(); // 注册配方  
	}  


    }
    /*ModifyHitNPC：在计算伤害之前调用，适合用于修改实际的伤害值和其他属性。
OnHitNPC：在伤害已经计算并应用之后调用，适合用于添加额外的效果或逻辑。*/

}