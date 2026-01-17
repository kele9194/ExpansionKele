using System;
using System.Threading;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;
using ExpansionKele.Global;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class SpectreSword : ModItem, IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.damage = ExpansionKele.ATKTool(140, 160);
            Item.knockBack = 6f;
            Item.width = 40;
            Item.height = 40;
            Item.scale = 1f;
            Item.UseSound = SoundID.Item1;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.ProjectileType<SpectreSwordProjectile>();
            Item.shootsEveryUse = true;
            Item.autoReuse = true;
            Item.noMelee = true; // Disable direct contact damage since we'll use projectile
        }
        
// ... existing code ...
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 获取蓄力玩家组件并应用蓄力效果
            SpectreSwordPlayer swordPlayer = player.GetModPlayer<SpectreSwordPlayer>();
            float adjustedItemScale;
            int enhancedDamage;
            if(player.altFunctionUse == 2){
                adjustedItemScale = player.GetAdjustedItemScale(Item) * SpectreSwordPlayer.MAX_CHARGE;
                enhancedDamage = (int)(damage * SpectreSwordPlayer.MAX_DAMAGE_CHARGE);
            }
            else{
            // 使用蓄力值增强伤害和大小
                adjustedItemScale = player.GetAdjustedItemScale(Item) * swordPlayer.spectreSwordCharge;
                enhancedDamage = (int)(damage * swordPlayer.spectreSwordDamageCharge);
            }
            
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, enhancedDamage, knockback, player.whoAmI, player.direction * player.gravDir, Item.useAnimation, adjustedItemScale);
            NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);

            // 攻击后重置蓄力
            swordPlayer.ResetCharge();

            return false; // Return false to prevent default projectile spawning
        }
        // ... existing code ...
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 检测是否正在使用技能
            {
                Item.useAnimation = 20*6;
                Item.useTime = 20*6;
                return true; // 禁用技能
            }
            else
            {
                Item.useAnimation = 20;
                Item.useTime = 20;
                return true; // 允许使用武器
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SpectreBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }

        // 实现IChargeableItem接口
        float IChargeableItem.GetCurrentCharge()
        {
            Player player = Main.LocalPlayer;
            SpectreSwordPlayer swordPlayer = player.GetModPlayer<SpectreSwordPlayer>();
            return swordPlayer.swordChargeTime;
        }

        float IChargeableItem.GetMaxCharge()
        {
            Player player = Main.LocalPlayer;
            SpectreSwordPlayer swordPlayer = player.GetModPlayer<SpectreSwordPlayer>();
            return swordPlayer.useTimeThreshold;
        }
    }
    
    
// ... existing code ...

// ... existing code ...
    public class SpectreSwordPlayer : ModPlayer
    {
        // 光谱剑蓄力相关参数
        public float spectreSwordCharge = 1f;           // 当前蓄力倍数（初始为1）
        public float spectreSwordDamageCharge = 1f;     // 当前伤害蓄力倍数（初始为1）
        public bool isSpectreSwordCharging = false;     // 是否正在蓄力状态
        public int swordChargeTime = 0;                 // 剑蓄力时间计数器

        // 最大蓄力值
        public const float MAX_CHARGE = 3.6f;
        public const float MAX_DAMAGE_CHARGE = 3.6f;
        
        // 使用时间阈值 - 经过一定时间达到最大值
        public int useTimeThreshold = 120; // 设定为2秒（60帧/秒 * 2秒）
        
        public override void ResetEffects()
        {
            isSpectreSwordCharging = false;
        }

        public override void PreUpdate()
        {
            // 更新使用时间阈值（仅当持有光谱剑时）
            Item heldItem = Player.HeldItem;
            if (heldItem != null && heldItem.ModItem is SpectreSword)
            {
                useTimeThreshold = UseTimeHelper.GetActualUseTime(Player, heldItem) * 4; // 6个使用时间达到最大值
            }
        }
        
        public override void PostUpdate()
        {
            Item heldItem = Player.HeldItem;
            
            // 检查是否持有光谱剑
            if (heldItem != null && heldItem.ModItem is SpectreSword)
            {
                // 持有光谱剑的情况下：
                // 如果没有在使用武器（没有攻击动画），则增加蓄力
                if (Player.itemAnimation <= 0)
                {
                    // 增加蓄力时间，但不超过阈值
                    swordChargeTime = Math.Min(useTimeThreshold, swordChargeTime + 1);

                    // 根据蓄力时间计算蓄力值，最多达到最大值
                    float chargeProgress = Math.Min(1f, (float)swordChargeTime / useTimeThreshold);
                    spectreSwordCharge = 1f + (MAX_CHARGE - 1f) * chargeProgress;
                    spectreSwordDamageCharge = 1f + (MAX_DAMAGE_CHARGE - 1f) * chargeProgress;
                }
                else
                {
                    // 正在使用武器（有攻击动画），重置蓄力
                    ResetCharge();
                }
            }
            else
            {
                // 没有持有光谱剑，重置蓄力
                ResetCharge();
            }
        }
        
        // 重置蓄力到基础值
        public void ResetCharge()
        {
            spectreSwordCharge = 1f;
            spectreSwordDamageCharge = 1f;
            swordChargeTime = 0;
        }
    }
// ... existing code ...
}