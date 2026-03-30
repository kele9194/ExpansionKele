using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.OtherItem.BagItem
{
    public class RangerRunicTablet : ModItem
    {
        public const float MaxDistance = 240f;
        public const float DistanceStep = 16f;
        public const float CritBonusPerStep = 0.01f;
        public const float MaxCritBonus = 0.05f;
        public const int RangedDamageBonus = 0;
        public const int ArmorPenetrationBonus = 3;
        public const int AggroReduction = 50;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(RangedDamageBonus),
            ValueUtils.FormatValue(ArmorPenetrationBonus),
            ValueUtils.FormatValue(AggroReduction),
            ValueUtils.FormatValue(MaxDistance),
            ValueUtils.FormatValue(DistanceStep),
            ValueUtils.FormatValue(CritBonusPerStep,true),
            ValueUtils.FormatValue(MaxCritBonus,true)
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
                player.GetModPlayer<MarksmanRuneStonePlayer>().marksmanRuneStoneEquipped = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.WoodenArrow, 50)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }

    public class MarksmanRuneStonePlayer : ModPlayer
    {
        public bool marksmanRuneStoneEquipped = false;

        public override void ResetEffects()
        {
            marksmanRuneStoneEquipped = false;
        }
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (item.DamageType == DamageClass.Ranged || item.DamageType.CountsAsClass(DamageClass.Ranged))
            {
                damage.Flat += RangerRunicTablet.RangedDamageBonus;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            if (marksmanRuneStoneEquipped)
            {
                Player.GetArmorPenetration(DamageClass.Ranged) += RangerRunicTablet.ArmorPenetrationBonus;
                Player.aggro -= RangerRunicTablet.AggroReduction;

                float closestEnemyDistance = float.MaxValue;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5)
                    {
                        float distance = Vector2.Distance(Player.Center, npc.Center);
                        if (distance < closestEnemyDistance)
                        {
                            closestEnemyDistance = distance;
                        }
                    }
                }

                if (closestEnemyDistance == float.MaxValue)
                {
                    closestEnemyDistance = RangerRunicTablet.MaxDistance;
                }

                if (closestEnemyDistance > RangerRunicTablet.MaxDistance)
                {
                    float steps = (closestEnemyDistance - RangerRunicTablet.MaxDistance) / RangerRunicTablet.DistanceStep;
                    float critBonus = steps * RangerRunicTablet.CritBonusPerStep;
                    critBonus = MathHelper.Min(critBonus, RangerRunicTablet.MaxCritBonus);
                    Player.GetCritChance(DamageClass.Ranged) += critBonus * 100;
                }
            }
        }
    }
}