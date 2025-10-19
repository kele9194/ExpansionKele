using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using ExpansionKele.Content.Buff;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Weapons
{
    /// <summary>
    /// 中子枪 - 一种特殊武器，发射中子光束对敌人造成基于最大生命值的持续伤害
    /// </summary>
    public class NeutronGun : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        /// <summary>
        /// 设置物品的基本属性
        /// </summary>
        public override void SetDefaults()
        {
            
            // 设置物品显示名称（将由本地化系统覆盖）
            //Item.SetNameOverride("中子枪");
            // 使用ExpansionKele工具计算伤害值，这里设置为1点伤害
            Item.damage = ExpansionKele.ATKTool(1,1);
            // 使用时间，单位为tick（1/60秒）
            Item.useTime = 3;
            // 动画时间，单位为tick（1/60秒）
            Item.useAnimation = 3;
            // 使用方式为射击
            Item.useStyle = ItemUseStyleID.Shoot;
            // 不进行近战攻击
            Item.noMelee = true;
            // 击退力为0
            Item.knockBack = 0;
            // 物品价值为1金币
            Item.value = Item.buyPrice(gold: 1);
            // 稀有度为红色（最高稀有度之一）
            Item.rare = ItemRarityID.Red;
            // 使用音效为Item11
            Item.UseSound = SoundID.Item11;
            // 不自动重复使用
            Item.autoReuse = false;
            // 发射的弹幕类型为中子弹幕
            Item.shoot = ModContent.ProjectileType<NeutronProjectile>();
            // 弹幕速度为30f
            Item.shootSpeed = 30f;
        }

        /// <summary>
        /// 发射弹幕的逻辑
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
            // 创建一个新的弹幕
            Terraria.Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            // 返回false以防止默认射击行为
            return false;
        }
        
        /// <summary>
        /// 修改物品提示信息，添加自定义说明
        /// </summary>
        /// <param name="tooltips">提示信息列表</param>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // var tooltipData = new Dictionary<string, string>
            // {
            //     // 从语言文件中获取介绍文本
            //     {"Introduction", Language.GetText("Mods.ExpansionKele.Items.NeutronGun.Introduction").Format(NeutronDecay.ReturnDEcay())}
            // };

            // foreach (var kvp in tooltipData)
            // {
            //     tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            // }
        }
        
        /// <summary>
        /// 添加物品合成配方
        /// </summary>
        public override void AddRecipes()  
	{  
            // 创建合成配方
            Recipe recipe = Recipe.Create(ModContent.ItemType<NeutronGun>());
            // 添加材料：陨石锭5个
            recipe.AddIngredient(ItemID.MeteoriteBar, 5);
            // 添加材料：骨头5个
            recipe.AddIngredient(ItemID.Bone, 5);
            // 合成站：铁砧或铅砧
            recipe.AddTile(TileID.Anvils);
            // 注册配方
            recipe.Register();
	}  
    }
}