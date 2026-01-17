using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs;

public class UseTimeHelper
{
	public static int GetActualUseTime(Player player, Item item)
	{
		float vanillaSpeedMult = ItemID.Sets.BonusAttackSpeedMultiplier[item.type];
		float playerSpeedMult = PlayerLoader.UseSpeedMultiplier(player, item);
		float itemSpeedMult = ItemLoader.UseSpeedMultiplier(item, player);
		float playerTimeMult = PlayerLoader.UseTimeMultiplier(player, item);
		float itemTimeMult = ItemLoader.UseTimeMultiplier(item, player);
		
		// 计算总攻击速度倍数，包括玩家特定类型的攻击速度
		float attackSpeed = player.GetTotalAttackSpeed(item.DamageType);
		
		// 应用原版速度倍数
		attackSpeed = 1f + (attackSpeed - 1f) * vanillaSpeedMult;
		float totalUseSpeedMult = playerSpeedMult * itemSpeedMult * attackSpeed;
		float totalUseTimeMult = playerTimeMult * itemTimeMult;
		totalUseTimeMult /= totalUseSpeedMult;
		
		return Math.Max(1, (int)((float)item.useTime * totalUseTimeMult));
	}
}
