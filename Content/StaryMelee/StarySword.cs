using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;
using Terraria.DataStructures;
using System;





namespace ExpansionKele.Content.StaryMelee
{
    public class StarySword : ModItem
    {
        public override string LocalizationCategory => "StaryMelee";
         public const int LeftClickDamage = 40;
        private const float LeftClickKnockBack = 8f;
        private const float LeftClickShootSpeed = 10f;
        private const int LeftClickUseTime = 20;
        private const int LeftClickUseAnimation = LeftClickUseTime;

        private const int constcrit = 30;
        private const int constrare = 9;
        private const string setNameOverride="StarySword Beta";

        private const string introduction ="星元剑系列始祖";


         private const int RightClickDamage = (int)(LeftClickDamage*rightClickCoefficient); // 示例伤害值
        private const float RightClickKnockBack = 10f; // 示例击退值
        private const float RightClickShootSpeed = 40f; // 示例射击速度
        private const int RightClickUseTime = (int)(LeftClickUseTime*2); // 示例使用时间
        private const int RightClickUseAnimation = RightClickUseTime; // 示例使用动画时间
        const double rightClickCoefficient = 2.7;
        //int rightClickBulletType=ExpansionKeleCal.expansionkele.Find<ModProjectile>("SharkyBullet").Type;

        double critOverflowCoefficient = 1.5;

        public double damageGenericUp  { get; set; }
        public double damageMeleeUp  { get; set; }
        
         private bool isDashing = false;
        private Vector2 AfterDashVelocity; // 记录冲刺前的速度
        private float dashingUp = 36f;
        private float dashingUp2 ;

        private int totalDashingTime=10;

        private float dashingGravity = 0.1f;

        private float defaultGravity =0.4f;

        private int immuningtime=40;

        //private int cooldownTicks = 7;
        private int BoostDuration;

         public StarySword()
        {
            
            dashingUp2 = dashingUp / 12; // 在构造函数中初始化
            
        }

        // public override Color? GetAlpha(Color lightColor) {
		// 	return Color.Red;
		// }

        public override void SetStaticDefaults()
	{
		//ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
	}

        public override void SetDefaults()
        {
        //Item.SetNameOverride(setNameOverride);
        Item.width = 80;
		Item.height = 80;
		Item.damage = LeftClickDamage;
        if(ExpansionKele.calamity!=null){
            Item.damage=(int)(Item.damage*1.25);
        }
		Item.DamageType = DamageClass.Melee;
		Item.useAnimation = LeftClickUseAnimation ;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = LeftClickUseTime;
		Item.useTurn = false;//自动转向
		Item.knockBack = LeftClickKnockBack;
		Item.UseSound = SoundID.Item1;
		Item.autoReuse = true;
        Item.value = Item.sellPrice(gold: 1); // 价值
        Item.rare = ItemRarityID.Blue; // 稀有度
        Item.shoot = ModContent.ProjectileType<ColaProjectile>(); // 射弹类型
        Item.shootSpeed =  LeftClickShootSpeed; // 射弹速度
        Item.crit= constcrit;
        Item.rare=constrare;
        }

         public override Vector2? HoldoutOffset() {  
             return new Vector2(0, 0); // 持有偏移量。  
         }  

        
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
    //     if (cooldownTicks > 0)
    // {
    //     return false; // 如果冷却时间大于0，不允许使用物品
    // }
       
        
        // 检查是否是挥动的开始
        
            // 发射单个射弹
            Terraria.Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false; // 返回 false 以防止默认行为
    }

    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
    // 临时移除敌人的免疫状态
    target.immune[player.whoAmI] = 0;
}
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // 添加自定义的 tooltip  
            TooltipLine line = new TooltipLine(Mod, setNameOverride, introduction);
            tooltips.Add(line);
        }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
            // Additional effects on hit can be added here if needed
            //近战增益
            if(target.lifeMax<=8000)
            {
                BoostDuration = 5;
            }
            else
            {
                BoostDuration = 25;
            }
            player.statDefense += 15;

            // 增加玩家的移动速度15%
            player.moveSpeed += 0.15f;

            player.endurance += 0.10f;

            // 增加玩家的回血速率 +10Hp/s
            player.lifeRegen += 10 * 20;

            // 恢复玩家的飞行时间20ticks
            //player.wingTime += 20;
            
            target.AddBuff(ModContent.BuffType<ArmorPodweredLower>(), 82);
            target.AddBuff(BuffID.OnFire, 82);
            int lifePercentageDamage = (int)(target.lifeMax * 0.005); // 例如，10%生命值
            damageDone=(int)(damageDone*Math.Sqrt(damageDone)/Math.Sqrt(Item.damage)+1);
            damageDone+=lifePercentageDamage;
            Vector2 position = player.Center;
    Vector2 velocity = player.DirectionTo(target.Center).SafeNormalize(Vector2.UnitX) * 10f; // 调整速度和方向
    Terraria.Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity, ModContent.ProjectileType<LifePercentageProjectile>(), damageDone, 0f, player.whoAmI, target.whoAmI);
            
        }
    
    public override bool AltFunctionUse(Player player)
{
    return true; // 允许右键使用
    
}

public override bool CanUseItem(Player player)
{
    
    if (player.altFunctionUse == 2)
    {
        
        player.itemTime = totalDashingTime; // 设置一个合理的值
        player.itemAnimation = totalDashingTime; // 设置一个合理的值
        
        return true;
    }
    return base.CanUseItem(player);
}

public override bool? UseItem(Player player)
{
    //Main.NewText("进入 UseItem", 255, 255, 255);
    //Main.NewText($"player.altFunctionUse: {player.altFunctionUse}", 255, 255, 255);
    if (player.altFunctionUse == 2)
    {
        
        //Main.NewText("右键使用2", 255, 255, 255);
        // 设置冲刺方向
        float lifeLoss = (float)(player.statLifeMax2 * 0.012 + player.statLife * 0.016 + 2);
                player.statLife -= (int)lifeLoss;
                player.HealEffect((int)lifeLoss, true);

                // 检查生命值是否小于0
                if (player.statLife <= 0)
                {
                    player.immune = false;
                    player.immuneTime = 0;
                    player.KillMe(PlayerDeathReason.ByCustomReason(player.name + " 受到了复仇和过往的反噬"), 9999, 0);
                    return true;
                }
        Vector2 direction = player.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX);
        direction *= dashingUp; // 冲刺速度

        // 设置无敌帧
        player.immune = true;
        player.immuneTime = immuningtime;

        // 设置玩家速度以实现冲刺效果
        player.velocity = direction;
        AfterDashVelocity = player.velocity;

        player.gravity = dashingGravity;
        

        // 记录冲刺开始时间
        player.itemAnimation = totalDashingTime; // 假设冲刺持续10帧
        isDashing = true;
        

        return true;
    }
    else
    {
        
        // 左键使用逻辑
        return base.UseItem(player);
    }
}

public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);
            

            if (isDashing)
            {
                player.gravity = dashingGravity;

                int remainingTime = player.itemAnimation;
                int thresholdTime = (int)(totalDashingTime*2 / 5); // 计算冲刺时间的1/5点

                if (remainingTime <= thresholdTime)
                {
                    // 开始逐渐降低速度和恢复重力
                    float t = (float)(thresholdTime - remainingTime) / thresholdTime; // 计算从1/5到结束的时间比例
                    player.velocity = Vector2.Lerp(player.velocity, AfterDashVelocity / dashingUp2, t);
                    player.gravity = MathHelper.Lerp(dashingGravity, defaultGravity, t);
                }

                if (remainingTime <= 0)
                {
                    // 重置速度
                    player.velocity = AfterDashVelocity / dashingUp2;

                    // 恢复重力
                    player.gravity = defaultGravity;

                    // 重置冲刺状态标志
                    isDashing = false;
                }
            }

    //         if (cooldownTicks > 0)
    // {
    //     cooldownTicks--;
    // }

    if (BoostDuration > 0)
    {
        // 如果防御增益仍在持续，减少计时器
        BoostDuration--;
        player.statDefense += 15;

            // 增加玩家的移动速度15%
        player.moveSpeed += 0.15f;
        player.endurance += 0.10f;

            // 增加玩家的回血速率 +10Hp/s
        player.lifeRegen += 10 * 20;



    }



        }
    }
}



