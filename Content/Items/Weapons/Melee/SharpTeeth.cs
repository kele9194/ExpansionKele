using ExpansionKele.Content.Customs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 锋利牙齿 - 近战武器
    /// 对最大生命值小于300的敌人直接击杀
    /// </summary>
    public class SharpTeeth : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            // 设置物品的基本属性
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(18,22); //20,24                // 20点伤害
            Item.DamageType = DamageClass.Melee; // 近战伤害类型
            Item.width = 30;                  // 物品宽度
            Item.height = 30;                 // 物品高度
            Item.useTime = 15;                // 使用时间15
            Item.useAnimation = 15;           // 动画时间15
            Item.useStyle = ItemUseStyleID.Swing; // 挥舞使用方式
            Item.knockBack = 4f;              // 击退力度
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 售价50银币
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);     // 蓝色稀有度
            Item.UseSound = SoundID.Item1;    // 使用声音
            Item.autoReuse = true;            // 自动连击
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 对最大生命值小于300的敌人直接击杀
            if (target.lifeMax < 300 && target.active && !target.friendly && !target.dontTakeDamage)
            {
                target.life = 0;
                target.HitEffect(0, 300.0);
                target.active = false;
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, target.whoAmI, -1);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("ExpansionKele:AnyIronBars",7)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}