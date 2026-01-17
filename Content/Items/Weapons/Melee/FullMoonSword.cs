// ... existing code ...
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 望月长剑 - 近战武器
    /// 可以像回旋镖一样发射弹幕，弹幕造成66%伤害并忽略敌人15点护甲
    /// </summary>
    public class FullMoonSword : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        /// <summary>
        /// 设置物品的静态属性
        /// 允许右键重复使用，并指定不是长矛类武器
        /// </summary>
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
            ItemID.Sets.Spears[Item.type] = false; // 不是长矛类
        }

        /// <summary>
        /// 设置物品的基础属性
        /// 包括伤害、类型、使用时间、稀有度等
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月长剑");
            Item.damage = ExpansionKele.ATKTool(90,125);
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FullMoonProjectile>();
            Item.shootSpeed = 12f;
            Item.shootsEveryUse = true;
            Item.noMelee = true; // 禁用直接接触伤害，使用弹幕代替
        }

        /// <summary>
        /// 自定义射击逻辑
        /// 发射类似回旋镖的弹幕，朝向鼠标位置飞行
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <param name="source">物品使用来源</param>
        /// <param name="position">发射位置</param>
        /// <param name="velocity">初始速度</param>
        /// <param name="type">弹幕类型</param>
        /// <param name="damage">伤害值</param>
        /// <param name="knockback">击退值</param>
        /// <returns>是否允许原版弹幕生成</returns>
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float adjustedItemScale = player.GetAdjustedItemScale(Item); // Get the melee scale of the player and item.
			Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<FullMoonSwordProjectile>(), damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
			NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI); // Sync the changes in multiplayer.
            // 获取鼠标指向的位置作为目标点
            Vector2 targetPosition = Main.MouseWorld;
            
            // 计算从武器位置指向鼠标位置的方向向量
            Vector2 direction = (targetPosition - position).SafeNormalize(Vector2.UnitX);
            Vector2 shootVelocity = direction * Item.shootSpeed;

            // 将目标坐标作为AI参数传入弹幕
            Projectile.NewProjectile(source, position, shootVelocity, type, damage, knockback, player.whoAmI, targetPosition.X, targetPosition.Y);

            return false; // 返回false以阻止原版弹幕生成
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
     
        }
        
        /// <summary>
        /// 添加物品合成配方
        /// 需要12个满月锭在铁砧上合成
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FullMoonBar>(), 12) // 修正此处
                .AddTile(TileID.Anvils)
                .Register();
        }

    }
}