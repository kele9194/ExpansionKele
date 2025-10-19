using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Weapons
{
    /// <summary>
    /// 质子加农炮 - 一种远程蓄力武器
    /// 具有充能和射击音效，使用特殊的持握弹幕
    /// </summary>
    public class ProtonCannon : ModItem
    {
        
        /// <summary>
        /// 充能音效
        /// </summary>
        public static readonly SoundStyle Charge = new SoundStyle("ExpansionKele/Content/Audio/Charge"); // 使用内置声音效果
        
        /// <summary>
        /// 射击音效
        /// </summary>
        public static readonly SoundStyle Fire = SoundID.Item12; // 使用内置声音效果

        /// <summary>
        /// 射击后冷却帧数
        /// </summary>
        public static int AftershotCooldownFrames = 12;
        
        /// <summary>
        /// 完全充能所需帧数
        /// </summary>
        public static int FullChargeFrames = 88;
        
        /// <summary>
        /// 物品本地化类别
        /// </summary>
        public override string LocalizationCategory => "Items.Weapons";

        /// <summary>
        /// 设置物品的静态属性
        /// 标记为远程专家武器
        /// </summary>
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
            //Item.SetNameOverride("质子加农炮");
            Item.width = 48;
            Item.height = 24;
            Item.damage = ExpansionKele.ATKTool(60,70);
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = (Item.useAnimation = AftershotCooldownFrames);
            Item.useStyle = ItemUseStyleID.Shoot; // 持握使用风格
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.channel = true; // 启用通道模式
            Item.shoot = ModContent.ProjectileType<ProtonCannonHoldOut>();
            Item.shootSpeed = 12f;
            Item.crit=5;
        }

        /// <summary>
        /// 判断物品是否可以使用
        /// 确保玩家最多只能持有一个该武器的弹幕
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <returns>是否可以使用</returns>
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        /// <summary>
        /// 持握物品时的处理
        /// </summary>
        /// <param name="player">持握物品的玩家</param>
        public override void HoldItem(Player player)
        {
            //player.mouseInterface = true;
        }
        
        /// <summary>
        /// 添加物品合成配方
        /// 需要副金属锭和力量魂在秘银砧上合成
        /// </summary>
        public override void AddRecipes()  
	{  
            // 创建质子加农炮的合成配方  
            Recipe recipe = Recipe.Create(ModContent.ItemType<ProtonCannon>()); // 质子加农炮类型  
            recipe.AddRecipeGroup("ExpansionKele:SecondaryBars", 5); // 添加任意副金属锭组，要求5个 
            recipe.AddIngredient(ItemID.SoulofMight, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register(); // 注册配方  
	}  
    }
}