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
    public class ExpansionKeleWorldGen : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // 在地狱生成之后添加铬矿生成任务
            int index = tasks.FindIndex(genpass => genpass.Name.Equals("Underworld"));
            if (index != -1)
            {
                tasks.Insert(index + 1, new PassLegacy("Chromium Ore", GenerateChromiumOre));
            }
        }

        public override void SetStaticDefaults() {
            // 允许OreRunner替换地狱中的灰烬方块
            TileID.Sets.CanBeClearedDuringOreRunner[TileID.Ash] = true;
        }

        // ... existing code ...
        private void GenerateChromiumOre(GenerationProgress progress, GameConfiguration config)
        {
            progress.Message = "正在生成铬矿";

            // 获取铬矿的tile类型
            int chromiumOreType = ModContent.TileType<Content.Items.Tiles.ChromiumOreTile>();
            
            // 确定生成区域：从Underworld开始到Main.maxTilesY-50
            int startY = Main.UnderworldLayer;
            int endY = Main.maxTilesY-50;
            
            // 确保起始位置小于结束位置
            if (startY >= endY) {
                startY = Main.UnderworldLayer;
                endY = Main.maxTilesY-50;
            }
            
            // 确保不会超出世界边界
            startY = Math.Max(startY, Main.UnderworldLayer);
            endY = Math.Min(endY, Main.maxTilesY-50);
            
            // 生成铬矿矿脉
            int maxAttempts = 10000; // 最大尝试次数
            int generatedCount = 0;
            int maxOre = (int)(Main.maxTilesX * Main.maxTilesY * 0.00012f); // 调整生成密度
            
            for (int k = 0; k < maxOre && generatedCount < maxAttempts; k++)
            {
                // 寻找合适的生成点
                int attempts = 0;
                int x, y;
                
                do
                {
                    x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                    y = WorldGen.genRand.Next(startY, endY);
                    attempts++;
                    generatedCount++;
                } 
                while (attempts < 100 && Main.tile[x, y].TileType != TileID.Ash);
                
                // 只有在灰烬块上才生成矿石
                if (Main.tile[x, y].TileType == TileID.Ash) {
                    // 使用OreRunner生成矿脉
                    WorldGen.OreRunner(x, y, WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(3, 6), (ushort)chromiumOreType);
                }
            }
        }
// ... existing code ...
    }
}