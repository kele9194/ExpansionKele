using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
namespace ExpansionKele.Content.Items.Accessories
{
    public class StarryWarriorEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float MeleeDamageBonus = 0.15f; // 15% 近战伤害加成
        private const float AttackSpeedBonus = 0.18f; // 10% 攻击速度加成
        private const float DamageToSpeedRatio = 0.3f; // 每1%额外近战伤害增加的攻击速度百分比
        private const float MeleeScaleIncrease = 1f; //近战武器尺寸增加

        public override void SetDefaults()
        {
            //Item.SetNameOverride("勇气星元徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.Cyan;
            Item.accessory = true;
        }

        // ... existing code ...
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ExpansionKelePlayer modPlayer = player.GetModPlayer<ExpansionKelePlayer>();
            
            // 检查当前玩家是否已经有星元徽章生效，且不是自己
            if (modPlayer.activeStarryEmblemType != -1 && 
                modPlayer.activeStarryEmblemType != Item.type)
                return; // 如果已经有其他类型的徽章生效了，则直接返回，不应用效果
                
            // 标记当前玩家已经有星元魔法师徽章生效
            modPlayer.activeStarryEmblemType = Item.type;


            
            // +15%近战伤害
            player.GetDamage(DamageClass.Melee) += MeleeDamageBonus;
            
            // +10%攻击速度
            player.GetAttackSpeed(DamageClass.Melee) += AttackSpeedBonus;
            
            // 每1%额外近战伤害增加0.30%攻速
            float additionalMeleeDamage = player.GetDamage(DamageClass.Melee).Additive - 1f;
            additionalMeleeDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.GetAttackSpeed(DamageClass.Melee) += additionalMeleeDamage * DamageToSpeedRatio;
            
            // 允许自动挥舞
            player.autoReuseGlove = true;
            player.GetModPlayer<MeleeScalePlayer>().hasStarryWarriorEmblem = true;
            
            // 增加近战武器尺寸
            player.GetModPlayer<MeleeScalePlayer>().meleeScaleIncrease += MeleeScaleIncrease;
        }
// ... existing code ...

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"StarryWarriorEmblemDamage", $"[c/00FF00:+{MeleeDamageBonus * 100}%近战伤害]"},
                    {"StarryWarriorEmblemSpeed", $"[c/00FF00:+{AttackSpeedBonus * 100}%攻击速度]"},
                    {"StarryWarriorEmblemBonus", $"[c/00FF00:每1%额外近战伤害增加{DamageToSpeedRatio}%攻击速度]"},
                    {"StarryWarriorEmblemAuto", "[c/00FF00:允许自动挥舞]"},
                    {"StarryWarriorEmblemBonus2", $"[c/00FF00:武器尺寸+{MeleeScaleIncrease*100}%]"},
                    {"StarryWarriorEmblemBonus3",$"[c/00FF00:,大多数剑刃命中敌人都可以回血和大幅提升生命再生]"},
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
            recipe.AddIngredient(ModContent.ItemType<MoonWarriorEmblem>(), 1);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }

    // ... existing code ...
    public class MeleeScalePlayer : ModPlayer
    {
        public float meleeScaleIncrease = 0f;
        public bool hasStarryWarriorEmblem = false;
        private float originalScale = 1f;

        public override void ResetEffects()
        {
            meleeScaleIncrease = 0f;
            hasStarryWarriorEmblem = false;
        }

        public override void PostUpdateEquips()
        {
            DamageClass trueMeleeDamageClass = null;
            if (ExpansionKele.calamity != null)
            {
                trueMeleeDamageClass = ExpansionKele.calamity.Find<DamageClass>("TrueMeleeDamageClass");
            }

            if (Player.HeldItem != null && Player.HeldItem.damage > 0)
            {
                if (meleeScaleIncrease > 0f)
                {
                    // 只有当武器改变时才重置尺寸
                    if (Player.HeldItem.scale != originalScale + meleeScaleIncrease &&
                        (Player.HeldItem.DamageType == DamageClass.Melee || Player.HeldItem.DamageType == trueMeleeDamageClass))
                    {
                        // 保存原始尺寸
                        if (Player.HeldItem.scale == 1f || Player.HeldItem.scale == originalScale)
                        {
                            originalScale = Player.HeldItem.scale;
                        }
                        // 设置新尺寸
                        Player.HeldItem.scale = originalScale + meleeScaleIncrease;
                    }
                }
                else
                {
                    // 如果没有加成，恢复原始尺寸
                    if (Player.HeldItem.scale != originalScale && originalScale != 1f)
                    {
                        Player.HeldItem.scale = originalScale;
                        originalScale = 1f;
                    }
                }
            }
            meleeScaleIncrease = 0f;
        }
        
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 检查是否装备了星月战士徽章
            if (hasStarryWarriorEmblem)
            {
                // 每次真近战击中敌人增加生命再生时间100，生命再生+2，直接回血4点
                Player.lifeRegenTime += 25;
                Player.lifeRegen += 1;
                Player.Heal(1);
            }
        }
    }
// ... existing code ...
    }