using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace ExpansionKele.Content.Items.Weapons
{
	public class FullMoonSpear : ModItem
	{
        public override bool MeleePrefix() => true;
		public override string LocalizationCategory => "Items.Weapons";

		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true; // 跳过使用动画开始时的声音播放
			ItemID.Sets.Spears[Item.type] = true; // 让游戏识别为长矛类型
		}

		public override void SetDefaults() {
			// 常规属性
			Item.rare = ItemRarityID.Pink; // 粉红色稀有度
			Item.value = Item.sellPrice(gold: 2); // 售价

			// 使用属性
			Item.useStyle = ItemUseStyleID.Shoot; // 使用风格为射击
			Item.useAnimation = 35; // 使用动画持续时间
			Item.useTime = 35; // 使用间隔时间
			Item.UseSound = SoundID.Item71; // 使用音效
			Item.autoReuse = true; // 自动重复使用

			// 武器属性
			Item.damage = ExpansionKele.ATKTool(72, 92); // 伤害
			Item.knockBack = 6.5f; // 击退
			Item.noUseGraphic = true; // 使用时不显示物品图形
			Item.DamageType = DamageClass.Melee; // 伤害类型为近战
			Item.noMelee = true; // 不使用物品本身的近战判定

			// 弹幕属性
			Item.shootSpeed = 4f; // 弹幕速度
			Item.shoot = ModContent.ProjectileType<FullMoonSpearProjectile>(); // 发射的弹幕类型
		}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            Projectile.NewProjectile(source, position, velocity*5f, ModContent.ProjectileType<FullMoonSpearHeadProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override bool CanUseItem(Player player) {
			// 确保最多只能有一个长矛存在
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override bool? UseItem(Player player) {
			// 由于跳过了使用动画开始时的声音播放，我们需要在实际使用时播放声音
			if (!Main.dedServ && Item.UseSound.HasValue) {
				SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
			}

			return null;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Placeables.FullMoonBar>(), 12)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}