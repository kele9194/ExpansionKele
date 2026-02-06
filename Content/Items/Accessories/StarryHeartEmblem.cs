using System.Collections.Generic;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Accessories
{
    public class StarryHeartEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 常量定义
        private const int BaseLifeMaxBonus = 100;
        private const float LifeMaxMultiplier = 1.15f;
        private const float LifeRegenBonus = 1.5f;
        private const float LifeRegenThreshold = 0.5f;
        private const float HealThreshold = 0.2f;
        private const float LifeRegenProbability = 0.02f;
        private const int HealInterval = 20;
        private const int HealAmount = 1;
        public const float DamageHealPercent = 0.15f;
        public const int DamageRegenTimeBonus = 300;
        public const float FatalHealPercent = 0.2f;
        public const int FatalImmuneTime = 60;
        public const int FatalCooldown = 3600;

        public override void SetDefaults()
        {
            //Item.SetNameOverride("生命星元徽章");
            Item.width = 32;
            Item.height = 36;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
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


            
            // 应用生命值加成
            player.statLifeMax2 += BaseLifeMaxBonus;
            player.statLifeMax2 = (int)(player.statLifeMax2 * LifeMaxMultiplier);
            
            // 生命再生加成
            player.lifeRegen += (int)LifeRegenBonus;

            player.GetModPlayer<StarryLifeEmblemPlayer>().isStarryHeartEmblemEquipped = true;

            // 获取玩家当前生命百分比
            float lifePercentage = (float)player.statLife / player.statLifeMax2;

            // 当生命值低于50%时，每减少1%的血量，生命回复时间有2%的概率增加2
            if (lifePercentage < LifeRegenThreshold)
            {
                float belowThreshold = LifeRegenThreshold - lifePercentage;
                float percentBelow = belowThreshold * 100f;
                
                // 每1%血量有2%概率增加2点生命回复时间
                if (Main.rand.NextFloat() < LifeRegenProbability * percentBelow)
                {
                    player.lifeRegenTime += 2;
                }
            }

            // 当生命值低于20%时，每过20帧回复1生命值
            if (lifePercentage < HealThreshold)
            {
                var modPlayerA = player.GetModPlayer<StarryLifeEmblemPlayer>();
                if (++modPlayerA.lowHealthTimer >= HealInterval)
                {
                    modPlayerA.lowHealthTimer = 0;
                    if (player.statLife < player.statLifeMax2)
                    {
                        player.statLife = Utils.Clamp(player.statLife + HealAmount, 0, player.statLifeMax2);
                        player.HealEffect(HealAmount, true);
                    }
                }
            }

            
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MoonLifeEmblem>(), 1);
            recipe.AddIngredient(ItemID.LifeFruit, 5);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
        
        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"LifeMaxBonus1", $"[c/00FF00:+{BaseLifeMaxBonus} 最大生命值]"},
                    {"LifeMaxBonus2", $"[c/00FF00:+{(LifeMaxMultiplier - 1f) * 100}% 最大生命值]"},
                    {"LifeRegenBonus", $"[c/00FF00:+{LifeRegenBonus} 生命再生]"},
                    {"LowLifeRegenBonus", $"[c/00FF00:生命值低于{LifeRegenThreshold * 100}%时，每减少1%生命值，生命回复时间有{LifeRegenProbability * 100}%概率增加2]"},
                    {"LowHealthHeal", $"[c/00FF00:生命值低于{HealThreshold * 100}%时，每过{HealInterval/60f}s回复{HealAmount}生命值]"},
                    {"DamageHeal", $"[c/00FF00:每次受到伤害都会回复少量的回复量，并且将生命再生时间+{DamageRegenTimeBonus}]"},
                    {"FatalHeal", $"[c/00FF00:当受到致命伤害时形成{FatalImmuneTime/60f}s的无敌帧，并且回复自身最大生命值{FatalHealPercent * 100}%的血量]"},
                    {"FatalCooldown", $"[c/00FF00:致命伤害保护的冷却时间为{FatalCooldown/60f}s]"},
                    {"WARNING", "[c/800000:注意：多个星元徽章装备将只有第一个生效]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...
    }

    // ... existing code ...
public class StarryLifeEmblemPlayer : ModPlayer
    {
        public int lowHealthTimer = 0;
        public int fatalCooldownTimer = StarryHeartEmblem.FatalCooldown; // 初始冷却时间
        public bool isStarryHeartEmblemEquipped = false; // 新增布尔变量

        public override void ResetEffects()
        {
            // 重置装备状态
            isStarryHeartEmblemEquipped = false;
            // 只有佩戴StarryHeartEmblem时才重置
            if (isStarryHeartEmblemEquipped)
            {
                lowHealthTimer = 0;
            }
        }

        // ... existing code ...
        public override void PreUpdate()
        {
            // 只有佩戴StarryHeartEmblem时才更新冷却计时器
            if (isStarryHeartEmblemEquipped)
            {
                // 更新冷却计时器
                if (fatalCooldownTimer > 0)
                {
                    fatalCooldownTimer--;
                    if(fatalCooldownTimer == 0){
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position);
                        // 冷却结束时移除buff
                        Player.ClearBuff(ModContent.BuffType<StarryHeartEmblemCooldown>());
                    }
                }
                // 如果冷却计时器为0但是还有buff，清理掉buff
                else if (Player.HasBuff(ModContent.BuffType<StarryHeartEmblemCooldown>()))
                {
                    Player.ClearBuff(ModContent.BuffType<StarryHeartEmblemCooldown>());
                }
            }
            // 如果没有装备饰品但是有buff，清理掉buff
            else if (Player.HasBuff(ModContent.BuffType<StarryHeartEmblemCooldown>()))
            {
                Player.ClearBuff(ModContent.BuffType<StarryHeartEmblemCooldown>());
                fatalCooldownTimer = 0;
            }
        }
// ... existing code ...

        // ... existing code ...
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            // 只有佩戴StarryHeartEmblem时才触发效果
            if (isStarryHeartEmblemEquipped)
            {
                // 检查是否是致命伤害（会导致玩家死亡）
                if (Player.statLife - (int)damage <= 0)
                {
                    // 当受到致命伤害时，如果冷却时间已结束
                    if (fatalCooldownTimer <= 0)
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.position);
                        
                        // 形成60帧的无敌帧
                        Player.SetImmuneTimeForAllTypes(StarryHeartEmblem.FatalImmuneTime);
                        
                        // 回复自身最大生命值20%的血量
                        int healAmount = (int)(Player.statLifeMax2 * StarryHeartEmblem.FatalHealPercent);
                        Player.statLife = Utils.Clamp(Player.statLife + healAmount, 0, Player.statLifeMax2);
                        Player.HealEffect(healAmount, true);
                        
                        // 重置冷却时间
                        fatalCooldownTimer = StarryHeartEmblem.FatalCooldown;
                        
                        // 应用冷却buff
                        Player.AddBuff(ModContent.BuffType<StarryHeartEmblemCooldown>(), StarryHeartEmblem.FatalCooldown);
                        
                        // 取消死亡
                        playSound = false;
                        genDust = false;
                        
                        // 阻止死亡
                        return false;
                    }
                }
            }
            
            // 允许正常死亡流程
            return true;
        }
// ... existing code ...

        public override void OnHurt(Player.HurtInfo info)
        {
            // 只有佩戴StarryHeartEmblem时才触发效果
            if (isStarryHeartEmblemEquipped)
            {
                // 每次受到伤害都会回复受到伤害10%的回复量，并且将生命再生时间+300
                int damageToHeal = (int)(info.Damage * StarryHeartEmblem.DamageHealPercent);
                // Main.NewText(damageToHeal);
                if (damageToHeal > 0)
                {
                    Player.Heal(damageToHeal);
                    //Player.HealEffect(damageToHeal, true);
                    Player.lifeRegenTime += StarryHeartEmblem.DamageRegenTimeBonus;
                }
            }
        }
// ... existing code ...
// ... existing code ...
    }
}