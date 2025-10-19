using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
namespace ExpansionKele.Content.Items.Accessories
{
    public class StarrySummonerEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float DamageBonus = 0.15f; // +15%伤害
        private const float CritChanceBonus = 0.02f; // +6%暴击率
        private const int MinionSlotBonus = 2; // +2召唤栏栏位
        private const int TurretSlotBonus = 1; // +1哨兵栏位
        private const float WhipRangeBonus = 0.25f; // +25%鞭子范围
        private const float WhipSpeedBonus = 0.15f; // +15%鞭子攻速
        private const float DamageToWhipRangeRatio = 0.5f; // 每1%额外召唤伤害增加的鞭子范围百分比
        private const float DamageToWhipSpeedRatio = 0.4f; // 每1%额外召唤伤害增加的鞭子攻速百分比

        public override void SetDefaults()
        {
            //Item.SetNameOverride("团结星元徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.Yellow;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ExpansionKelePlayer modPlayer = player.GetModPlayer<ExpansionKelePlayer>();
            
            // 检查当前玩家是否已经有星元徽章生效，且不是自己
            if (modPlayer.activeStarryEmblemType != -1 && 
                modPlayer.activeStarryEmblemType != Item.type)
                return; // 如果已经有其他类型的徽章生效了，则直接返回，不应用效果
                
            // 标记当前玩家已经有星元魔法师徽章生效
            modPlayer.activeStarryEmblemType = Item.type;


            // +15%伤害（所有类型）
            player.GetDamage(DamageClass.Generic) += DamageBonus;
            
            // +6%暴击率
            player.GetCritChance(DamageClass.Generic) += CritChanceBonus * 100;
            
            // +2召唤栏栏位
            player.maxMinions += MinionSlotBonus;
            
            // +1哨兵栏位
            player.maxTurrets += TurretSlotBonus;
            
            // +25%鞭子范围
            player.whipRangeMultiplier += WhipRangeBonus;
            
            // +15%鞭子攻速
            player.GetAttackSpeed(DamageClass.Summon) += WhipSpeedBonus;
            
            // 允许将暴击率按1:1增加到召唤伤害
            player.GetDamage(DamageClass.Summon) += player.GetCritChance(DamageClass.Generic) / 100f;
            
            // 每1%额外召唤伤害增加0.5%鞭子范围和0.4%鞭子攻速
            float additionalSummonDamage = player.GetDamage(DamageClass.Summon).Additive - 1f;
            additionalSummonDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.whipRangeMultiplier += additionalSummonDamage * DamageToWhipRangeRatio;
            player.GetAttackSpeed(DamageClass.Summon) += additionalSummonDamage * DamageToWhipSpeedRatio;
            
            // 仆从数和哨兵数每个额外增加0.25的生命再生和4点防御
            float regenBonus = (player.maxMinions + player.maxTurrets) * 0.25f;
            float defenseBonus = (player.maxMinions + player.maxTurrets) * 4f;
            player.lifeRegen += (int)regenBonus;
            player.statDefense += (int)defenseBonus;
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"StarrySummonerEmblemDamage", $"[c/00FF00:+{DamageBonus * 100}%伤害]"},
                    {"StarrySummonerEmblemCrit", $"[c/00FF00:+{CritChanceBonus * 100}%暴击率]"},
                    {"StarrySummonerEmblemMinion", $"[c/00FF00:+{MinionSlotBonus}召唤栏栏位]"},
                    {"StarrySummonerEmblemTurret", $"[c/00FF00:+{TurretSlotBonus}哨兵栏位]"},
                    {"StarrySummonerEmblemWhipRange", $"[c/00FF00:+{WhipRangeBonus * 100}%鞭子范围]"},
                    {"StarrySummonerEmblemWhipSpeed", $"[c/00FF00:+{WhipSpeedBonus * 100}%鞭子攻速]"},
                    {"StarrySummonerEmblemCritToDamage", "[c/00FF00:允许将暴击率按1:1增加到召唤伤害]"},
                    {"StarrySummonerEmblemBonus1", "[c/00FF00:仆从数和哨兵数每个额外增加0.25的生命再生和4点防御]"},
                    {"StarrySummonerEmblemBonus2", $"[c/00FF00:每1%额外召唤伤害增加{DamageToWhipRangeRatio}%鞭子范围和{DamageToWhipSpeedRatio}%鞭子攻速]"},
                    {"WARNING", "[c/800000:注意：多个星元徽章装备将只有第一个生效]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MoonSummonerEmblem>(), 1);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}