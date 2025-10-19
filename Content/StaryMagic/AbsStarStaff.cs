using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;

namespace ExpansionKele.Content.StaryMagic
{
    
    public abstract class AbsStarStaff : ModItem
{
    protected abstract string setNameOverride { get; }
    
    public override string Texture => "ExpansionKele/Content/StaryMagic/StarStaffs";

        protected virtual int randomNum => 20;
        protected virtual float perDamageMultiplier => 0.12f;
        protected virtual float damageMaxMultiplier => 2.2f;
        protected virtual int projectileNum => 4;
        protected virtual int itemMana => 12;
        protected abstract int damage { get; }
        protected virtual int useTime => 30;
        protected virtual float multipleDDamage => 3.2f;
        protected virtual float angle => 15f;
        protected virtual int searchRange => 1600;

    public override void SetStaticDefaults()
	{
		ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
	}
    public override void SetDefaults()
    {
        //Item.SetNameOverride(setNameOverride);
        Item.damage = damage;
        Item.DamageType = DamageClass.Magic;
        Item.mana = itemMana;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = useTime;
        Item.useAnimation = Item.useTime;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 2;
        Item.value = Item.sellPrice(0, 1, 0, 0);
        Item.rare = ItemRarityID.Pink; 
        Item.UseSound = SoundID.Item71;
        Item.shoot=ProjectileID.BlackBolt;//无用但得有
        Item.autoReuse = true;
        Item.shootSpeed = 30f;
    }
    public override bool AltFunctionUse(Player player)
        {
            return true; // 允许右键使用
        }

    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2)
{
    damage=(int)(damage/projectileNum*multipleDDamage+0.5f);
    Vector2 playerCenter = player.Center;
    Vector2 mousePosition = Main.MouseWorld;
    Vector2 shootDirection = mousePosition - playerCenter;
    shootDirection.Normalize();
    float angle = shootDirection.ToRotation();
    float angleVariation = MathHelper.ToRadians(angle);

    // 存储四个随机角度和速度
    float[] randomAngles = new float[projectileNum];
    Vector2[] randomVelocities = new Vector2[projectileNum];

    for (int i = 0; i < projectileNum; i++)
    {
        float randomAngle = angle + Main.rand.NextFloat(-angleVariation, angleVariation);
        Vector2 finalDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
        Vector2 randomVelocity = finalDirection * Main.rand.NextFloat(6f,8f); // 随机速度

        randomAngles[i] = randomAngle;
        randomVelocities[i] = randomVelocity;
    }

    // 使用循环射出四个抛射体
    for (int i = 0; i < projectileNum; i++)
    {
        Terraria.Projectile.NewProjectile(source, playerCenter, randomVelocities[i], ModContent.ProjectileType<MagicStarProjectileLower>(), damage, knockback, player.whoAmI);
    }
}
        
        
        else{
        // 计算玩家周围的敌对NPC数量
    int npcCount = 0;
    int range = searchRange;
    Vector2 playerCenter = player.Center;

    for (int i = 0; i < Main.maxNPCs; i++)
    {
        NPC npc = Main.npc[i];
        if (npc.active && npc.CanBeChasedBy() && Vector2.Distance(playerCenter, npc.Center) <= range)
        {
            npcCount++;
        }
    }

    // 计算增伤值，最大增伤为3倍
    float damageMultiplier = 1 + (perDamageMultiplier * npcCount);
    
    if (damageMultiplier > damageMaxMultiplier)
    {
        damageMultiplier = damageMaxMultiplier;
    }

    // 应用增伤
    damage = (int)(damage * damageMultiplier);
    
        int randomEffect = Main.rand.Next(4); // 随机选择一种效果
        switch (randomEffect)
        {
            case 0:
                Terraria.Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<MagicBlueProjectile>(), damage, knockback, player.whoAmI);
                break;
            case 1:
                Terraria.Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<MagicRedProjectile>(), damage, knockback, player.whoAmI);
                break;
            case 2:
                Terraria.Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<MagicPurpleProjectile>(), damage, knockback, player.whoAmI);
                break;
            case 3:
                {
                    
                    Terraria.Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<MagicCyanProjectile>(), damage, knockback, player.whoAmI);
                    break;
                    }
                
        }
        int randomEffect2 = Main.rand.Next(randomNum);
        if(randomEffect2==0){
            Terraria.Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<MagicStarProjectile>(), damage, knockback, player.whoAmI);
        }
        }
        return false; // 不使用默认的射弹生成逻辑
    }

     // ... existing code ...
public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"Introduction", $"[c/00FF00:随机发射四种追踪射弹,射弹颜色依次为蓝,红,紫,青\n"+
                    $"蓝色命中后回复自身{Data.BaseMana}+已损蓝量{Data.BasePreMana*100f}%的蓝量,并且获得{Data.buffDuration}ticks蓝耗减少的增益\n"+
                    $"红色命中后造成{(int)Data.redDamageBonus}倍伤害，同时在{Data.buffDuration}ticks内获得额外乘算{MagicSBData.DamageBonus100}%魔法增伤\n"+
                    $"紫色射弹造成回复{Data.BaseLife}已损生命值{Data.BasePreLife*100f}%的血量在接下来{Data.buffDuration}ticks内并额外获得{MagicSBData.enduranceBonus100}%的减伤\n"+
                    $"青色射弹命中后飞行时间回复{Data.BaseWingTime},生命回复时间增加{Data.lifeRegenTime}，,获得额外对单位当前生命值{Data.PreTargetLife100}%的伤害加成\n"+
                    $"接下来{Data.buffDuration}ticks获得防御+{MagicSBData.defenseBonus}+{MagicSBData.defenseBonus100}%，生命回复增加{MagicSBData.LifeRegenBonus}\n"+
                    $"彩色射弹只有1/{randomNum}出现，但是造成基础伤害{Data.StarDamageBonus}倍，额外造成对目标最大生命值{Data.PreTargetLifeMax100}%的伤害，且同时可以造成上述所有效果\n"+
                    $"多个敌方单位在附近时可以额外造成伤害{perDamageMultiplier}每个，最多可造成{damageMaxMultiplier}倍伤害]"} 
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
            else
            {
                var tooltipData = new Dictionary<string, string>
                {
                    {"Introduction", $"[c/00FF00:按下左shift查看详情]"} 
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
            }
        }
// ... existing code ...

        public abstract override void AddRecipes();
	


}
}
