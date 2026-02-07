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
        protected const int ChargeTime = 30;
        
        // 用于存储当前的使用速度乘数
        private float _useSpeedMultiplier = 1f;
        // 用于存储当前的音效
        private SoundStyle? _customUseSound = null;

        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            // 允许重复右键点击
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            // 右键蓄力时不消耗弹药
            if (player.altFunctionUse == 2)
            {
                return false;
            }
            return base.CanConsumeAmmo(ammo, player);
        }

        public override void SetDefaults()
        {
            // 设置基础属性
            Item.damage = GetBaseDamage();
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 24;
            Item.useTime = 5;
            Item.useAnimation = 5;
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

        public override bool AltFunctionUse(Player player)
        {
            // 允许右键使用
            return true;
        }

        // public override bool CanUseItem(Player player)
        // {
        //     // 如果是右键使用，则改变为蓄力模式
        //     if (player.altFunctionUse == 2)
        //     {
        //         if(chargeLevel==MaxChargeLevel){
        //             return false; // 达到最大层数后不能再蓄力
        //         }
                
        //     }
        //     else
        //     {
        //         if(chargeLevel == 0){
        //             return false;
        //         }
        //     }
        //     return true;
        // }

        // 使用UseSpeedMultiplier来修改使用时间，这样在多人游戏中能正确同步
        public override float UseSpeedMultiplier(Player player){
            if (player.altFunctionUse == 2)
            {
                return 5f/30f;
            }
            else {return 1f;}

        }
        // 通过这个方法来修改使用音效，避免直接修改Item.UseSound导致的同步问题


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
            if (player.altFunctionUse == 2&&chargeLevel < MaxChargeLevel)
            {
                chargeLevel++;
                CombatText.NewText(player.getRect(), Color.Cyan, $"Level: {chargeLevel}");
                return false;
            }
            else if (player.altFunctionUse == 2&&chargeLevel == MaxChargeLevel)
            {
                return false;
            }
            else if(chargeLevel == 0){
                return false;
            }
            else
            {
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

                chargeLevel = 0;
                // 射击完成后恢复默认音效
                _customUseSound = SoundID.Item5;
                return false;
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