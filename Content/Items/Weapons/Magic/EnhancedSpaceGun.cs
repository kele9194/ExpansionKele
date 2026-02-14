using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MagicProj;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
	public class EnhancedSpaceGun : ModItem
	{
		public override string LocalizationCategory => "Items.Weapons.Magic";


		public override void SetDefaults()
		{
			// 使用CloneDefaults复制太空枪的所有基础属性
			Item.CloneDefaults(ItemID.SpaceGun);

			// 修改特定属性以适应我们的强化版本
			Item.damage = ExpansionKele.ATKTool(46,57); // 设置伤害为45点（原版太空枪是30点）
			Item.DamageType = DamageClass.Magic; // 确保是法师职业伤害
			
			// 修改弹幕类型为我们自定义的强化激光弹幕
			Item.shoot = ModContent.ProjectileType<EnhancedLaserProjectile>();
			
			// 可以适当调整其他属性
			Item.shootSpeed *= 1.8f; // 提高射速
			Item.mana = 6; // 降低魔法消耗
			Item.rare = ItemUtils.CalculateRarityFromRecipes(this); // 提升稀有度
			Item.value = ItemUtils.CalculateValueFromRecipes(this); // 提高价值
		}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0f, 0f);
            return false;
        }



        public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.SpaceGun)
				.AddIngredient(ItemID.CrystalShard, 10)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}