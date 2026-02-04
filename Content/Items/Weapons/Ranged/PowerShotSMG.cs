using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    /// <summary>
    /// 劲射冲锋枪 - 一种远程武器
    /// 随着不断射击，伤害会逐渐提高，最高变为2倍，远程暴击率最高提升20%
    /// 停止射击伤害缓慢降低为原值，切换武器伤害恢复为原值
    /// </summary>
    public class PowerShotSMG : ModItem, IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        // 超载增量
        public const int OVERLOAD_INCREMENT = 2;
        // 最大超载值
        public const int MAX_OVERLOAD = 50;
        // 衰减计时器
        public const int DECAY_TIMER = 10;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }
        /// <summary>
        /// 设置物品的基础属性
        /// 包括伤害、使用时间、稀有度等
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("劲射冲锋枪");
            Item.damage = ExpansionKele.ATKTool(32,40);
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet; // 修改为实际子弹类型
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Bullet;
        }

        /// <summary>
        /// 添加物品合成配方
        /// 需要神圣锭和非法枪械部件在秘银砧上合成
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 12) // 需要12个神圣锭
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddTile(TileID.MythrilAnvil) // 在秘银砧制作
                .Register();
        }
        
        /// <summary>
        /// 修改武器伤害
        /// 根据超载计数器增加伤害和暴击率
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <param name="damage">伤害修饰符</param>
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            var modPlayer = player.GetModPlayer<PowerShotPlayer>();
            int currentOverload = modPlayer?.overloadCounter ?? 0;
            float overloadBonus = 1.0f + 2.0f * currentOverload / 100.0f;
            damage *= overloadBonus;
            player.GetCritChance<RangedDamageClass>() += (int)(currentOverload * 0.4f); // 可选：转换为百分比整数
        }

        /// <summary>
        /// 发射弹幕
        /// 增加超载计数器
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <param name="source">物品使用源信息</param>
        /// <param name="position">发射位置</param>
        /// <param name="velocity">发射速度</param>
        /// <param name="type">弹幕类型</param>
        /// <param name="damage">伤害值</param>
        /// <param name="knockback">击退值</param>
        /// <returns>是否使用默认的发射行为</returns>
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<PowerShotPlayer>();
            if (modPlayer != null)
            {
                IncreaseOverload(modPlayer);
            }
            
            return true;
        }
        
        /// <summary>
        /// 增加超载值
        /// </summary>
        /// <param name="modPlayer">玩家的PowerShotPlayer模组数据</param>
        private void IncreaseOverload(PowerShotPlayer modPlayer)
        {
            modPlayer.overloadCounter += OVERLOAD_INCREMENT;
            modPlayer.overloadCounter = Math.Min(modPlayer.overloadCounter, MAX_OVERLOAD);
        }
        
        /// <summary>
        /// 修改物品的提示信息
        /// 添加关于伤害提升机制的说明
        /// </summary>
        /// <param name="tooltips">提示信息列表</param>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // var tooltipData = new Dictionary<string, string>
            // {
            //     {"MoveSpeedBonus", Language.GetText("Mods.ExpansionKele.Items.PowerShotSMG.MoveSpeedBonus").Value}
            // };

            // foreach (var kvp in tooltipData)
            // {
            //     tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            // }
        }

        /// <summary>
        /// 获取当前充能值
        /// </summary>
        /// <returns>当前超载值</returns>
        public float GetCurrentCharge()
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<PowerShotPlayer>();
            return modPlayer?.overloadCounter ?? 0;
        }

        /// <summary>
        /// 获取最大充能值
        /// </summary>
        /// <returns>最大超载值</returns>
        public float GetMaxCharge()
        {
            return MAX_OVERLOAD;
        }
    }

    /// <summary>
    /// 劲射冲锋枪玩家数据类
    /// 管理超载计数器和相关逻辑
    /// </summary>
    public class PowerShotPlayer : ModPlayer
    {
        // 超载计数器
        public int overloadCounter;
        // 计时器
        private int timer;
        
        /// <summary>
        /// 玩家更新后的处理
        /// 处理超载衰减和应用加成
        /// </summary>
        public override void PostUpdate()
        {
            HandleOverloadDecay();
            ApplyOverloadBonus();
        }
        
        /// <summary>
        /// 处理超载衰减
        /// 当玩家停止使用武器时逐渐减少超载值
        /// </summary>
        private void HandleOverloadDecay()
        {
            if (Player.HeldItem.type != ModContent.ItemType<PowerShotSMG>())
            {
                overloadCounter=0;
                return;
            }
            
            if (++timer >= PowerShotSMG.DECAY_TIMER)
            {
                DecrementOverload();
                timer = 0;
            }
        }
        
        /// <summary>
        /// 减少超载值
        /// </summary>
        private void DecrementOverload()
        {
            if (overloadCounter > 0)
            {
                overloadCounter--;
            }
        }
        
        /// <summary>
        /// 应用超载加成
        /// </summary>
        private void ApplyOverloadBonus()
        {
            if (Player.HeldItem.type == ModContent.ItemType<PowerShotSMG>())
            {
                
            }
        }
    }
}