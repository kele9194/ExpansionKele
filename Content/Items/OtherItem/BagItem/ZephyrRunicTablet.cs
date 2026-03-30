using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;
using System;

namespace ExpansionKele.Content.Items.OtherItem.BagItem
{
    public class ZephyrRunicTablet : ModItem
    {
        public const float FlightAccelerationBonus = 1f; // 25% 飞行加速度
        public const float MaxFlightSpeedBonus = 1f; // 5% 最大飞行速度
        public const float MoveSpeedBonus = 0.5f; // 5% 移动速度
        public const float MoveAccelerationBonus = 1f; // 25% 移动加速度
        public const float MoveDecelerationBonus = 1f; // 25% 移动减速度
        
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(FlightAccelerationBonus),
            ValueUtils.FormatValue(MaxFlightSpeedBonus),
            ValueUtils.FormatValue(MoveSpeedBonus),
            ValueUtils.FormatValue(MoveAccelerationBonus),
            ValueUtils.FormatValue(MoveDecelerationBonus)
        );

        public override string LocalizationCategory => "Items.OtherItem";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
            {
                player.GetModPlayer<ZephyrRunicTabletPlayer>().zephyrRuneEquipped = true;
            }
        }

        public override void AddRecipes()
        {

        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }

    public class ZephyrRunicTabletPlayer : ModPlayer
    {
        public bool zephyrRuneEquipped = false;

        public override void ResetEffects()
        {
            zephyrRuneEquipped = false;
        }

        public override void PostUpdateRunSpeeds()
    {
            if (zephyrRuneEquipped)
            {
                // Player.flightAccel += ZephyrRunicTablet.FlightAccelerationBonus;
                // Player.flightDeccel += ZephyrRunicTablet.FlightDecelerationBonus;
                Player.accRunSpeed *= (1f + ZephyrRunicTablet.MoveSpeedBonus);
                Player.runAcceleration *= (1f + ZephyrRunicTablet.MoveAccelerationBonus);
                Player.runSlowdown *= (1f + ZephyrRunicTablet.MoveDecelerationBonus);
            }

            
        }

        public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
	{
        if (zephyrRuneEquipped)
        {
            // 飞行效果
            speed *= (1f + ZephyrRunicTablet.MaxFlightSpeedBonus);      // 飞行速度 +5%
            acceleration *= (1f + ZephyrRunicTablet.FlightAccelerationBonus); // 飞行加速度 +25%
        }
	}
    }
}