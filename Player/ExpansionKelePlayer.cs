using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using System;
using ExpansionKele.Content.Buff;
using Microsoft.Xna.Framework;
using ExpansionKele.Properties;
using ExpansionKele.Content.Armor.StarArmorA;
using ExpansionKele.Content.Armor;
using Terraria.ID;

namespace ExpansionKele
{
    public class ExpansionKelePlayer : ModPlayer
    {
        // private Keys? setBonusKey = null; // 缓存键绑定
        private int buffDuration = 504; // 增益持续时间，默认5秒

        public int activeStarryEmblemType = -1;
        public int activeMoonEmblemType = -1;
        public bool moonWarriorEmblemSpeedMode = true;

        public override void ResetEffects()
        {
            // 每一帧开始时重置徽章的应用状态
            activeStarryEmblemType = -1;
            activeMoonEmblemType = -1;
        }
        

        

        


        public override void PostUpdate()
        {
            // 使用 KeybindSystem 来检测按键是否刚刚按下
            if (ExpansionKele.StarKeyBind.JustPressed)
            {
                var buff = ModContent.GetInstance<StarSetBonusBuff>();
                // 使用 Player 属性访问当前玩家实例
                Player playerInstance = Player;

                // 检查玩家是否装备了完整的套装
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmet>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplate>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggings>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[0]/100f);
                    buff.SetTime(buffDuration);
                    // 应用增益
                    playerInstance.AddBuff(buff.Type, buffDuration);
                    //Main.NewText("检测通过", Color.Red);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetA>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateA>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsA>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[0]/100f);
                    buff.SetTime(buffDuration);
                    // 应用增益
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                    //Main.NewText("检测通过", Color.Red);
                }

                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetB>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateB>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsB>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[1]/100f);
                    buff.SetTime(buffDuration);
                    // 应用增益
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                    //Main.NewText("检测通过", Color.Red);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetC>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateC>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsC>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[2]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetD>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateD>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsD>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[3]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetE>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateE>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsE>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[4]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetF>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateF>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsF>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[5]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetG>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateG>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsG>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[6]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetH>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateH>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsH>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[7]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetI>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateI>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsI>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[8]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
                if (playerInstance.armor[0].type == ModContent.ItemType<StarHelmetJ>() &&
                    playerInstance.armor[1].type == ModContent.ItemType<StarBreastplateJ>() &&
                    playerInstance.armor[2].type == ModContent.ItemType<StarLeggingsJ>())
                {
                    buff.SetExtraDamage(ArmorData.GenericDamageBonus[9]/100f);
                    buff.SetTime(buffDuration);
                    playerInstance.AddBuff(ModContent.BuffType<StarSetBonusBuff>(), buffDuration);
                }
            }
        }

        // ... existing code ...
       
// ... existing code ...
    }
}