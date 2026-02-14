using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Projectiles.MagicProj;

namespace ExpansionKele.Content.StaryMagic
{
    
    public class StarStaff : ModItem
{
    public override string LocalizationCategory => "StaryMagic";
    public override string Texture =>"ExpansionKele/Content/StaryMagic/StarStaffs";
    public static int randomNum=20;
    public static float PerDamageMultiplier = 0.12f;
    public static float DamageMaxMultiplier = 2.2f;

    public override void SetDefaults()
    {
        //Item.SetNameOverride("星元法杖");
        Item.damage = 50;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 12;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 30;
        Item.useAnimation = 30;
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

    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // 计算玩家周围的敌对NPC数量
    int npcCount = 0;
    int range = 1600;
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
    float damageMultiplier = 1 + (PerDamageMultiplier * npcCount);
    
    if (damageMultiplier > DamageMaxMultiplier)
    {
        damageMultiplier = DamageMaxMultiplier;
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
        
        return false; // 不使用默认的射弹生成逻辑
    }

     public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipData = new Dictionary<string, string>
            {
                {"Introduction", $"随机发射四种追踪射弹,射弹颜色依次为蓝,红,紫,青\n"+
                $"蓝色命中后回复自身{Data.BaseMana}+已损蓝量{Data.BasePreMana*100f}%的蓝量,并且获得{Data.buffDuration}ticks蓝耗减少的增益\n"+
                $"红色命中后造成{(int)Data.redDamageBonus}倍伤害，同时在{Data.buffDuration}ticks内获得额外乘算{MagicSBData.DamageBonus100}%魔法增伤\n"+
                $"紫色射弹造成回复{Data.BaseLife}已损生命值{Data.BasePreLife*100f}%的血量在接下来{Data.buffDuration}ticks内并额外获得{MagicSBData.enduranceBonus100}%的减伤\n"+
                $"青色射弹命中后飞行时间回复{Data.BaseWingTime},生命回复时间增加{Data.lifeRegenTime}，,获得额外对单位当前生命值{Data.PreTargetLife100}%的伤害加成\n"+
                $"接下来{Data.buffDuration}ticks获得防御+{MagicSBData.defenseBonus}+{MagicSBData.defenseBonus100}%，生命回复增加{MagicSBData.LifeRegenBonus}\n"+
                $"彩色射弹只有1/{randomNum}出现，但是造成基础伤害{Data.StarDamageBonus}倍，额外造成对目标最大生命值{Data.PreTargetLifeMax100}%的伤害，且同时可以造成上述所有效果\n"+
                $"多个敌方单位在附近时可以额外造成伤害{PerDamageMultiplier}每个，最多可造成{DamageMaxMultiplier}倍伤害"} // 移速通常是百分比形式
				//{"Tooltip",$"星元套装的第一个系列的护胫"}
            };

            foreach (var kvp in tooltipData)
            {
                tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            }
        }

}
}





    
    
    
    


    





