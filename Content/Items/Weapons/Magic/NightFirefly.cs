using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MagicProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
	public class NightFirefly : ModItem,IChargeableItem
	{
		public override string LocalizationCategory => "Items.Weapons";

		public override void SetStaticDefaults()
		{
			// 设置法杖的持有位置偏移
			Item.staff[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = ExpansionKele.ATKTool(250,300); // 伤害250
			Item.DamageType = DamageClass.Magic; // 法师伤害
			Item.width = 80;
			Item.height = 80;
			Item.useTime = 6; // 使用时间6帧
			Item.useAnimation = 6;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true; // 不接触伤害
			Item.knockBack = 5f;
			Item.value = ItemUtils.CalculateValueFromRecipes(this);// 卖价10金币
			Item.rare = ItemUtils.CalculateRarityFromRecipes(this);  // 黄色稀有度
			Item.UseSound = SoundID.Item8; // 魔法射击音效
			Item.autoReuse = true; // 自动连发
			Item.shoot = ModContent.ProjectileType<NightFireflyProjectile>(); // 发射NightFireflyProjectile
			Item.shootSpeed = 12f; // 射速12
			Item.mana = 7; // 消耗15魔法值
		}
        public override bool AltFunctionUse(Player player)
		{
			// 允许右键使用，仅当生命值低于1/3时
			return player.statLife <= player.statLifeMax / 3;
		}

		public override bool CanUseItem(Player player)
		{
			// 右键使用检查
			if (player.altFunctionUse == 2)
			{
				return player.statLife <= player.statLifeMax / 3;
			}
			return true;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			// 右键使用触发萤火状态
			if (player.altFunctionUse == 2)
			{
				player.AddBuff(ModContent.BuffType<Content.Buff.NightFireflyBuff>(), 600);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// 发射自定义弹幕
			int proj=Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NightFireflyProjectile>(), damage, knockback, player.whoAmI);
			Main.projectile[proj].originalDamage = damage;
            return false; // 阻止默认弹幕生成
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-2f, 0f); // 持握偏移
		}

		public override void AddRecipes()
		{

		}
		public float GetCurrentCharge()
		{
			Player player = Main.LocalPlayer;
			if (player.HasBuff(ModContent.BuffType<Content.Buff.NightFireflyBuff>()))
			{
				int buffIndex = -1;
				for (int i = 0; i < player.buffType.Length; i++)
				{
					if (player.buffType[i] == ModContent.BuffType<Content.Buff.NightFireflyBuff>() && player.buffTime[i] > 0)
					{
						buffIndex = i;
						break;
					}
				}

				if (buffIndex >= 0)
				{
					return player.buffTime[buffIndex]; // 返回buff剩余时间
				}
			}
			return 0f;
		}

		public float GetMaxCharge()
		{
			// NightFireflyBuff的持续时间为600帧
			return 600f;
		}
	}

    public class NightFireflyPlayer : ModPlayer
    {
        // 萤火状态计时器
        public int nightFireflyTimer = 0;
        public bool inNightFireflyState = false;

        public override void ResetEffects()
        {
            // 重置萤火状态
            inNightFireflyState = false;
        }

                // ... existing code ...
                    public override void PreUpdateBuffs()
        {
            // 检查玩家是否处于萤火状态
            if (Player.HasBuff(ModContent.BuffType<Content.Buff.NightFireflyBuff>()))
            {
                // 如果刚刚进入萤火状态，则恢复最大生命值
                if (nightFireflyTimer == 0)
                {
                    Player.Heal(Player.statLifeMax2);
                }
                
                inNightFireflyState = true;
                nightFireflyTimer++;
                CustomDamageReductionPlayer customDamageReductionPlayer = Player.GetModPlayer<CustomDamageReductionPlayer>();
                customDamageReductionPlayer.MultiPreDefenseDamageReduction(0.5f);

                // 设置玩家为无敌状态
                // Player.immune = true;
                // Player.immuneTime = 2;
                

                // 根据计时器逐渐减少生命值
                if (nightFireflyTimer % 20 == 0) // 每20帧减少一次生命值
                {
                    int previousLife = Player.statLife;
                    int targetLife = Player.statLifeMax2 - (int)((Player.statLifeMax2 - 1) * (nightFireflyTimer / 600f));
                    if (Player.statLife > targetLife)
                    {
                        Player.statLife = targetLife;
                        if (Player.statLife < 1) Player.statLife = 1;
                        
                        // 显示生命值减少的效果
                        int lifeLost = previousLife - Player.statLife;
                        if (lifeLost > 0)
                        {
                            CombatText.NewText(Player.getRect(), Color.Red, lifeLost, false, true);
                        }
                    }
                }
            }
            else
            {
                // 重置计时器
                nightFireflyTimer = 0;
            }
        }
// ... existing code ...
    }
}