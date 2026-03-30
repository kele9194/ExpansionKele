using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Projectiles.SummonProj;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Summon
{
    /// <summary>
    /// GoldenWeaverNeedle - 金锦针
    /// 使用武器召唤永久存在的小金针，不消耗召唤栏位
    /// 根据召唤栏位数量增加伤害，手持时伤害翻倍
    /// 金针召唤后即使放下武器也不会消失
    /// 最多只能存在1根金针，平时悬浮在玩家头顶，遇到敌人会冲刺戳刺攻击
    /// </summary>
    public class GoldenWeaverNeedle : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetStaticDefaults()
        {
            
            // 游戏手柄相关设置
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            
            // 不占用召唤栏位（重要：设置为0）
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 0f;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(62, 76);         // 基础伤害值62，76
            Item.DamageType = DamageClass.Summon;                // 召唤伤害类型
            Item.width = 28;                                     // 物品宽高
            Item.height = 30;
            Item.useTime = 30;                                   // 使用时间
            Item.useAnimation = 30;                              // 动画持续时间
            Item.useStyle = ItemUseStyleID.Swing;                // 使用样式为挥舞
            Item.noMelee = false;                                 // 关闭近战攻击判定
            Item.knockBack = 3f;
            Item.useTurn= true;                                  // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); // 稀有度
            Item.UseSound = SoundID.Item1;                      // 使用音效
            Item.mana = 0;                                      // 消耗魔力值
            Item.autoReuse = true;                               // 自动重用
            Item.buffType = ModContent.BuffType<GoldenWeaverMinionBuff>(); // 对应的Buff
            Item.shoot = ModContent.ProjectileType<GoldenWeaverMinion>(); // 设置要生成的弹幕类型
        }
        
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 在鼠标位置生成，但限制在玩家可达范围内
            position = Main.MouseWorld;
            player.LimitPointToPlayerReachableArea(ref position);
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            
            
            damage *=GetBonusMultiplier(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if(player.HasBuff(ModContent.BuffType<GoldenWeaverMinionBuff>())){
                return false;
            }
            else{

                // 添加持续时间较短的Buff，确保召唤物能够生成
                player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                
                // 直接返回true让游戏处理弹幕生成
                return false;
            }

        }

        public static float GetBonusMultiplier(Player player)
        {
            // 根据召唤栏位数量增加伤害（每个栏位+10%）
            int summonSlotTotal = player.maxMinions;
            // 使用MinionSlotCalculator计算空余召唤栏位
            float summonSlotUnused = MinionSlotCalculator.CalculateAvailableMinionSlots(player);
            float bonusMultiplier = 0.1f + (summonSlotTotal * 0.1f) + summonSlotUnused * 1f;
            if (player.HeldItem.type == ModContent.ItemType<GoldenWeaverNeedle>())
            {
                bonusMultiplier *= 1.5f;
            }
            return bonusMultiplier;
            
            // 手持武器时伤害翻倍
        }

        

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
           
        }

        /// <summary>
        /// 注册合成配方
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBroadsword, 1)
                .AddIngredient(ItemID.Silk, 3)
                .AddIngredient(ItemID.SpectreBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class GoldenWeaverNeedlePlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            // 移除了原有的自动召唤逻辑，现在完全依赖Shoot方法处理
        }

        /// <summary>
        /// 手动移除所有金针的方法（可用于特殊情况下）
        /// </summary>
        public void RemoveAllGoldenNeedles()
        {
            // 查找并移除所有属于该玩家的金针
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && 
                    proj.type == ModContent.ProjectileType<GoldenWeaverMinion>() && 
                    proj.owner == Player.whoAmI)
                {
                    proj.Kill();
                }
            }
        }

        public override void ResetEffects()
        {
            // 可以在这里添加其他重置逻辑
        }
    }
}