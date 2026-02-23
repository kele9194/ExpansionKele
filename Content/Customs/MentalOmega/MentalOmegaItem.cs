using ExpansionKele.Content.Customs;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items
{
    public abstract class MentalOmegaItem : ModItem
    {
        // 定义物品的类别枚举
        public enum ItemCategory
        {
            Infantry,   // 步兵
            Armor,      // 装甲
            AirForce    // 空军
        }

        // 定义物品的科技等级枚举（原Tier）
        public enum ItemTechnology
        {
            T0,
            T1,
            T1_5,
            T2,
            T3,
            T4,
            T5
        }

        // ... existing code ...
        // 可在SetOmegaDefaults中设置的属性
        // ... existing code ...
        // 可在SetOmegaDefaults中设置的属性
        public ItemCategory Category { get; private set; }
        public ItemTechnology Technology { get; private set; }
        public bool isAirForce { get; private set; }

        

        // 用于在SetOmegaDefaults中设置属性的方法
        protected void SetCategory(ItemCategory category) { 
            Category = category;
            if (category == ItemCategory.AirForce)
                isAirForce = true;
        }
        protected void SetTechnology(ItemTechnology technology) => Technology = technology;
// ... existing code ...
// ... existing code ...

        public override void SetDefaults()
        {
            // 调用子类实现的抽象方法来设置Omega特定属性
            SetOmegaDefaults();
            Item.DamageType = ModContent.GetInstance<MentalOmegaDamageClass>();
            base.SetDefaults();
        }

        // 抽象方法，强制子类必须实现此方法来设置Omega特定属性
        protected abstract void SetOmegaDefaults();
        public void AirForceBonus(Player player)
{
    

    // 先尝试转换类型
    if (player.HeldItem.ModItem is MentalOmegaItem heldOmegaItem)
    {
        // 检查是否为空军类别
        if ((int)heldOmegaItem.Category == (int)ItemCategory.AirForce)
        {
            
            Main.NewText($"{player.wingTime},{player.wingTimeMax}");
            if ( player.wingTime < 10f)
        {
            player.wingTime =10;
        }

            // 如果玩家不在地面，应用减伤和增伤
            if (player.velocity.Y == 0)
            {
                ExpansionKeleTool.AddDamageReduction(player, -0.4f);
                ExpansionKeleTool.AddDamageBonus(player, -0.4f);
            }
        }
        else
        {
            // 此时 heldOmegaItem 已确定初始化，可以安全访问
            //Main.NewText($"{(int)heldOmegaItem.Category == (int)ItemCategory.AirForce}");
        }
    }
}
        }
    }
