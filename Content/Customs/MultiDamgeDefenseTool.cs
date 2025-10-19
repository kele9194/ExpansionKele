using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    public class ExpansionKeleDamageMulti : ModPlayer
    {
        //player.GetModPlayer<ExpansionKelePlayer>().SetMultiplicativeDamageBonus(1.15f)
        /// <summary>
        /// 玩家的乘算伤害加成，1.0表示无加成，1.1表示增加10%伤害
        /// </summary>
        public float MultiplicativeDamageBonus = 1.0f;

        public override void ResetEffects()
        {
            // 每帧重置乘算伤害加成为默认值
            MultiplicativeDamageBonus = 1.0f;
        }

        /// <summary>
        /// 应用所有伤害类型的乘算加成
        /// </summary>
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // 应用乘算伤害加成
            damage *= MultiplicativeDamageBonus;
        }

        /// <summary>
        /// 提供一个方法来设置乘算伤害加成
        /// </summary>
        /// <param name="bonus">伤害倍数，例如1.1表示增加10%伤害</param>
        public void SetMultiplicativeDamageBonus(float bonus)
        {
            MultiplicativeDamageBonus = bonus;
        }

        /// <summary>
        /// 提供一个方法来增加乘算伤害加成
        /// </summary>
        /// <param name="bonus">要增加的伤害倍数</param>
        public void AddMultiplicativeDamageBonus(float bonus)
        {
            MultiplicativeDamageBonus += bonus;
        }

        /// <summary>
        /// 提供一个方法来乘算叠加伤害加成
        /// </summary>
        /// <param name="bonus">要乘算的伤害倍数</param>
        public void MultiplyMultiplicativeDamageBonus(float bonus)
        {
            MultiplicativeDamageBonus *= bonus;
        }
    }
    
    public class CustomDamageReductionPlayer : ModPlayer
    { 
    
     public float customDamageReduction = 0f;
        public bool hasCustomDamageReduction = false;

        // 新增：防御前减伤属性
        public float preDefenseDamageReduction = 0f;
        public bool hasPreDefenseDamageReduction = false;

        public float customDamageReductionMulti = 1f;
        public bool hasCustomDamageReductionMulti = false;
        public float preDefenseDamageReductionMulti = 1f;
        public bool hasPreDefenseDamageReductionMulti = false;


        public override void ResetEffects()
        {
            // 重置自定义减伤效果
            customDamageReduction = 0f;
            hasCustomDamageReduction = false;
            
            // 重置防御前减伤效果
            preDefenseDamageReduction = 0f;
            hasPreDefenseDamageReduction = false;
            customDamageReductionMulti = 1f;
            hasCustomDamageReductionMulti = false;
            preDefenseDamageReductionMulti = 1f;
            hasPreDefenseDamageReductionMulti = false;

        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if(hasPreDefenseDamageReductionMulti){
                modifiers.IncomingDamageMultiplier *= preDefenseDamageReductionMulti;
            }
            // 应用防御前减伤效果
            if (hasPreDefenseDamageReduction && preDefenseDamageReduction != 0f)
            {
                // 使用IncomingDamageMultiplier实现防御前减伤
                modifiers.IncomingDamageMultiplier *= (1f - preDefenseDamageReduction);
            }
            
            // 应用自定义减伤效果
            if (hasCustomDamageReduction && customDamageReduction != 0f)
            {
                modifiers.FinalDamage *= (1f - customDamageReduction);
            }

            if(hasCustomDamageReductionMulti){
                modifiers.FinalDamage *= customDamageReductionMulti;
            }
            
        }
        
        /// <summary>
        /// 添加自定义减伤值（累加方式）
        /// </summary>
        /// <param name="reduction">要添加的减伤值（0.0-1.0）</param>
        public void AddCustomDamageReduction(float reduction)
        {
            // 启用自定义减伤
            hasCustomDamageReduction = true;
            
            // 累加减伤值
            customDamageReduction += reduction;
            
            // 限制最大减伤为99%
            if (customDamageReduction > 0.99f)
                customDamageReduction = 0.99f;
        }
        
        /// <summary>
        /// 设置自定义减伤值（直接设置方式）
        /// </summary>
        /// <param name="reduction">要设置的减伤值（0.0-1.0）</param>
        public void SetCustomDamageReduction(float reduction)
        {
            // 启用自定义减伤
            hasCustomDamageReduction = true;
            
            // 直接设置减伤值
            customDamageReduction = reduction;
            
            // 限制最大减伤为90%
            if (customDamageReduction > 0.99f)
                customDamageReduction = 0.99f;
        }
        
        /// <summary>
        /// 添加防御前减伤值（累加方式）
        /// </summary>
        /// <param name="reduction">要添加的减伤值（0.0-1.0）</param>
        public void AddPreDefenseDamageReduction(float reduction)
        {
            // 启用防御前减伤
            hasPreDefenseDamageReduction = true;
            
            // 累加减伤值
            preDefenseDamageReduction += reduction;
            
            // 限制最大减伤为99%
            if (preDefenseDamageReduction > 0.99f)
                preDefenseDamageReduction = 0.99f;
        }
        public void MultiPreDefenseDamageReduction(float reduction)
        {
            // 启用防御前减伤
            hasPreDefenseDamageReductionMulti = true;
            
            // 累加减伤值
            preDefenseDamageReductionMulti *= reduction;
            
        }

        public void MulticustomDamageReduction(float reduction)
        {
            // 启用防御前减伤
            hasCustomDamageReductionMulti = true;
            
            // 累加减伤值
            customDamageReductionMulti *= reduction;
            
        }
        
        /// <summary>
        /// 设置防御前减伤值（直接设置方式）
        /// </summary>
        /// <param name="reduction">要设置的减伤值（0.0-1.0）</param>
        public void SetPreDefenseDamageReduction(float reduction)
        {
            // 启用防御前减伤
            hasPreDefenseDamageReduction = true;
            
            // 直接设置减伤值
            preDefenseDamageReduction = reduction;
            
            // 限制最大减伤为90%
            if (preDefenseDamageReduction > 0.99f)
                preDefenseDamageReduction = 0.99f;
        }
        
    }

    public static class ExpansionKeleTool
    {
        /// <summary>
        /// 为指定玩家增加乘算伤害加成
        /// </summary>
        /// <param name="player">要增加伤害的玩家</param>
        /// <param name="bonus">伤害倍数，例如0.1表示增加10%伤害</param>
        public static void AddDamageBonus(Player player, float bonus)
        {
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damagePlayer.AddMultiplicativeDamageBonus(bonus);
        }

        /// <summary>
        /// 为指定玩家设置乘算伤害加成
        /// </summary>
        /// <param name="player">要设置伤害的玩家</param>
        /// <param name="bonus">伤害倍数，例如1.1表示总伤害为110%</param>
        public static void SetDamageBonus(Player player, float bonus)
        {
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damagePlayer.SetMultiplicativeDamageBonus(bonus);
        }

        /// <summary>
        /// 为指定玩家乘算叠加伤害加成
        /// </summary>
        /// <param name="player">要增加伤害的玩家</param>
        /// <param name="bonus">伤害倍数，例如1.1表示在当前基础上增加10%伤害</param>
        public static void MultiplyDamageBonus(Player player, float bonus)
        {
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damagePlayer.MultiplyMultiplicativeDamageBonus(bonus);
        }

        /// <summary>
        /// 为指定玩家添加自定义减伤值
        /// </summary>
        /// <param name="player">要添加减伤的玩家</param>
        /// <param name="reduction">减伤值，例如0.1表示减少10%伤害</param>
        public static void AddDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.AddCustomDamageReduction(reduction);
        }

        /// <summary>
        /// 为指定玩家设置自定义减伤值
        /// </summary>
        /// <param name="player">要设置减伤的玩家</param>
        /// <param name="reduction">减伤值，例如0.1表示减少10%伤害</param>
        public static void SetDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.SetCustomDamageReduction(reduction);
        }
        
        /// <summary>
        /// 为指定玩家添加防御前减伤值
        /// </summary>
        /// <param name="player">要添加减伤的玩家</param>
        /// <param name="reduction">减伤值，例如0.1表示减少10%伤害</param>
        public static void AddPreDefenseDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.AddPreDefenseDamageReduction(reduction);
        }

        /// <summary>
        /// 为指定玩家设置防御前减伤值
        /// </summary>
        /// <param name="player">要设置减伤的玩家</param>
        /// <param name="reduction">减伤值，例如0.1表示减少10%伤害</param>
        public static void SetPreDefenseDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.SetPreDefenseDamageReduction(reduction);
        }
    }

    
}