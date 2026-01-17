using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using System;
using ExpansionKele.Content.Customs;
namespace ExpansionKele.Content.Items.Accessories
{
    public class StarryWarriorEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float MeleeDamageBonus = 0.15f; // 15% 近战伤害加成
        private const float AttackSpeedBonus = 0.18f; // 18% 攻击速度加成
        private const float DamageToSpeedRatio = 0.3f; // 每1%额外近战伤害增加的攻击速度百分比
        private const float MeleeScaleIncrease = 0.5f; //近战武器尺寸增加50%

        private const float AttackSpeedToDRbonus = 0.01f/0.05f;

        private const float AttackSpeedToDEFBonus = 1f/0.05f; 

        public override void SetDefaults()
        {
            //Item.SetNameOverride("勇气星元徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
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
            if(player.HeldItem.DamageType == DamageClass.MeleeNoSpeed){
            float extraMeleeSpeed=GetExtraMaxMeleeSpeed(player);
            float damageReductionMultiplier = Math.Max(0.01f, 1 - (extraMeleeSpeed*AttackSpeedToDRbonus)); // 确保不会变成负数
            player.GetModPlayer<CustomDamageReductionPlayer>().MulticustomDamageReduction(damageReductionMultiplier);
            player.statDefense+=(int)(extraMeleeSpeed*AttackSpeedToDEFBonus);
            }
        }

        public static float GetExtraMaxMeleeSpeed(Player player){
            float meleeSpeed;
            float VanillameleeSpeed=player.GetTotalAttackSpeed(DamageClass.Melee);
            float trueMeleeSpeed=0f;
            if(ExpansionKele.calamity != null){
                trueMeleeSpeed=player.GetTotalAttackSpeed(ExpansionKele.calamity.Find<DamageClass>("TrueMeleeDamageClass"));
            }
            meleeSpeed=Math.Max(trueMeleeSpeed,VanillameleeSpeed);
                // 修正伤害减免计算公式，避免出现负数伤害乘数
                float speedBonus = meleeSpeed - 1f; // 确保speedBonus不为负数
                return speedBonus;
                
        }
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
                    {"StarryWarriorEmblemBonus3",$"[c/00FF00:,近距离命中敌人都可以回血和大幅提升生命再生]"},
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
    // ... existing code ...
    // ... existing code ...
public class MeleeScalePlayer : ModPlayer
    {
        public float meleeScaleIncrease = 0f;
        public bool hasStarryWarriorEmblem = false;
        // 删除冷却相关变量
        // public int healCooldownTimer = 0; // 新增：治疗冷却计时器
        // public int healCooldownDuration = 15; // 新增：治疗冷却时间（以tick为单位，默认30tick = 0.5秒）
        // public int cooldownReductionPerHit = 1; // 新增：每次命中减少的冷却时间
        
        // 新增：用于跟踪最近击中的敌人数量
        private List<int> recentlyHitNPCs = new List<int>();
        private int hitTrackingTimer = 0;
        private const int HIT_TRACKING_DURATION = 60; // 跟踪最近1秒内的击中目标

        public override void ResetEffects()
        {
            meleeScaleIncrease = 0f;
            hasStarryWarriorEmblem = false;
            // 移除冷却计时器更新逻辑
            // healCooldownTimer = Math.Max(0, healCooldownTimer - 1); // 冷却计时递减，但不能低于0
            
            // 更新击中追踪计时器
            hitTrackingTimer = Math.Max(0, hitTrackingTimer - 1);
            if(hitTrackingTimer <= 0)
            {
                recentlyHitNPCs.Clear(); // 清除过期的记录
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 添加当前击中的NPC到列表（如果还没有记录的话）
            if (!recentlyHitNPCs.Contains(target.whoAmI))
            {
                recentlyHitNPCs.Add(target.whoAmI);
                hitTrackingTimer = HIT_TRACKING_DURATION; // 重置追踪计时器
            }
            
            // 检查是否装备了星月战士徽章
            if (hasStarryWarriorEmblem)
            {
                // 检查玩家当前使用的是否是近战武器
                if (Player.HeldItem != null && (Player.HeldItem.DamageType == DamageClass.Melee || ScaleModifierGlobalItem.IsTrueMelee(Player.HeldItem)))
                {
                    // 移除冷却检查和减少逻辑
                    // if (healCooldownTimer > 0)
                    // {
                    //     healCooldownTimer = Math.Max(0, healCooldownTimer - cooldownReductionPerHit); // 减少冷却时间，但不低于0
                    // }

                    // 移除冷却检查条件
                    // if (healCooldownTimer <= 0)
                    // {
                        // 计算玩家与目标之间的距离
                        float distance = Vector2.Distance(Player.Center, target.Center);
                        
                        // 如果距离小于100像素，直接触发效果
                        if (distance <= 100f)
                        {
                            // 计算基于击中敌人数量的治疗系数作为触发概率
                            int currentHitCount = recentlyHitNPCs.Count;
                            float healingProbability = CalculateHealingProbability(currentHitCount);
                            
                            // 使用计算出的概率决定是否触发治疗效果
                            if (Main.rand.NextFloat() <= healingProbability)
                            {
                                // 增加生命再生时间，提升生命再生，直接回血（不再乘以系数）
                                Player.lifeRegenTime += 25;
                                Player.lifeRegen += 1;
                                Player.Heal(1);
                                // healCooldownTimer = healCooldownDuration; // 移除冷却机制
                            }
                        }
                        // 如果距离在100到300像素之间，计算触发概率
                        else if (distance <= 300f)
                        {
                            // 计算从1线性减少到0的概率
                            float distanceProbability = (300f - distance) / (300f - 100f); // 当distance=100时概率为1，当distance=300时概率为0
                            
                            // 计算基于击中敌人数量的治疗系数作为触发概率
                            int currentHitCount = recentlyHitNPCs.Count;
                            float healingProbability = CalculateHealingProbability(currentHitCount);
                            
                            // 将距离概率与击中敌人数量概率结合
                            float finalProbability = distanceProbability * healingProbability;
                            
                            // 检查随机数是否小于等于最终概率
                            if (Main.rand.NextFloat() <= finalProbability)
                            {
                                // 增加生命再生时间，提升生命再生，直接回血（不再乘以系数）
                                Player.lifeRegenTime += 25;
                                Player.lifeRegen += 1;
                                Player.Heal(1);
                                // healCooldownTimer = healCooldownDuration; // 移除冷却机制
                            }
                        }
                    // }
                }
            }
        }
        
        // 根据击中敌人数量计算治疗概率的方法
        private float CalculateHealingProbability(int hitCount)
        {
            // 当只击中1个敌人时，提供最高治疗概率
            // 随着击中敌人数量增加，治疗概率降低
            // 使用新的公式: probability = 2.5 / (hitCount + 1.5)
            float probability = 1.8f / (hitCount + 0.8f);
            return Math.Min(1.0f, probability); // 确保概率不超过100%
        }
    }
// ... existing code ...
// ... existing code ...

// ... existing code ...

    public class ScaleModifierGlobalItem : GlobalItem
    {
        public override  void ModifyItemScale(Item item, Player player, ref float scale)
        {
            // 检查玩家是否装备了星元战士徽章
            var modPlayer = player.GetModPlayer<MeleeScalePlayer>();
            
            if (modPlayer.hasStarryWarriorEmblem && 
                (item.DamageType == DamageClass.Melee || IsTrueMelee(item)))
            {
                scale *= (1+modPlayer.meleeScaleIncrease);
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