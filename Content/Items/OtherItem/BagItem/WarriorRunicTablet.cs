using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.OtherItem.BagItem
{
    public class WarriorRunicTablet : ModItem
    {
        public const float HealPercent = 0.025f;
        public const float MaxDistance = 100f;
        public const int CooldownTime = 90;
        public const float AttackSpeedBonus = 0.02f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(MaxDistance),
            ValueUtils.FormatValue(HealPercent,true),
            ValueUtils.FormatValue(CooldownTime/60f),
            ValueUtils.FormatValue(AttackSpeedBonus,true)
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
                player.GetModPlayer<RuneStoneTabletPlayer>().runeStoneEquipped = true;
            }
            Wrench.CheckAndFixAutoUse(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.CopperShortsword, 1)
                .AddTile(TileID.Anvils)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.TinShortsword, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }

    public class RuneStoneTabletPlayer : ModPlayer
    {
        public bool runeStoneEquipped = false;
        public int healCooldownTimer = 0;

        public override void ResetEffects()
        {
            runeStoneEquipped = false;
        }

        //<summary>//
        //这个更新非常重要//
        //<summary>//
        public override void PostUpdateMiscEffects()
        {
            if (healCooldownTimer > 0)
            {
                healCooldownTimer--;
            }
            if (runeStoneEquipped)
            {
                Player.GetAttackSpeed(DamageClass.Melee) += 0.02f;
            }
        }
 
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!runeStoneEquipped || healCooldownTimer > 0)
                return;


            if (Player.HeldItem == null || (Player.HeldItem.DamageType != DamageClass.Melee && !Player.HeldItem.DamageType.CountsAsClass(DamageClass.Melee)))
                return;

            float distance = Vector2.Distance(Player.Center, target.Center);
            
            if (distance <= WarriorRunicTablet.MaxDistance)
            {
                int healAmount = ValueUtils.ProbabilisticRound(Player.statLifeMax2 * WarriorRunicTablet.HealPercent);
                
                if (healAmount < 1)
                    healAmount = 1;

                Player.Heal(healAmount);
                healCooldownTimer = WarriorRunicTablet.CooldownTime;
            }
        }
    }
}