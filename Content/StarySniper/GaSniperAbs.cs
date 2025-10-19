using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using System;
using Terraria.Audio;
using System.Collections.Generic;

namespace ExpansionKele.Content.StarySniper
{
    public abstract class GaSniperAbs : ModItem
    {
       
        // 基础属性
        public virtual int BaseDamage { get; }
        public virtual float KnockBack { get; }
        public virtual float ShootSpeed { get; }
        public virtual int UseTime { get; }
        public virtual int UseAnimationTime => UseTime;
        public virtual int Crit { get; }
        public virtual int Rarity { get; }
        public virtual string ItemName { get; }
        public virtual string introduction { get; }

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
            Item.damage = BaseDamage;
            
            if(ExpansionKele.calamity != null)
            {
                Item.damage = (int)(BaseDamage * 1.45);
            }
            
            Item.autoReuse = true;  
            Item.DamageType = DamageClass.Ranged; 
            Item.knockBack = KnockBack; 
            Item.noMelee = true; 
            
            Item.shootSpeed = ShootSpeed; 
            Item.useAnimation = UseAnimationTime; 
            Item.useTime = UseTime; 
            Item.UseSound = ExpansionKele.SniperSound; 
            Item.useStyle = ItemUseStyleID.Shoot; 
            Item.value = Item.buyPrice(gold: 6); 
            Item.crit = Crit;
            Item.rare = Rarity; 

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
            _focustime = 0;
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
        
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) 
        {  
            if (type == ProjectileID.Bullet) 
            {  
                type = ProjectileID.BulletHighVelocity; // 转换为高速子弹  
            }  
            
            if (player.velocity == Vector2.Zero)
            {
                damage = (int)(damage * 1.2f * (1 + _focusbonus));
            }
            else 
            {
                damage = (int)(damage * (1 + _focusbonus));
            }
        }
        
        public override void UpdateInventory(Player player)
        {
            if(_focustime < 300 && player.HeldItem.type == Item.type)
            {
                _focustime++;
            }
            
            _focusbonus = Math.Min(_focustime / Item.useAnimation - 1, 2);
            
            // 当focusbonus达到最大值2时播放声音
            if (_focustime == 3 * Item.useAnimation||_focustime ==299)
            {
                SoundEngine.PlaySound(SoundID.Item75, player.position);
            }
            
            base.UpdateInventory(player);
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
            // // 添加右键功能说明
            // tooltips.Add(new TooltipLine(Mod, "RightClickAbility", "右键发射带治疗效果的穿甲弹" + 
            //     (ExpansionKele.calamity != null ? "（附加死亡标记）" : "")));
            
            // // 添加专注机制说明
            // tooltips.Add(new TooltipLine(Mod, "FocusAbility", "专注机制：武器在手中不使用时会积累专注值，增加伤害，满级时有提示音效"));
            
            // // 添加辅助瞄准说明
            // tooltips.Add(new TooltipLine(Mod, "LaserAbility", "拥有镭射激光辅助瞄准（可在模组设置中开关）"));
            // tooltips.Add(new TooltipLine(Mod, "introduction", introduction));
        }
        
        public override void AddRecipes()
        {

        }
    }
}