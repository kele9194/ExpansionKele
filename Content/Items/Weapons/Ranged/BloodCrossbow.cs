using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using System;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class BloodCrossbow : ModItem
    {
        // 添加蓄力层数变量
        private int chargeLevel = 0;
        private const int maxChargeLevel = 3;
        private const int chargeTime = 30;

        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            // 允许重复右键点击
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            // 基本属性设置
            Item.damage = ExpansionKele.ATKTool(40, 48); // 基础伤害35（相比WoodenCrossbow提升）
            Item.DamageType = DamageClass.Ranged; // 远程伤害类型
            Item.width = 50; // 物品宽度
            Item.height = 24; // 物品高度
            Item.useTime = 5; // 使用时间改为5（左键快速射击）
            Item.useAnimation = 5; // 动画时间
            Item.useStyle = ItemUseStyleID.Shoot; // 使用样式为射击
            Item.noMelee = true; // 不进行近战攻击
            Item.knockBack = 3f; // 击退（略微增强）
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 价值
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); // 稀有度
            Item.UseSound = SoundID.Item5; // 使用音效
            Item.autoReuse = true; // 自动连发
            
            // 弹药相关设置
            Item.shoot = ProjectileID.FireArrow; // 默认发射物
            Item.shootSpeed = 18f; // 发射速度
            Item.useAmmo = AmmoID.Arrow; // 使用箭作为弹药
        }

        public override void HoldItem(Player player)
        {
            player.lifeRegenTime += 1;
        }

        public override bool AltFunctionUse(Player player)
        {
            // 允许右键使用
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            // 如果是右键使用，则改变为蓄力模式
            if (player.altFunctionUse == 2)
            {
                // 右键使用时重置蓄力计时器（如果还未达到最大层数）
                if (chargeLevel < maxChargeLevel)
                {
                    Item.useTime = chargeTime;
                    Item.useAnimation = chargeTime;
                    Item.UseSound = null;
                    return true;
                }
                return false; // 达到最大层数后不能再蓄力
            }
            else
            {
                // 左键使用只有在有蓄力时才能发射
                Item.useTime = 5;
                Item.useAnimation = 5;
                return chargeLevel > 0; // 只有蓄力层数大于0时才能使用
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
            if (player.altFunctionUse == 2)
            {
                chargeLevel++;
                CombatText.NewText(player.getRect(), Color.Cyan, $"Level: {chargeLevel}");
                return false;
            }
            else
            {
                // 发射修改后的箭矢并增加额外更新速度
                Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
                proj.extraUpdates += chargeLevel * 3; // 增加弹幕更新速度
                proj.ArmorPenetration += chargeLevel * chargeLevel * 4;

                // 获取弹药物品并计算经过加成的伤害
                Item ammoItem = new Item(source.AmmoItemIdUsed);
                int extraDamage = ammoItem.damage;
                // if (ammoItem.damage > 0) {
                //     // 使用Player.GetWeaponDamage来获取包含远程伤害加成的弹药伤害
                //     StatModifier ammoDamageModifier = new StatModifier();
                //     ammoDamageModifier.Base = ammoItem.damage;
                //     extraDamage = (int)player.GetDamage(DamageClass.Ranged).ApplyTo(ammoItem.damage);
                // }
                proj.damage += extraDamage;

                chargeLevel = 0;

                return false;
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 6) // 6个猩红锭
                .AddIngredient(ItemID.TissueSample, 6) // 6个组织样本
                .AddTile(TileID.Anvils) // 在铁砧制作
                .Register();
        }
    }
}