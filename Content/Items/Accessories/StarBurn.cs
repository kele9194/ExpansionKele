using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Accessories
{
    public class StarBurn : ModItem,IChargeableAccessories
    {
        public override string LocalizationCategory => "Items.Accessories";
        
        // 常量定义
        public const int HealthLossInterval = 4; // 每4帧损失1点血量
        public const float ShieldConversionRate = 1f; // 1:1转换比率
        public const float ShieldCapPercentage = 0.5f; // 临时盾牌上限为最大生命值的50%
        public const float LowHealthThreshold = 0.1f; // 10%血量阈值

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var starBurnPlayer = player.GetModPlayer<StarBurnPlayer>();
            starBurnPlayer.hasStarBurn = true;
            ExpansionKeleTool.MultiplyPreDefenseDamageReduction(player,starBurnPlayer.damageReduction);

            // 注册到充能饰品系统
            if (player.whoAmI == Main.myPlayer)
            {
                var chargeablePlayer = player.GetModPlayer<ChargeableAccessoriesPlayer>();
                chargeablePlayer.RegisterAccessory(this);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentSolar, 3)
                .AddIngredient(ItemID.FallenStar,5)
                .AddIngredient(ModContent.ItemType<StarryBar>(), 3)
                .AddIngredient(ItemID.LunarBar, 3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
        public float GetCurrentCharge()
        {
            // 获取本地玩家的StarBurnPlayer实例
            Player player = Main.LocalPlayer;
            if (player.active)
            {
                var starBurnPlayer = player.GetModPlayer<StarBurnPlayer>();
                // 只有当玩家装备了星灼饰品时才返回充能值
                if (starBurnPlayer.hasStarBurn)
                {
                    return starBurnPlayer.temporaryShield;
                }
            }
            return 0f;
        }

        public float GetMaxCharge()
        {
            // 获取本地玩家的最大充能值
            Player player = Main.LocalPlayer;
            if (player.active)
            {
                var starBurnPlayer = player.GetModPlayer<StarBurnPlayer>();
                // 只有当玩家装备了星灼饰品时才返回最大充能值
                if (starBurnPlayer.hasStarBurn)
                {
                    return starBurnPlayer.maxTemporaryShield;
                }
            }
            return 0f;
        }

        public string GetAccessoryName()
        {
            return Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.StarBurn.DisplayName");
        }
    }

    public class StarBurnPlayer : ModPlayer
    {
        public bool hasStarBurn = false;
        public float temporaryShield = 0f;
        public int healthLossTimer = 0;
        public float maxTemporaryShield = 0f;
        public bool _shouldDodge=false;
        public float damageReduction=0f;

        public override void ResetEffects()
        {
            hasStarBurn = false;
        }

        public override void PreUpdate()
        {
            if (!hasStarBurn)
                return;

            // 计算临时盾牌上限
            maxTemporaryShield = Player.statLifeMax2 * StarBurn.ShieldCapPercentage;

            // 处理血量损失和盾牌转化
            HandleHealthDrainAndShieldConversion();

            // 应用动态减伤效果
            ApplyDynamicDamageReduction();
        }

        private void HandleHealthDrainAndShieldConversion()
        {
            // 检查停止条件：盾牌已满 或 血量低于10%
            float currentHealthPercentage = (float)Player.statLife / Player.statLifeMax2;
            if (temporaryShield >= maxTemporaryShield || currentHealthPercentage <= StarBurn.LowHealthThreshold)
            {
                healthLossTimer = 0; // 重置计时器
                return;
            }
            else{

            // 增加计时器
            healthLossTimer++;

            // 每4帧损失1点血量
                if (healthLossTimer >= StarBurn.HealthLossInterval)
                {
                    // 损失1点血量
                    Player.statLife -= 1;
                    
                    // 将损失的血量转化为临时盾牌
                    temporaryShield += 1 * StarBurn.ShieldConversionRate;
                    
                    // 确保不超过上限
                    if (temporaryShield > maxTemporaryShield)
                    {
                        temporaryShield = maxTemporaryShield;
                    }

                    // 重置计时器
                    healthLossTimer = 0;
                }
            }
        }

        private void ApplyDynamicDamageReduction()
        {
            // 计算盾牌占比 x%
            float shieldRatio = temporaryShield / maxTemporaryShield;
            
            // 计算血量占比 y%
            float healthRatio = (float)Player.statLife / Player.statLifeMax2;
            
            // 计算自定义减伤公式：(0.95-((1-x)+(1-y))*0.08)*100%
            damageReduction = (0.95f - ((1f - shieldRatio) + (1f - healthRatio)) * 0.08f);
            
            // 确保减伤值在合理范围内
            if (damageReduction < 0f) damageReduction = 0f;
            
            
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            float incomingDamage = npc.damage;

            ConsumeTemporaryShield(ref modifiers,incomingDamage);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            float Projectilemodifier = ECShieldSystem.UnknownMultiplier*Main.GameModeInfo.EnemyDamageMultiplier;
            float incomingDamage = proj.damage*Projectilemodifier;

            ConsumeTemporaryShield(ref modifiers,incomingDamage);
        }

        

        private void ConsumeTemporaryShield(ref Player.HurtModifiers modifiers,float incomingDamage)
        {
            // 获取原始伤害
            float originalDamage = modifiers.GetDamage(incomingDamage,Player.statDefense,Player.DefenseEffectiveness.Value);
            
            // 如果临时盾牌足够吸收全部伤害
            if (temporaryShield >= originalDamage)
            {
                // 完全吸收伤害
                temporaryShield -= originalDamage;
                modifiers.FinalDamage*=0;

                _shouldDodge=true;
            }
            else
            {
                // 部分吸收伤害
                temporaryShield = 0f;
                // 修改伤害为剩余部分
                modifiers.FinalDamage.Flat -= temporaryShield;
            }
        }
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!hasStarBurn || temporaryShield <= 0)
            {
                return base.FreeDodge(info);
            }

            
            if (_shouldDodge)
            {
                _shouldDodge = false; // 重置标志
                
                // 给予30帧无敌时间
                Player.immune = true;
                Player.immuneTime =30;
                
                return true; // 完全闪避成功
            }
            else
            {
                
            }
            
            return base.FreeDodge(info); // 无法完全闪避
        }

        public override void PostUpdate()
        {
            // 可以在这里添加视觉效果或其他后处理逻辑
        }
    }
}