using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MagicProj;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
	public class AnaxaMagicTrick : ModItem
	{
		public override string LocalizationCategory => "Items.Weapons.Magic";

		public override void SetStaticDefaults()
		{
			
		}

		public override void SetDefaults()
		{
			Item.damage = ExpansionKele.ATKTool(46, 58); // 伤害180-220
			Item.DamageType = DamageClass.Magic; // 法师伤害
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 20; // 使用时间20帧
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true; // 不接触伤害
			Item.knockBack = 4f;
			Item.value = ItemUtils.CalculateValueFromRecipes(this);
			Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
			Item.UseSound = SoundID.Item11; // 魔法射击音效
			Item.autoReuse = true; // 自动连发
			Item.shoot = ModContent.ProjectileType<AnaxaMagicTrickProjectile>(); // 发射AnaxaMagicTrickProjectile
			Item.shootSpeed = 10f; // 射速10
			Item.mana = 8; // 消耗10魔法值
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// 发射自定义弹幕
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false; // 阻止默认弹幕生成
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-20f, -2f); // 持握偏移
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			GlowmaskHelper.DrawItemGlowmaskInWorld(Item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
		}
		
		// 绘制玩家持握时的发光遮罩 - 简化版本
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			GlowmaskHelper.DrawItemGlowmaskInInventory(Item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			return true; // 允许继续绘制原始纹理
		}

		public override void AddRecipes()
		{
			// 可以在这里添加合成配方
            CreateRecipe()
				.AddIngredient(ItemID.SpectreBar, 5) // 幽灵锭*5
				.AddIngredient(ItemID.ChlorophyteBar, 5) // 叶绿锭*5
				.AddIngredient(ItemID.GoldBar, 5) // 金锭*5
				.AddIngredient(ItemID.Shotgun, 1) // 散弹枪
				.AddTile(TileID.MythrilAnvil) // 在秘银砧合成
				.Register();
		}
	}
}