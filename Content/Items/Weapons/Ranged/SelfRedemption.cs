using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Weapons.Magic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Items.OtherItem;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
	public class SelfRedemption : ModItem
	{
        public override string LocalizationCategory => "Items.Weapons";
        private const int MaxAmmoCount = 40;
		private int ammoCount = MaxAmmoCount;
        private const int reloadTime = 100;
        private const int AmmoTime = 8;
		
		public override void SetStaticDefaults()
		{
			// 允许右键重复使用
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
			ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
		}



		public override void SetDefaults() {
			// 基本属性设置
			Item.damage = ExpansionKele.ATKTool(189,480); // 伤害75
			Item.DamageType = DamageClass.Ranged; // 远程伤害
			Item.width = 32;
			Item.height = 32;
			Item.useTime = AmmoTime; // 使用时间6帧
			Item.useAnimation = AmmoTime;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true; // 不接触伤害
			Item.knockBack = 2f;
			Item.value = ItemUtils.CalculateValueFromRecipes(this); // 卖价10金币
			Item.rare = ItemUtils.CalculateRarityFromRecipes(this);  // 黄色稀有度
			Item.UseSound = SoundID.Item11; // 枪声
			Item.autoReuse = true; // 自动连发
			
			// 弹药相关设置
			Item.shoot = ModContent.ProjectileType<SelfRedemptionProjectile>(); // 发射FadeProjectile
			Item.shootSpeed = 16f; // 射速16
			Item.useAmmo = AmmoID.None; // 不消耗弹药
			
		}
        
		public override bool AltFunctionUse(Player player)
		{
			// 允许右键使用
			return true;
		}
		
        public override bool CanUseItem(Player player)
		{
			// 右键装填
			if (player.altFunctionUse == 2)
			{
				Item.useTime = reloadTime;
				Item.useAnimation = reloadTime;
				Item.noUseGraphic = true;
                Item.UseSound = null; // 禁用使用音效
				// 只要弹药不是满的就可以装填
				return ammoCount < MaxAmmoCount;
			}
			else{
				Item.useTime = AmmoTime;
				Item.useAnimation = AmmoTime;
				Item.noUseGraphic = false;
                Item.UseSound = SoundID.Item11; // 恢复使用音效
				if (ammoCount == 0)
				{
					CombatText.NewText(player.getRect(), Color.Cyan, "NoAmmo", true);
					return false;
				}
				return ammoCount > 0;
			}
		}
		
		public override void HoldItem(Player player) {
			// 应用负面效果
			var selfRedemptionPlayer = player.GetModPlayer<SelfRedemptionPlayer>();
			selfRedemptionPlayer.isHoldingSelfRedemption = true;
		}

		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// 右键装填
			if (player.altFunctionUse == 2)
			{
				// 装填弹药
				ammoCount = MaxAmmoCount;
    
				CombatText.NewText(player.getRect(), Color.Cyan, $"{ammoCount}/{MaxAmmoCount}", true);
				Terraria.Audio.SoundEngine.PlaySound(ExpansionKele.FadeReloadSound, player.position);
				return false;
			}
			
			// 发射自定义弹幕
			Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<FadeProjectile>(), damage, knockback, player.whoAmI);
			
			// 减少弹药数量
			ammoCount--;
			if(ammoCount%5==0){
				CombatText.NewText(player.getRect(), Color.Cyan, $"{ammoCount}/{MaxAmmoCount}", true);
                }
			
			return false; // 阻止默认弹幕生成
		}
		
		public override Vector2? HoldoutOffset() {
			return new Vector2(-2f, 0f); // 持握偏移
		}
		
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Fade>()
				.AddIngredient<Resentment>()
				.AddIngredient<StarryBar>(3)
				.AddIngredient<RedemptionShard>(3996)
				.AddTile(TileID.LunarCraftingStation) // 远古操纵机
				.Register();
		}
		
	}

	public class SelfRedemptionPlayer : ModPlayer
	{
		public bool isHoldingSelfRedemption = false;
		
		public override void ResetEffects()
		{
			isHoldingSelfRedemption = false;
		}
	}

	public class SelfRedemptionHitEffectPlayer : ModPlayer
    {
        // 受击后效果相关变量
        public bool hitEffectActive = false;
        public int hitEffectTimer = 0;
        public const int HitEffectDuration = 360; // 持续360帧 (6秒)

        public override void ResetEffects()
        {
            // 每帧重置效果
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			base.ModifyHurt(ref modifiers);

			// 只有当玩家手持SelfRedemption武器时才激活效果
			var selfRedemptionPlayer = Player.GetModPlayer<SelfRedemptionPlayer>();
			if (selfRedemptionPlayer.isHoldingSelfRedemption && !hitEffectActive)
			{
				hitEffectActive = true;
				hitEffectTimer = HitEffectDuration;
			}

			// 如果受击效果激活，则增加30%防御前减伤
			if (hitEffectActive && selfRedemptionPlayer.isHoldingSelfRedemption)
			{
				var defensePlayer = Player.GetModPlayer<CustomDamageReductionPlayer>();
				defensePlayer.preDefenseDamageReductionMulti -= 0.3f; // 增加30%防御前减伤
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, Player.position);
			}
		}

        public override void PreUpdate()
        {
            // 更新计时器
            if (hitEffectActive)
            {
                hitEffectTimer--;
                if (hitEffectTimer <= 0)
                {
                    hitEffectActive = false;
                }
            }
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // 只有当玩家手持SelfRedemption且受击效果激活时，才增加20%武器伤害
            var selfRedemptionPlayer = Player.GetModPlayer<SelfRedemptionPlayer>();
            if (hitEffectActive && selfRedemptionPlayer.isHoldingSelfRedemption)
            {
                var damagePlayer = Player.GetModPlayer<ExpansionKeleDamageMulti>();
                damagePlayer.MultiplyMultiplicativeDamageBonus(1.2f); // 增加20%武器伤害
            }
        }

        public override void UpdateLifeRegen()
        {
            // 只有当玩家手持Fade武器且受击效果激活时，才减少生命恢复时间
            var fadePlayer = Player.GetModPlayer<FadePlayer>();
            if (hitEffectActive && fadePlayer.isHoldingFade && Main.rand.NextDouble() <= 0.5)
            {
                Player.lifeRegenTime += 2;
            }
        }
    }
}