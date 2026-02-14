using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using System;
using Terraria.Audio;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.RangedProj;

namespace ExpansionKele.Content.StarySniper
{
    public abstract class GaSniperAbs : ModItem,IChargeableItem
    {
       
        // 基础属性
        public virtual int BaseDamage { get; }
        public virtual float KnockBack { get; }
        public virtual float ShootSpeed { get; }
        public virtual int UseTime { get; }
        public virtual int UseAnimationTime => UseTime;
        public virtual int Crit { get; }
        public virtual int Rarity { get; }
        public virtual int Width => 80;
        public virtual int Height => 31;
        public virtual string ItemName { get; }
        public virtual string introduction { get; }
        public virtual Vector2 HoldoutOffsetValue => new Vector2(-17f, -2f);

        // 右键增强属性
        public virtual float RightClickDamageMultiplier => 2.5f;
        public virtual float RightClickKnockBackBonus => 3f;
        public virtual float RightClickSpeedMultiplier => 2f;
        public virtual float RightClickUseTimeMultiplier => 2f;

        public override void SetDefaults()
        {
            //Item.SetNameOverride(ItemName);
            Item.width = 80;
            Item.height = 31;
            Item.damage = Item.damage = Item.damage = ExpansionKele.ATKTool(BaseDamage,default);
            
            Item.autoReuse = true;  
            Item.DamageType = DamageClass.Ranged; 
            Item.knockBack = KnockBack; 
            Item.noMelee = true; 
            
            Item.shootSpeed = ShootSpeed; 
            Item.useAnimation = UseAnimationTime; 
            Item.useTime = UseTime; 
            Item.UseSound = ExpansionKele.SniperSound; 
            Item.useStyle = ItemUseStyleID.Shoot; 
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.crit = Crit;

            Item.shoot = ProjectileID.Bullet;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true; // 允许右键使用
        }
        
        public override bool CanUseItem(Player player) 
        {
            // 右键使用逻辑
            if (player.altFunctionUse == 2) 
            {
                Item.damage = (int)(BaseDamage * RightClickDamageMultiplier);
                Item.useTime = (int)(UseTime * RightClickUseTimeMultiplier);
                Item.knockBack = KnockBack + RightClickKnockBackBonus;
                Item.shootSpeed = ShootSpeed * RightClickSpeedMultiplier;
                Item.useAnimation = (int)(UseTime * RightClickUseTimeMultiplier);
            }
            else
            {
                Item.damage = BaseDamage;
                Item.useTime = UseTime;
                Item.knockBack = KnockBack;
                Item.shootSpeed = ShootSpeed;
                Item.useAnimation = UseTime;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 重置专注时间，因为射击会中断专注
            var focusPlayer = player.GetModPlayer<FocusSniperPlayer>();
            focusPlayer.focusTime = 0;
            
            // 使用 Projectile.NewProjectile 方法创建新的弹丸
            int projectileType = player.altFunctionUse == 2 ? GetRightClickProjectile() : type;
            // 使用 Projectile.NewProjectile 方法创建新的弹丸
            Terraria.Projectile.NewProjectile(source, position, velocity, projectileType, damage, knockback, player.whoAmI);
            return false; // 返回 false 以防止默认发射行为
        }

        // 可重写的方法，用于定义右键发射的弹丸类型
        public virtual int GetRightClickProjectile()
        {
            return ModContent.ProjectileType<SharkyBullet>();
        }

        // 此方法可以调整武器在玩家手中的位置。调整这些值直到与图形效果匹配。  
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-17f, -2f); // 持有偏移量。  
        }

        private float _focustime;
        private float _focusbonus;
        public float actualUseTime;
        
         public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) 
        {  
            if (type == ProjectileID.Bullet) 
            {  
                type = ProjectileID.BulletHighVelocity; // 转换为高速子弹  
            }  

            // 使用FocusSniperPlayer提供的专注加成
            var focusPlayer = player.GetModPlayer<FocusSniperPlayer>();
            float focusDamageBonus = focusPlayer.focusBonus;
            
            if (player.velocity == Vector2.Zero)
            {
                damage = (int)(damage * 1.2f * (1 + focusDamageBonus)); // 静止时额外1.2倍伤害
            }
            else 
            {
                damage = (int)(damage * (1 + focusDamageBonus)); // 移动时仅专注加成
            }
        }
        
        public override void UpdateInventory(Player player)
        {
            // actualUseTime = UseTimeHelper.GetActualUseTime(player, Item);
            // if(_focustime < 3*actualUseTime && player.HeldItem.type == Item.type)
            // {
            //     _focustime++;
            // }
            
            
            // _focusbonus = Math.Min(_focustime / actualUseTime-1, 2); // 将专注值的上限设置为3倍实际使用时间
            

            
            // base.UpdateInventory(player);
        }
        
        // 添加获取当前焦点加成的方法，供派生类使用
        public float GetFocusBonus()
        {
            return _focusbonus;
        }
        
        // 添加获取当前充能时间的方法，供派生类使用
        public float GetFocusTime()
        {
            return _focustime;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
        }
        
        public override void AddRecipes()
        {

        }

        float IChargeableItem.GetCurrentCharge()
        {
            // 在客户端环境中获取当前玩家的专注时间
            if (Main.netMode != NetmodeID.Server && Main.LocalPlayer.active)
            {
                var focusPlayer = Main.LocalPlayer.GetModPlayer<FocusSniperPlayer>();
                // 检查当前手持的是否是本狙击枪
                if (Main.LocalPlayer.HeldItem.ModItem == this)
                {
                    return focusPlayer.focusTime;
                }
            }
            return 0f;
        }

        float IChargeableItem.GetMaxCharge()
        {
            // 在客户端环境中获取当前玩家的实际使用时间
            if (Main.netMode != NetmodeID.Server && Main.LocalPlayer.active)
            {
                var focusPlayer = Main.LocalPlayer.GetModPlayer<FocusSniperPlayer>();
                // 检查当前手持的是否是本狙击枪
                if (Main.LocalPlayer.HeldItem.ModItem == this)
                {
                    return 3 * focusPlayer.actualUseTime;
                }
            }
            return 0;
        }
        
    }
    public class FocusSniperPlayer : ModPlayer
    {
        // 当前专注时间
        public float focusTime = 0;
        // 专注加成值
        public float focusBonus = 0;
        // 当前持有时长武器
        public bool holdingSniper = false;
        // 实际使用时间
        public int actualUseTime = 0;

        public override void ResetEffects()
        {
            holdingSniper = false;
        }

        public override void PostUpdate()
        {
            // 获取当前手持的狙击枪
            if (Player.HeldItem.ModItem is GaSniperAbs sniperItem)
            {
                holdingSniper = true;
                actualUseTime = UseTimeHelper.GetActualUseTime(Player, Player.HeldItem);
                    focusTime = Math.Min(focusTime + 1, 3 * actualUseTime);

                // 计算专注加成
                focusBonus = Math.Max(0, focusTime / actualUseTime - 1);
                focusBonus = Math.Min(focusBonus, 2f); // 最大200%伤害加成
            }
            else
            {
                // 不持有狙击枪时重置专注
                holdingSniper = false;
                focusTime = 0;
                focusBonus = 0;
                actualUseTime = 0;
            }
        }

        // 静态方法，用于获取专注加成
        public static float GetFocusBonusForItem(Player player, Item item)
        {
            var focusPlayer = player.GetModPlayer<FocusSniperPlayer>();
            if (player.HeldItem.type == item.type && player.HeldItem.ModItem is GaSniperAbs)
            {
                return focusPlayer.focusBonus;
            }
            return 0;
        }

        // 静态方法，用于获取专注时间
        public static float GetFocusTimeForItem(Player player, Item item)
        {
            var focusPlayer = player.GetModPlayer<FocusSniperPlayer>();
            if (player.HeldItem.type == item.type && player.HeldItem.ModItem is GaSniperAbs)
            {
                return focusPlayer.focusTime;
            }
            return 0;
        }
    }
}