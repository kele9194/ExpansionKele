using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Microsoft.Xna.Framework; // 添加这一行
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using static Terraria.ModLoader.ModContent;

namespace ExpansionKele.Content.Buff
{
    public class BleedBlue : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public override void SetStaticDefaults() {
            // 设置本地化名称和描述
            //DisplayName.SetDefault("Bleed Blue");
            //Description.SetDefault("敌人变蓝");

            Main.debuff[Type] = true; // 设置为减益效果
            Main.pvpBuff[Type] = true; // 设置为PvP减益效果
            Main.buffNoSave[Type] = true; // 不保存减益效果
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.color = Color.Blue; // 设置敌人颜色为蓝色
        }
    }
}