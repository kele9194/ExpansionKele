using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class IronWillShield : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
            Item.defense = ExpansionKele.DEFTool(0,2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 免疫击退效果
            player.noKnockback = true;
            
            // 获取玩家的自定义减伤组件
            var modPlayer = player.GetModPlayer<IronWillShieldPlayer>();
            
            // 更新玩家的减伤乘数
            modPlayer.UpdateIronWillShield();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("ExpansionKele:AnyIronBars", 5)
                .AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 5)
                .AddTile(TileID.Anvils) // 在工作台制作
                .Register();
        }
    }

    public class IronWillShieldPlayer : ModPlayer
    {
        // 计时器，记录未受伤的时间（ticks）
        public int noHitTimer = 0;
        
        // 最大计时器值，对应40秒（60 ticks = 1秒）
        private static int MaxTimer = 40 * 60;
        
        // 开始获得减伤的时间（5秒后）
        private static int DamageReductionStartTime = 5 * 60;
        
        // 最大减伤乘数（70%减伤 = 0.3的乘数）
        private static float MaxDamageReduction = 0.3f;
        public void SetTimers(int maxtimer,int damageReductionStartTime,float maxDamageReduction){
            MaxTimer = maxtimer;
            DamageReductionStartTime = damageReductionStartTime;
            MaxDamageReduction = maxDamageReduction;
        }
        
        // 是否正在穿戴钢铁意志之盾
        private bool isWearingShield = false;

        public override void ResetEffects()
        {
            // 每帧重置效果
            isWearingShield = false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            // 受伤时重置计时器
            noHitTimer = 0;
        }

        public override void PostUpdate()
        {
            // 只有当穿戴了盾牌时才增加计时器
            if (isWearingShield)
            {
                // 增加未受伤计时器
                if (noHitTimer < MaxTimer)
                {
                    noHitTimer++;
                }
            }
        }

        public void UpdateIronWillShield()
        {
            // 标记为正在穿戴盾牌
            isWearingShield = true;
            
            // 只有当未受伤时间超过5秒时才开始应用减伤
            if (noHitTimer >= DamageReductionStartTime)
            {
                var reductionPlayer = Player.GetModPlayer<CustomDamageReductionPlayer>();
                
                // 根据计时器计算减伤乘数
                // 从1.0（无减伤）逐渐减少到0.3（70%减伤）
                float timeSinceReductionStarted = noHitTimer - DamageReductionStartTime;
                float maxReductionTime = MaxTimer - DamageReductionStartTime;
                
                // 线性减少乘数
                float damageMultiplier = 1.0f;
                if (maxReductionTime > 0)
                {
                    float progress = timeSinceReductionStarted / maxReductionTime;
                    if (progress > 1f) progress = 1f;
                    damageMultiplier = 1.0f - progress * (1.0f - MaxDamageReduction);
                }
                
                // 应用减伤乘数
                reductionPlayer.MulticustomDamageReduction(damageMultiplier);
            }
        }
    }
}