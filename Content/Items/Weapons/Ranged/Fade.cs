using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
	public class Fade : ModItem
	{
        public override string LocalizationCategory => "Items.Weapons";
        private const int MaxAmmoCount = 30;
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
			Item.damage = ExpansionKele.ATKTool(130,160); // 伤害45
			Item.DamageType = DamageClass.Ranged; // 远程伤害
			Item.width = 32;
			Item.height = 32;
			Item.useTime = AmmoTime; // 使用时间6帧
			Item.useAnimation = AmmoTime;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true; // 不接触伤害
			Item.knockBack = 2f;
			Item.value = ItemUtils.CalculateValueFromRecipes(this); // 卖价2银50铜
			Item.rare = ItemUtils.CalculateRarityFromRecipes(this);  // 蓝色稀有度
			Item.UseSound = SoundID.Item11; // 枪声
			Item.autoReuse = true; // 自动连发
			
			// 弹药相关设置
			Item.shoot = ModContent.ProjectileType<FadeProjectile>(); // 发射FadeProjectile
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

			var fadePlayer = player.GetModPlayer<FadePlayer>();
			fadePlayer.isHoldingFade = true;
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
				.AddIngredient(ItemID.IllegalGunParts, 1)
				.AddIngredient(ItemID.ChlorophyteBar, 12)
				.AddIngredient(ItemID.HallowedBar, 5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class FadePlayer : ModPlayer
	{
		public bool isHoldingFade = false;
		public override void ResetEffects()
		{
			isHoldingFade = false;
		}
	}

	public class HitEffectPlayer : ModPlayer
    {
        // 受击后效果相关变量
        public bool hitEffectActive = false;
        public int hitEffectTimer = 0;
        public const int HitEffectDuration = 240; // 持续240帧 (4秒)

        public override void ResetEffects()
        {
            // 每帧重置效果
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			base.ModifyHurt(ref modifiers);

			// 只有当玩家手持Fade武器时才激活效果
			var fadePlayer = Player.GetModPlayer<FadePlayer>();
			if (fadePlayer.isHoldingFade && !hitEffectActive)
			{
				hitEffectActive = true;
				hitEffectTimer = HitEffectDuration;
			}

			// 如果受击效果激活，则减少20%防御前减伤
			if (hitEffectActive && fadePlayer.isHoldingFade)
			{
				modifiers.IncomingDamageMultiplier*=1.2f; // 减少20%防御前减伤
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
            // 只有当玩家手持Fade武器且受击效果激活时，才降低20%武器伤害
            var fadePlayer = Player.GetModPlayer<FadePlayer>();
            if (hitEffectActive && fadePlayer.isHoldingFade)
            {
                damage*=0.8f; // 降低20%武器伤害
            }
        }

        public override void UpdateLifeRegen()
        {
            // 只有当玩家手持Fade武器且受击效果激活时，才减少生命恢复时间
            var fadePlayer = Player.GetModPlayer<FadePlayer>();
            if (hitEffectActive && fadePlayer.isHoldingFade && Main.rand.NextDouble() <= 0.5)
            {
                Player.lifeRegenTime -= 1;
            }
        }
    }
}