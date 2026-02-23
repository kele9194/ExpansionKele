using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using System;
using Terraria.Audio;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    /// <summary>
    /// 弩类武器的抽象基类，包含弩类武器的通用逻辑
    /// </summary>
    public abstract class BaseCrossbow : ModItem, IChargeableItem
    {
        // 蓄力相关变量
        protected int chargeLevel = 0;
        protected const int MaxChargeLevel = 3;
        protected const int ChargeTime = 30; // 蓄力时间改为30帧
        
        // 自动蓄力计时器
        private int chargeTimer = 0;
        
        // 用于存储当前的使用速度乘数
        private float _useSpeedMultiplier = 1f;
        // 用于存储当前的音效
        private SoundStyle? _customUseSound = null;

        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            // 移除右键点击设置，因为改为自动蓄力
            // ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        // 删除CanConsumeAmmo方法，因为不再需要判断右键状态
        /*
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            // 右键蓄力时不消耗弹药
            if (player.altFunctionUse == 2)
            {
                return false;
            }
            return base.CanConsumeAmmo(ammo, player);
        }
        */

        public override void SetDefaults()
        {
            // 设置基础属性
            Item.damage = GetBaseDamage();
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 24;
            Item.useTime = 30; // 使用时间改为30帧
            Item.useAnimation = 30; // 动画时间改为30帧
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = GetKnockback();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;

            // 弹药设置
            Item.shoot = ProjectileID.FireArrow;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Arrow;
        }

        // 删除AltFunctionUse方法，因为不再需要右键功能
        /*
        public override bool AltFunctionUse(Player player)
        {
            // 允许右键使用
            return true;
        }
        */

        // 使用UseSpeedMultiplier来控制蓄力和射击节奏
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {   
            if (chargeLevel < MaxChargeLevel)
            {
                // 蓄力期间不发射弹幕
                return false;
            }
            else return base.CanConsumeAmmo(ammo, player);
 
        }
        public override float UseSpeedMultiplier(Player player)
        {
            // 自动蓄力逻辑：如果未满级则继续蓄力，满级则可以射击
            if (chargeLevel < MaxChargeLevel)
            {
                return 1f; // 蓄力期间保持正常速度
            }
            else
            {
                return 1f; // 满级后可以射击
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 根据蓄力层数计算伤害倍数
            if (chargeLevel > 0)
            {
                float multiplier = (float)Math.Pow(chargeLevel, 1.5);
                damage = (int)(damage * multiplier);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 自动蓄力逻辑
            if (chargeLevel < MaxChargeLevel)
            {
                // 蓄力期间不发射弹幕
                return false;
            }
            else
            {
                // 达到最大蓄力等级，发射弹幕
                // 发射修改后的箭矢并增加额外更新速度
                int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                
                // 通过索引访问弹幕并修改属性
                if (Main.projectile.IndexInRange(projIndex))
                {
                    Projectile proj = Main.projectile[projIndex];
                    proj.extraUpdates += chargeLevel * 3;
                    proj.ArmorPenetration += chargeLevel * chargeLevel * 4;

                    // 获取弹药物品并计算经过加成的伤害
                    Item ammoItem = new Item(source.AmmoItemIdUsed);
                    int extraDamage = ammoItem.damage;
                    proj.damage += extraDamage;
                }

                // 射击后重置蓄力
                chargeLevel = 0;
                chargeTimer = 0;
                
                // 射击完成后恢复默认音效
                _customUseSound = SoundID.Item5;
                return false;
            }
        }

        // 添加更新方法处理自动蓄力
        public override void UpdateInventory(Player player)
        {
            // 自动蓄力计时逻辑
            if (chargeLevel < MaxChargeLevel && player.itemAnimation == 0)
            {
                chargeTimer++;
                
                // 每30帧增加一级蓄力（1秒）
                if (chargeTimer >= ChargeTime)
                {
                    chargeLevel++;
                    chargeTimer = 0;
                    
                    // 显示蓄力进度
                    if (chargeLevel <= MaxChargeLevel)
                    {
                        CombatText.NewText(player.getRect(), Color.Yellow, $"Level:{chargeLevel}");
                    }
                }
            }
            
            // 如果玩家停止使用武器，重置蓄力
            if (player.itemAnimation == 0 && chargeLevel > 0 && chargeLevel < MaxChargeLevel)
            {
                // 可以选择是否在此处重置蓄力
                // 目前设计为保持蓄力直到满级或发射
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }

        public float GetCurrentCharge()
        {
            return chargeLevel;
        }

        public float GetMaxCharge()
        {
            return MaxChargeLevel;
        }

        // 抽象方法，子类必须实现
        protected abstract int GetBaseDamage();
        protected abstract float GetKnockback();
        public abstract override void AddRecipes();
    }
}