using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Localization;
using ExpansionKele.Content.Items.Weapons.Ranged;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.RangedProj;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    /// <summary>
    /// 星璇导弹发射器 - 一种高级远程武器
    /// 发射一枚直射弹和两枚追踪导弹，追踪导弹对空中目标造成巨额额外伤害
    /// </summary>
    public class VortexMissileLauncher : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }
        public override string LocalizationCategory => "Items.Weapons";
        /// <summary>
        /// 设置物品的基础属性
        /// 包括伤害、使用时间、稀有度等
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("星璇导弹发射器");
            Item.damage = ExpansionKele.ATKTool(320,380);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.MoonlordBullet; // 使用自定义主导弹
            Item.shootSpeed = 16f;
        }
        
        /// <summary>
        /// 发射弹幕
        /// 发射一枚直射弹和两枚追踪导弹
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
            // 发射直射导弹（使用自定义主导弹）
            Projectile.NewProjectile(source, position, velocity, ProjectileID.MoonlordBullet, damage, knockback, player.whoAmI);

            // 计算两个偏移角度（正负12.5度）
            float angleOffset = MathHelper.ToRadians(12.5f);
            
            // 发射第一个追踪导弹（+12.5度）
            Vector2 velocity1 = velocity.RotatedBy(angleOffset);
            Projectile.NewProjectile(source, position, velocity1, ModContent.ProjectileType<VortexHomingProjectile>(), damage, knockback, player.whoAmI);

            // 发射第二个追踪导弹（-12.5度）
            Vector2 velocity2 = velocity.RotatedBy(-angleOffset);
            Projectile.NewProjectile(source, position, velocity2, ModContent.ProjectileType<VortexHomingProjectile>(), damage, knockback, player.whoAmI);

            return false; // 返回false以防止默认射击行为
        }
        
        /// <summary>
        /// 设置持握物品时的偏移量
        /// </summary>
        /// <returns>偏移量向量</returns>
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6, 0);
        }

        /// <summary>
        /// 修改物品的提示信息
        /// 添加关于武器特殊功能的说明
        /// </summary>
        /// <param name="tooltips">提示信息列表</param>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // tooltips.Add(new TooltipLine(Mod, "Introduction", Language.GetText("Mods.ExpansionKele.Items.VortexMissileLauncher.Introduction").Value));
        }

        /// <summary>
        /// 添加物品合成配方
        /// 需要月球锭、星璇碎片和防空导弹发射器在月亮祭坛上合成
        /// </summary>
        public override void AddRecipes()
        {
            Recipe.Create(ModContent.ItemType<VortexMissileLauncher>())
                .AddIngredient(ItemID.LunarBar, 2)
                .AddIngredient(ItemID.FragmentVortex, 4)
                .AddIngredient(ModContent.ItemType<AAMissileLauncher>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}