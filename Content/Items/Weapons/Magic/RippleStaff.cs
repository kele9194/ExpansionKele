using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MagicProj;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    
    public class RippleStaff : ModItem
    {
        public override string LocalizationCategory=>"Items.Weapons";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("荡漾水杖");
            // Tooltip.SetDefault("发射一枚水弹");
            Item.staff[Type] = true; // 使物品被视为法杖，允许使用法杖动画
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(28,32); // 伤害30
            Item.DamageType = DamageClass.Magic; // 魔法伤害
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 25; // 使用时间25
            Item.useAnimation = 25; // 使用动画25
            Item.useStyle = ItemUseStyleID.Shoot; // 射击样式
            Item.knockBack = 4;
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 50银币
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);  // 蓝色稀有度
            Item.UseSound = SoundID.Item21; // 使用音效
            Item.autoReuse = true; // 自动连发
            Item.shoot = ModContent.ProjectileType<RippleProjectile>(); // 发射水弹
            Item.shootSpeed = 8f; // 射弹速度
            Item.mana = 8; // 消耗8点法力值
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Sapphire, 5) // 蓝玉5个
                .AddIngredient(ItemID.GoldBar, 5) // 金锭5个（也可以用铂金锭）
                .AddIngredient(ItemID.ManaCrystal) // 魔力水晶1个
                .AddTile(TileID.Sinks) // 在水槽处合成
                .Register();

            // 铂金版本的合成配方
            CreateRecipe()
                .AddIngredient(ItemID.Sapphire, 5) // 蓝玉5个
                .AddIngredient(ItemID.PlatinumBar, 5) // 铂金锭5个
                .AddIngredient(ItemID.ManaCrystal) // 魔力水晶1个
                .AddTile(TileID.Sinks) // 在水槽处合成
                .Register();
        }
    }
}