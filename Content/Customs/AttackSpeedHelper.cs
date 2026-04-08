using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs;

/// <summary>
/// 攻击速度辅助类，用于计算物品在玩家使用时的实际使用时间
/// </summary>
/// <remarks>
/// 这个工具类整合了Terraria中各种影响攻击速度的因素：
/// - 原版物品的攻击速度加成
/// - 玩家和物品的使用速度倍数
/// - 玩家和物品的使用时间倍数
/// - 玩家特定伤害类型的基础攻击速度
/// </remarks>
public class UseTimeHelper
{
	/// <summary>
	/// 计算玩家使用指定物品时的实际使用时间（帧数）
	/// </summary>
	/// <param name="player">使用物品的玩家实例</param>
	/// <param name="item">要使用的物品实例</param>
	/// <returns>实际使用时间（帧数），最小值为1</returns>
	/// <remarks>
	/// 计算过程包含以下步骤：
	/// 1. 获取原版物品的攻击速度倍数加成
	/// 2. 获取玩家对物品的使用速度倍数
	/// 3. 获取物品对玩家的使用速度倍数
	/// 4. 获取玩家对物品的使用时间倍数
	/// 5. 获取物品对玩家的使用时间倍数
	/// 6. 计算玩家特定伤害类型的总攻击速度
	/// 7. 应用原版速度倍数修正
	/// 8. 综合所有倍数计算最终使用时间
	/// </remarks>
	public static int GetActualUseTime(Player player, Item item)
	{
		// 获取原版物品设置的攻击速度倍数加成
		float vanillaSpeedMult = ItemID.Sets.BonusAttackSpeedMultiplier[item.type];
		
		// 获取玩家对物品的使用速度倍数（来自玩家模组钩子）
		float playerSpeedMult = PlayerLoader.UseSpeedMultiplier(player, item);
		
		// 获取物品对玩家的使用速度倍数（来自物品模组钩子）
		float itemSpeedMult = ItemLoader.UseSpeedMultiplier(item, player);
		
		// 获取玩家对物品的使用时间倍数（来自玩家模组钩子）
		float playerTimeMult = PlayerLoader.UseTimeMultiplier(player, item);
		
		// 获取物品对玩家的使用时间倍数（来自物品模组钩子）
		float itemTimeMult = ItemLoader.UseTimeMultiplier(item, player);
		
		// 计算总攻击速度倍数，包括玩家特定类型的攻击速度
		float attackSpeed = player.GetTotalAttackSpeed(item.DamageType);
		
		// 应用原版速度倍数修正到攻击速度上
		attackSpeed = 1f + (attackSpeed - 1f) * vanillaSpeedMult;
		
		// 计算总的使用速度倍数（玩家倍数 × 物品倍数 × 攻击速度）
		float totalUseSpeedMult = playerSpeedMult * itemSpeedMult * attackSpeed;
		
		// 计算总的使用时间倍数（玩家时间倍数 × 物品时间倍数）
		float totalUseTimeMult = playerTimeMult * itemTimeMult;
		
		// 将使用时间倍数除以使用速度倍数得到最终的时间修正因子
		totalUseTimeMult /= totalUseSpeedMult;
		
		// 返回修正后的使用时间，确保至少为1帧
		return Math.Max(1, (int)((float)item.useTime * totalUseTimeMult));
	}

	/// <summary>
	/// 获取玩家使用物品时的总倍数
	/// </summary>
	/// <param name="player">使用物品的玩家实例</param>
	/// <param name="item">要使用的物品实例</param>
	/// <param name="includeAttackSpeed">是否包含攻击速度效果，默认为false</param>
	/// <returns>总倍数，值越大表示使用速度越快</returns>
	/// <remarks>
	/// 当includeAttackSpeed为false时，只考虑：
	/// - 玩家使用速度倍数
	/// - 物品使用速度倍数
	/// - 玩家使用时间倍数
	/// - 物品使用时间倍数
	/// - 原版物品攻击速度乘数
	/// 
	/// 当includeAttackSpeed为true时，额外考虑玩家的攻击速度属性
	/// </remarks>
	public static float GetTotalUseMultiplier(Player player, Item item, bool includeAttackSpeed = false)
	{
		float vanillaSpeedMult = ItemID.Sets.BonusAttackSpeedMultiplier[item.type];
		float playerSpeedMult = PlayerLoader.UseSpeedMultiplier(player, item);
		float itemSpeedMult = ItemLoader.UseSpeedMultiplier(item, player);
		float playerTimeMult = PlayerLoader.UseTimeMultiplier(player, item);
		float itemTimeMult = ItemLoader.UseTimeMultiplier(item, player);
		
		float totalMultiplier = playerSpeedMult * itemSpeedMult * vanillaSpeedMult / (playerTimeMult * itemTimeMult);
		
		if (includeAttackSpeed)
		{
			float attackSpeed = player.GetTotalAttackSpeed(item.DamageType);
			attackSpeed = 1f + (attackSpeed - 1f) * vanillaSpeedMult;
			totalMultiplier *= attackSpeed;
		}
		
		return totalMultiplier;
	}
}