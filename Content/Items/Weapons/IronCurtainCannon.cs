using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Terraria.NPC;
using Terraria.DataStructures;
using ExpansionKele.Content.Projectiles;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Weapons
{
    /// <summary>
    /// 铁幕加农炮 - 一种远程武器
    /// 左键发射铁幕粒子炮弹，右键发射铁幕激光
    /// </summary>
    public class IronCurtainCannon : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        // 基础伤害值
        private const int constDamage=143; 
        
        /// <summary>
        /// 设置物品的静态属性
        /// 允许右键重复使用
        /// </summary>
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        /// <summary>
        /// 允许使用物品的副功能（右键）
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <returns>是否允许使用副功能</returns>
        public override bool AltFunctionUse(Player player)
        {
            return true; // 允许右键使用
        }

        /// <summary>
        /// 设置物品的基础属性
        /// 包括伤害、类型、使用时间、稀有度等
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("铁幕加农炮");
            Item.damage = ExpansionKele.ATKTool(default,constDamage);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 32;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IronCurtainCannonProjectile>();
            Item.shootSpeed = 30f;
        }

        /// <summary>
        /// 判断物品是否可以使用
        /// 根据左右键设置不同的使用时间和弹幕类型
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <returns>是否可以使用</returns>
        public override bool CanUseItem(Player player)
        {
            // 右键使用
            if (player.altFunctionUse == 2)
            {
                // 设置较快的使用时间和激光弹幕
                Item.useTime = 35;
                Item.useAnimation =35;
                Item.shoot = ModContent.ProjectileType<IronCurtainCannonLaser>();
            }
            else
            {
                // 左键使用，设置较慢的使用时间和粒子炮弹幕
                Item.useTime = 60;
                Item.useAnimation = 60;
                Item.shoot = ModContent.ProjectileType<IronCurtainCannonProjectile>();
            }
            return base.CanUseItem(player);
        }
        
        /// <summary>
        /// 修改物品的提示信息
        /// 添加关于左右键功能的说明
        /// </summary>
        /// <param name="tooltips">提示信息列表</param>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // var tooltipData = new Dictionary<string, string>
            // {
            //     {"Introduction", Language.GetText("Mods.ExpansionKele.Items.IronCurtainCannon.Introduction").Value}
            // };

            // foreach (var kvp in tooltipData)
            // {
            //     tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            // }
        }

        /// <summary>
        /// 添加物品合成配方
        /// 需要光之魂、陨石锭在铁砧上合成
        /// </summary>
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<IronCurtainCannon>());
            recipe.AddIngredient(ItemID.SoulofLight, 5);
            recipe.AddIngredient(ItemID.MeteoriteBar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}