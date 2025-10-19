using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExpansionKele
{
    /*public class ExpansionKeleWorldGen : ModSystem
    {


        private void CheckAndGenerateOre()
        {
            var config = ModContent.GetInstance<ExpansionKeleConfig>();

            if (NPC.downedMechBossAny && !config.FullMoonOreGenerated)
            {
                GenerateFullMoonOre(null, null);
                config.FullMoonOreGenerated = true;
            }
        }
        
        public override void PostUpdateEverything()
{
    if (!ModContent.GetInstance<ExpansionKeleConfig>().FullMoonOreGenerated)
    {
        CheckAndGenerateOre();
    }
}

        private void GenerateFullMoonOre(GenerationProgress progress, GameConfiguration config)
{
    if (progress == null)
    {
        progress = new GenerationProgress();
        progress.Message = "罪恶与力量于石头处渗出";
        Main.NewText(progress.Message, 255, 255, 255);
    }

    int oreType = ModContent.TileType<FullMoonOreTile>();

    for (int i = 0; i < 600; i++)
{
    int x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
    int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY); // 修改此处

    WorldGen.TileRunner(x, y, WorldGen.genRand.Next(7, 12), 3, oreType);
}
}
        }*/
    }