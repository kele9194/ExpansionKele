using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 扩展克勒模组的伤害乘数系统
    /// 用于管理玩家的乘算伤害加成，支持多种伤害类型的统一加成
    /// </summary>
    public class ExpansionKeleDamageMulti : ModPlayer
    {
        //player.GetModPlayer<ExpansionKelePlayer>().SetMultiplicativeDamageBonus(1.15f)
        /// <summary>
        /// 玩家的乘算伤害加成系数
        /// <para>1.0 表示无加成（基础伤害）</para>
        /// <para>1.1 表示增加10%伤害</para>
        /// <para>0.9 表示减少10%伤害</para>
        /// </summary>
        public float MultiplicativeDamageBonus = 1.0f;

        /// <summary>
        /// 每帧重置效果回调
        /// 将乘算伤害加成重置为默认值1.0
        /// </summary>
        public override void ResetEffects()
        {
            // 每帧重置乘算伤害加成为默认值
            MultiplicativeDamageBonus = 1.0f;
        }

        /// <summary>
        /// 修改武器伤害回调
        /// 应用所有伤害类型的乘算加成
        /// </summary>
        /// <param name="item">使用的武器物品</param>
        /// <param name="damage">伤害修饰符引用</param>
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // 应用乘算伤害加成到所有伤害类型
            damage *= MultiplicativeDamageBonus;
        }

        /// <summary>
        /// 设置乘算伤害加成
        /// 直接设置伤害倍数，会覆盖之前的加成值
        /// </summary>
        /// <param name="bonus">伤害倍数系数
        /// <para>1.0 = 基础伤害</para>
        /// <para>1.1 = 增加10%伤害</para>
        /// <para>1.5 = 增加50%伤害</para>
        /// <para>0.8 = 减少20%伤害</para>
        /// </param>
        public void SetMultiplicativeDamageBonus(float bonus)
        {
            MultiplicativeDamageBonus = bonus;
        }

        /// <summary>
        /// 增加乘算伤害加成
        /// 在当前加成基础上累加新的加成值
        /// </summary>
        /// <param name="bonus">要增加的伤害倍数
        /// <para>0.1 = 增加10%伤害</para>
        /// <para>-0.1 = 减少10%伤害</para>
        /// </param>
        public void AddMultiplicativeDamageBonus(float bonus)
        {
            MultiplicativeDamageBonus += bonus;
        }

        /// <summary>
        /// 乘算叠加伤害加成
        /// 将当前加成与新倍数相乘实现复合加成效果
        /// </summary>
        /// <param name="bonus">要乘算的伤害倍数
        /// <para>1.1 = 在当前基础上增加10%</para>
        /// <para>0.9 = 在当前基础上减少10%</para>
        /// </param>
        public void MultiplyMultiplicativeDamageBonus(float bonus)
        {
            MultiplicativeDamageBonus *= bonus;
        }
        // ... existing code ...
        /// <summary>
        /// 修改弹幕击中 NPC 时的伤害
        /// 特别处理召唤物弹幕的乘算伤害加成
        /// 解决 ModifyWeaponDamage 无法为召唤武器的弹幕提供伤害的问题
        /// </summary>
        /// <param name="proj">击中 NPC 的弹幕</param>
        /// <param name="target">被击中的 NPC</param>
        /// <param name="modifiers">伤害修饰符引用</param>
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            SummonDamageHelper.ApplyMultiplicativeBonusToSummon(proj, ref modifiers, MultiplicativeDamageBonus);
        }
    }
// ... existing code ...
    
    /// <summary>
    /// 自定义伤害减免系统
    /// 提供防御前减伤和最终减伤两种机制
    /// 支持加法和乘法两种计算方式
    /// </summary>
    public class CustomDamageReductionPlayer : ModPlayer
    { 
    
        /// <summary>
        /// 自定义伤害减免值（加法计算）
        /// 应用于最终伤害计算阶段
        /// 范围：0.0 - 1.0（0%-100%减伤）
        /// </summary>
        public float customDamageReduction = 0f;
        
        /// <summary>
        /// 是否启用自定义伤害减免
        /// </summary>
        public bool hasCustomDamageReduction = false;

        /// <summary>
        /// 防御前伤害减免值（加法计算）
        /// 在防御计算之前应用的减伤
        /// 范围：0.0 - 1.0（0%-100%减伤）
        /// </summary>
        public float preDefenseDamageReduction = 0f;
        
        /// <summary>
        /// 是否启用防御前伤害减免
        /// </summary>
        public bool hasPreDefenseDamageReduction = false;

        /// <summary>
        /// 自定义伤害减免乘数（乘法计算）
        /// 应用于最终伤害计算阶段
        /// <para>0.9 = 减少10%伤害</para>
        /// <para>1.1 = 增加10%伤害</para>
        /// </summary>
        public float customDamageReductionMulti = 1f;
        
        /// <summary>
        /// 是否启用自定义伤害减免乘数
        /// </summary>
        public bool hasCustomDamageReductionMulti = false;
        
        /// <summary>
        /// 防御前伤害减免乘数（乘法计算）
        /// 在防御计算之前应用的减伤乘数
        /// <para>0.9 = 减少10%伤害</para>
        /// <para>1.1 = 增加10%伤害</para>
        /// </summary>
        public float preDefenseDamageReductionMulti = 1f;
        
        /// <summary>
        /// 是否启用防御前伤害减免乘数
        /// </summary>
        public bool hasPreDefenseDamageReductionMulti = false;


        /// <summary>
        /// 每帧重置效果回调
        /// 重置所有自定义减伤相关属性
        /// </summary>
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

        /// <summary>
        /// 修改受伤回调
        /// 按顺序应用各种减伤效果：
        /// 1. 防御前减伤乘数
        /// 2. 防御前减伤加法
        /// 3. 自定义减伤加法
        /// 4. 自定义减伤乘数
        /// </summary>
        /// <param name="modifiers">受伤修饰符引用</param>
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            // 首先应用防御前减伤乘数
            if(hasPreDefenseDamageReductionMulti){
                modifiers.IncomingDamageMultiplier *= preDefenseDamageReductionMulti;
            }
            
            // 应用防御前减伤效果（加法）
            if (hasPreDefenseDamageReduction && preDefenseDamageReduction != 0f)
            {
                // 使用IncomingDamageMultiplier实现防御前减伤
                modifiers.IncomingDamageMultiplier *= (1f - preDefenseDamageReduction);
            }
            
            // 应用自定义减伤效果（加法）
            if (hasCustomDamageReduction && customDamageReduction != 0f)
            {
                modifiers.FinalDamage *= (1f - customDamageReduction);
            }

            // 最后应用自定义减伤乘数
            if(hasCustomDamageReductionMulti){
                modifiers.FinalDamage *= customDamageReductionMulti;
            }
        }
        
        /// <summary>
        /// 添加自定义减伤值（累加方式）
        /// 在现有减伤基础上增加新的减伤值
        /// </summary>
        /// <param name="reduction">要添加的减伤值
        /// <para>范围：0.0 - 1.0</para>
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.5 = 减少50%伤害</para>
        /// </param>
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
        /// 直接设置减伤值，会覆盖之前的减伤效果
        /// </summary>
        /// <param name="reduction">要设置的减伤值
        /// <para>范围：0.0 - 1.0</para>
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.5 = 减少50%伤害</para>
        /// </param>
        public void SetCustomDamageReduction(float reduction)
        {
            // 启用自定义减伤
            hasCustomDamageReduction = true;
            
            // 直接设置减伤值
            customDamageReduction = reduction;
            
            // 限制最大减伤为99%
            if (customDamageReduction > 0.99f)
                customDamageReduction = 0.99f;
        }
        
        /// <summary>
        /// 添加防御前减伤值（累加方式）
        /// 在现有防御前减伤基础上增加新的减伤值
        /// </summary>
        /// <param name="reduction">要添加的减伤值
        /// <para>范围：0.0 - 1.0</para>
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.3 = 减少30%伤害</para>
        /// </param>
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
        
        /// <summary>
        /// 乘算防御前减伤值
        /// 使用乘法方式叠加减伤效果
        /// </summary>
        /// <param name="reduction">减伤乘数
        /// <para>0.9 = 减少10%伤害</para>
        /// <para>0.8 = 减少20%伤害</para>
        /// <para>1.1 = 增加10%伤害</para>
        /// </param>
        public void MultiPreDefenseDamageReduction(float reduction)
        {
            // 启用防御前减伤乘数
            hasPreDefenseDamageReductionMulti = true;
            
            // 乘算减伤值
            preDefenseDamageReductionMulti *= reduction;
        }

        /// <summary>
        /// 乘算自定义减伤值
        /// 使用乘法方式叠加最终减伤效果
        /// </summary>
        /// <param name="reduction">减伤乘数
        /// <para>0.9 = 减少10%伤害</para>
        /// <para>0.8 = 减少20%伤害</para>
        /// <para>1.1 = 增加10%伤害</para>
        /// </param>
        public void MulticustomDamageReduction(float reduction)
        {
            // 启用自定义减伤乘数
            hasCustomDamageReductionMulti = true;
            
            // 乘算减伤值
            customDamageReductionMulti *= reduction;
        }
        
        /// <summary>
        /// 设置防御前减伤值（直接设置方式）
        /// 直接设置防御前减伤值，会覆盖之前的减伤效果
        /// </summary>
        /// <param name="reduction">要设置的减伤值
        /// <para>范围：0.0 - 1.0</para>
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.5 = 减少50%伤害</para>
        /// </param>
        public void SetPreDefenseDamageReduction(float reduction)
        {
            // 启用防御前减伤
            hasPreDefenseDamageReduction = true;
            
            // 直接设置减伤值
            preDefenseDamageReduction = reduction;
            
            // 限制最大减伤为99%
            if (preDefenseDamageReduction > 0.99f)
                preDefenseDamageReduction = 0.99f;
        }
        
    }

    /// <summary>
    /// 扩展克勒工具类
    /// 提供便捷的方法来操作伤害加成和减伤系统
    /// 所有方法都是静态方法，可以直接调用
    /// </summary>
    public static class ExpansionKeleTool
    {
        /// <summary>
        /// 为指定玩家增加乘算伤害加成
        /// 使用加法方式累加伤害加成
        /// </summary>
        /// <param name="player">要增加伤害的玩家实例</param>
        /// <param name="bonus">伤害加成倍数
        /// <para>0.1 = 增加10%伤害</para>
        /// <para>0.5 = 增加50%伤害</para>
        /// <para>-0.1 = 减少10%伤害</para>
        /// </param>
        public static void AddDamageBonus(Player player, float bonus)
        {
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damagePlayer.AddMultiplicativeDamageBonus(bonus);
        }

        /// <summary>
        /// 为指定玩家设置乘算伤害加成
        /// 直接设置伤害倍数，会覆盖之前的加成
        /// </summary>
        /// <param name="player">要设置伤害的玩家实例</param>
        /// <param name="bonus">伤害倍数系数
        /// <para>1.0 = 基础伤害</para>
        /// <para>1.1 = 总伤害为110%</para>
        /// <para>1.5 = 总伤害为150%</para>
        /// </param>
        public static void SetDamageBonus(Player player, float bonus)
        {
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damagePlayer.SetMultiplicativeDamageBonus(bonus);
        }

        /// <summary>
        /// 为指定玩家乘算叠加伤害加成
        /// 使用乘法方式实现复合加成效果
        /// </summary>
        /// <param name="player">要增加伤害的玩家实例</param>
        /// <param name="bonus">伤害倍数系数
        /// <para>1.1 = 在当前基础上增加10%伤害</para>
        /// <para>0.9 = 在当前基础上减少10%伤害</para>
        /// </param>
        public static void MultiplyDamageBonus(Player player, float bonus)
        {
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damagePlayer.MultiplyMultiplicativeDamageBonus(bonus);
        }

        /// <summary>
        /// 为指定玩家添加自定义减伤值
        /// 在最终伤害计算阶段应用减伤效果
        /// </summary>
        /// <param name="player">要添加减伤的玩家实例</param>
        /// <param name="reduction">减伤值
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.3 = 减少30%伤害</para>
        /// </param>
        public static void AddDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.AddCustomDamageReduction(reduction);
        }

        /// <summary>
        /// 为指定玩家设置自定义减伤值
        /// 直接设置最终减伤值，会覆盖之前的减伤效果
        /// </summary>
        /// <param name="player">要设置减伤的玩家实例</param>
        /// <param name="reduction">减伤值
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.5 = 减少50%伤害</para>
        /// </param>
        public static void SetDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.SetCustomDamageReduction(reduction);
        }
        
        /// <summary>
        /// 为指定玩家添加防御前减伤值
        /// 在防御计算之前应用减伤效果
        /// </summary>
        /// <param name="player">要添加减伤的玩家实例</param>
        /// <param name="reduction">减伤值
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.2 = 减少20%伤害</para>
        /// </param>
        public static void AddPreDefenseDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.AddPreDefenseDamageReduction(reduction);
        }

        /// <summary>
        /// 为指定玩家设置防御前减伤值
        /// 直接设置防御前减伤值，会覆盖之前的减伤效果
        /// </summary>
        /// <param name="player">要设置减伤的玩家实例</param>
        /// <param name="reduction">减伤值
        /// <para>0.1 = 减少10%伤害</para>
        /// <para>0.4 = 减少40%伤害</para>
        /// </param>
        public static void SetPreDefenseDamageReduction(Player player, float reduction)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.SetPreDefenseDamageReduction(reduction);
        }
        
        /// <summary>
        /// 为指定玩家乘算防御前减伤值
        /// 使用乘法方式叠加防御前减伤效果
        /// </summary>
        /// <param name="player">要设置减伤的玩家实例</param>
        /// <param name="multiplier">减伤乘数
        /// <para>0.9 = 减少10%伤害</para>
        /// <para>0.8 = 减少20%伤害</para>
        /// <para>1.1 = 增加10%伤害</para>
        /// </param>
        public static void MultiplyPreDefenseDamageReduction(Player player, float multiplier)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.MultiPreDefenseDamageReduction(multiplier);
        }
        
        /// <summary>
        /// 为指定玩家乘算自定义减伤值
        /// 使用乘法方式叠加最终减伤效果
        /// </summary>
        /// <param name="player">要设置减伤的玩家实例</param>
        /// <param name="multiplier">减伤乘数
        /// <para>0.9 = 减少10%伤害</para>
        /// <para>0.7 = 减少30%伤害</para>
        /// <para>1.2 = 增加20%伤害</para>
        /// </param>
        public static void MultiplyCustomDamageReduction(Player player, float multiplier)
        {
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.MulticustomDamageReduction(multiplier);
        }
    }
}