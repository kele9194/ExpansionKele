using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;
using System;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonWarriorEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float MeleeDamageBonus = 0.12f; // 12% 近战伤害加成
        private const float AttackSpeedBonus = 0.08f; // 8% 攻击速度加成
        private const float DamageToSpeedRatio = 0.26f; // 每1%额外近战伤害增加的攻击速度百分比

        private const float AttackSpeedToDRbonus = 0.01f/0.05f;
        private const float AttackSpeedToDEFBonus = 1f/0.05f;

        public override void SetDefaults()
        {
            //Item.SetNameOverride("战士满月徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override bool? UseItem(Player player)
        {
            // 移除模式切换功能，保留UseItem方法但不做任何操作
            return true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ExpansionKelePlayer modPlayer = player.GetModPlayer<ExpansionKelePlayer>();
            
            // 检查当前玩家是否已经有星元徽章生效，且不是自己
            if (modPlayer.activeMoonEmblemType != -1 && 
                modPlayer.activeMoonEmblemType != Item.type)
                return; // 如果已经有其他类型的徽章生效了，则直接返回，不应用效果
                
            // 标记当前玩家已经有星元魔法师徽章生效
            modPlayer.activeMoonEmblemType = Item.type;
            // +12%近战伤害
            player.GetDamage(DamageClass.Melee) += MeleeDamageBonus;
            
            // +8%攻击速度
            player.GetAttackSpeed(DamageClass.Melee) += AttackSpeedBonus;
            
            // 每1%额外近战伤害增加0.26%攻速
            float additionalMeleeDamage = player.GetDamage(DamageClass.Melee).Additive - 1f;
            additionalMeleeDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            
            // 总是应用完整的攻击速度加成（移除模式切换功能）
            player.GetAttackSpeed(DamageClass.Melee) += additionalMeleeDamage * DamageToSpeedRatio;
            
            // 允许自动挥舞
            player.autoReuseGlove = true;
            
            // 对非攻速武器的补偿机制
            if(player.HeldItem.DamageType == DamageClass.MeleeNoSpeed){
                float extraMeleeSpeed = StarryWarriorEmblem.GetExtraMaxMeleeSpeed(player);
                float damageReductionMultiplier = Math.Max(0.01f, 1 - (extraMeleeSpeed * AttackSpeedToDRbonus)); // 确保不会变成负数
                player.GetModPlayer<CustomDamageReductionPlayer>().MulticustomDamageReduction(damageReductionMultiplier);
                player.statDefense+=(int)(extraMeleeSpeed * AttackSpeedToDEFBonus);
            }
        }

       

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                ExpansionKelePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExpansionKelePlayer>();
                string modeText = "固定模式（移除了切换功能）"; // 显示当前模式状态
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoonWarriorEmblemDamage", $"[c/00FF00:+{MeleeDamageBonus * 100}%近战伤害]"},
                    {"MoonWarriorEmblemSpeed", $"[c/00FF00:+{AttackSpeedBonus * 100}%攻击速度]"},
                    {"MoonWarriorEmblemBonus", $"[c/00FF00:每1%额外近战伤害增加{DamageToSpeedRatio}%攻击速度]"},
                    {"MoonWarriorEmblemMode", $"[c/00FF00:当前模式: {modeText}]"},
                    {"MoonWarriorEmblemAuto", "[c/00FF00:允许自动挥舞]"},
                    {"WARNING", "[c/800000:注意：多个满月徽章装备将只有第一个生效]"}
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
            recipe.AddIngredient(ModContent.ItemType<FullMoonBar>(), 3);
            recipe.AddIngredient(ItemID.WarriorEmblem, 1);
            recipe.AddIngredient(ModContent.ItemType<Whetstone>(), 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }

    public class ScaleModifierGlobalItemSmaller : GlobalItem
    {
        public override  void ModifyItemScale(Item item, Player player, ref float scale)
        {
            ExpansionKelePlayer modPlayer = player.GetModPlayer<ExpansionKelePlayer>();
            if (modPlayer.activeMoonEmblemType==ModContent.ItemType<MoonWarriorEmblem>() &&
                (item.DamageType == DamageClass.Melee || IsTrueMelee(item)))
            {
                scale *= 1.15f;
            }
            
        }

        public static bool IsTrueMelee(Item item)
        {
            // 如果启用了Calamity模组，检查是否为真近战
            if (ExpansionKele.calamity != null)
            {
                var trueMeleeDamageClass = ExpansionKele.calamity.Find<DamageClass>("TrueMeleeDamageClass");
                return item.DamageType == trueMeleeDamageClass;
            }
            return false;
        }
    }
}