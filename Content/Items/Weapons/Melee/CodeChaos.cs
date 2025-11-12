using System;
using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele;
using Terraria.Localization;
using static Terraria.NPC;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class CodeChaos : ModItem
    {
        // 当前伤害类型 0=近战, 1=远程, 2=魔法, 3=召唤
        const int meleeDamageType = 3;
        const int rangedDamageType = (meleeDamageType + 1)%4;
        const int magicDamageType = (rangedDamageType + 1)%4;
        const int summonDamageType = (magicDamageType + 1)%4;
        private int currentDamageType = meleeDamageType;
        
        // 浮动参数
        private float damageMultiplier = 1f;
        private float critDamageMultiplier = 1.5f;
        private float critChanceMultiplier = 1f;
        private float useTimeMultiplier = 1f;
        
        public override string LocalizationCategory => "Items.Weapons";

        // 添加一个确保数组正确初始化且元素有效的方法
        // ... existing code ...
private int GetValidProjectileType(int index)
        {
            // 确保数组已初始化
            if (ExpansionKele.projectileTypes == null || index < 0 || index >= ExpansionKele.projectileTypes.Length)
            {
                // 返回默认有效的Projectile类型
                return ProjectileID.FireArrow;
            }

            // 检查数组中的Projectile类型是否有效
            if (ExpansionKele.projectileTypes[index] > 0)
            {
                // 再次检查该Projectile类型是否在游戏中注册
                if (ContentSamples.ProjectilesByType.ContainsKey(ExpansionKele.projectileTypes[index]))
                {
                    return ExpansionKele.projectileTypes[index];
                }
            }

            // 如果无效，返回默认Projectile类型
            return ProjectileID.FireArrow;
        }

        private int GetRandomValidProjectileType()
	{
		if (ExpansionKele.projectileTypes == null || ExpansionKele.projectileTypes.Length == 0)
		{
			return 2;
		}
		for (int i = 0; i < 10; i++)
		{
			int randomIndex = Main.rand.Next(ExpansionKele.projectileTypes.Length);
			if (ExpansionKele.projectileTypes[randomIndex] > 0 && ContentSamples.ProjectilesByType.ContainsKey(ExpansionKele.projectileTypes[randomIndex]))
			{
				return ExpansionKele.projectileTypes[randomIndex];
			}
		}
		for (int j = 0; j < ExpansionKele.projectileTypes.Length; j++)
		{
			if (ExpansionKele.projectileTypes[j] > 0 && ContentSamples.ProjectilesByType.ContainsKey(ExpansionKele.projectileTypes[j]))
			{
				return ExpansionKele.projectileTypes[j];
			}
		}
		return 2;
	}


        
// ... existing code ...

        public override void SetDefaults()
        {
            Item.damage = 200;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Master;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16f;
            Item.crit = 4;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = false;
            Item.noUseGraphic = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Projectile.NewProjectile((IEntitySource)(object)source, position, velocity, type, damage, knockback, ((Entity)player).whoAmI, 0f, 0f, 0f);
		return false;
	}

	public override void OnHitNPC(Player player, NPC target, HitInfo hit, int damageDone)
	{
		target.life -= damageDone * 3;
		if (target.life < 0)
		{
			target.life = 0;
		}
	}
// ... existing code ...
public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		SwitchDamageType();
		UpdateFloatParameters();
		int newType = 2;
		switch (currentDamageType)
		{
		case 3:
			newType = GetRandomValidProjectileType();
			break;
		case 0:
			newType = GetRandomValidProjectileType();
			break;
		case 1:
			newType = GetRandomValidProjectileType();
			break;
		case 2:
			newType = GetRandomValidProjectileType();
			break;
		}
		if (newType > 0 && ContentSamples.ProjectilesByType.ContainsKey(newType))
		{
			type = newType;
		}
		else
		{
			type = 2;
		}
	}

        private void UpdateFloatParameters()
	{
		float t1 = (float)Main.rand.NextDouble();
		float t2 = (float)Main.rand.NextDouble();
		float t3 = (float)Main.rand.NextDouble();
		float t4 = (float)Main.rand.NextDouble();
		damageMultiplier = (float)Math.Pow(2.0, (double)(2f * t1 - 1f));
		critDamageMultiplier = 1.5f + t2 * 1f;
		critChanceMultiplier = 0.75f + t3 * 0.5f;
		useTimeMultiplier = 0.75f + t4 * 0.5f;
	}
// ... existing code ...
public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
	{
		switch (currentDamageType)
		{
		case 3:
			damage = damage.CombineWith(player.GetDamage(DamageClass.Melee).Scale(3f));
			break;
		case 0:
			damage = damage.CombineWith(player.GetDamage(DamageClass.Ranged).Scale(3f));
			break;
		case 1:
			damage = damage.CombineWith(player.GetDamage(DamageClass.Magic).Scale(3f));
			break;
		case 2:
			damage = damage.CombineWith(player.GetDamage(DamageClass.Summon).Scale(3f));
			break;
		}
		damage = damage.CombineWith(new StatModifier(1f, damageMultiplier, 0f, 0f));
	}
// ... existing code ...

        public override bool CanUseItem(Player player)
        {
            // 切换到下一个伤害类型
            currentDamageType = (currentDamageType + 1) % 4;
            Item.useTime = (int)(20 * useTimeMultiplier);
            Item.useAnimation = (int)(20 * useTimeMultiplier);
            SwitchDamageType();
            return base.CanUseItem(player);
        }
// ... existing code ...

        private void SwitchDamageType()
        {
            switch (currentDamageType)
            {
                case meleeDamageType:
                    Item.DamageType = DamageClass.Melee;
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.noMelee = false;
                    Item.noUseGraphic = false;
                    break;
                case rangedDamageType:
                    Item.DamageType = DamageClass.Ranged;
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.noMelee = false;
                    Item.noUseGraphic = false;
                    break;
                case magicDamageType:
                    Item.DamageType = DamageClass.Magic;
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.noMelee = false;
                    Item.noUseGraphic = false;
                    break;
                case summonDamageType:
                    Item.DamageType = DamageClass.Summon;
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.noMelee=false;
                    Item.noUseGraphic = false;
                    break;
            }
            //Main.NewText($"SwitchDamageType!currentDamageType: {currentDamageType}");
        }
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(player, target, ref modifiers);
            // 应用暴击伤害浮动
            modifiers.CritDamage *= critDamageMultiplier/2;//防止有其他伤害倍率出现
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // // 添加说明文字
            // tooltips.Add(new TooltipLine(Mod, "ChaosDescription", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosDescription").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect3", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect3").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect4", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect4").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect7", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect7").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect8", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect8").Value));
            // tooltips.Add(new TooltipLine(Mod, "Pity", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Pity").Value));

            
            // string currentType = "";
            // switch(currentDamageType)
            // {
            //     case meleeDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Melee").Value; break;
            //     case rangedDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Ranged").Value; break;
            //     case magicDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Magic").Value; break;
            //     case summonDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Summon").Value; break;
            // }
            // tooltips.Add(new TooltipLine(Mod, "CurrentType", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.CurrentType").Format(currentType)));
        }
    }
}