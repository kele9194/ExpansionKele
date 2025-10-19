using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using ExpansionKele.Content.Customs;
using Terraria.WorldBuilding;
using Terraria.Localization;
namespace ExpansionKele.Content.Buff{

    public  static class MagicSBData{
        public const float DamageBonus100 = 15;


        public const float manaCostBonus100 = 30f;
        public const int manaRegenBonus = 2;

        

        public const float enduranceBonus100 = 15f;


        public const float defenseBonus100 = 4f;
        public const int defenseBonus=4;
        public const float moveSpeedBonus100=15;
        public const int LifeRegenBonus = 2;

    }
public class RedStarBuff : ModBuff
{
    public override string LocalizationCategory => "Buff";
    
    public override void SetStaticDefaults()
    {
        
        Main.buffNoTimeDisplay[Type] = false; // 可选：设置为 true 表示该 buff 不会被保存到存档中
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        tip = Language.GetText("Mods.ExpansionKele.Buff.RedStarBuff.Description").Format(MagicSBData.DamageBonus100);
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // 计算魔法伤害加成
        float baseDamageIncrease = MagicSBData.DamageBonus100/100f;
        float additionalDamageIncrease = MagicSBData.DamageBonus100/100f * player.GetTotalDamage(DamageClass.Magic).ApplyTo(1);
        float totalDamageIncrease = baseDamageIncrease + additionalDamageIncrease;

        // 应用魔法伤害加成
        player.GetDamage(DamageClass.Magic) += totalDamageIncrease;
    }
}

public class BlueStarBuff : ModBuff
{
    public override string LocalizationCategory => "Buff";
    public override void SetStaticDefaults()
    {
        
        Main.buffNoTimeDisplay[Type] = false; // 可选：设置为 true 表示该 buff 不会被保存到存档中
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        tip = Language.GetText("Mods.ExpansionKele.Buff.BlueStarBuff.Description").Format(MagicSBData.manaRegenBonus, MagicSBData.manaCostBonus100);
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // 计算魔法伤害加成
        

        // 应用魔法伤害加成
        player.manaRegen+=MagicSBData.manaRegenBonus;
        player.manaCost-=MagicSBData.manaCostBonus100/100f;
        
    }
}


public class PurpleStarBuff : ModBuff
{
    public override string LocalizationCategory => "Buff";
    public override void SetStaticDefaults()
    {
        
       Main.buffNoTimeDisplay[Type] = false; // 可选：设置为 true 表示该 buff 不会被保存到存档中
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        tip = Language.GetText("Mods.ExpansionKele.Buff.PurpleStarBuff.Description").Format(MagicSBData.enduranceBonus100);
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // 计算魔法伤害加成
        player.endurance+=MagicSBData.enduranceBonus100/100f;
    }
}

public class CyanStarBuff : ModBuff
{
    public override string LocalizationCategory => "Buff";
    public override void SetStaticDefaults()
    {
        
        Main.buffNoTimeDisplay[Type] = false; // 可选：设置为 true 表示该 buff 不会被保存到存档中
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        tip = Language.GetText("Mods.ExpansionKele.Buff.CyanStarBuff.Description").Format(MagicSBData.defenseBonus, MagicSBData.DamageBonus100, MagicSBData.LifeRegenBonus, MagicSBData.moveSpeedBonus100);
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // 计算魔法伤害加成
        player.statDefense+=MagicSBData.defenseBonus;
        player.statDefense*=(1+MagicSBData.DamageBonus100/100f);
        player.lifeRegen+=MagicSBData.LifeRegenBonus;
        player.moveSpeed+=MagicSBData.moveSpeedBonus100/100f;
    }
}



}