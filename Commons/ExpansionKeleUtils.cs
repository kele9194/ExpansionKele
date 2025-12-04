using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Commons
{
    public static class ExpansionKeleUtils
    { 
        public static void ConductBetterItemLocation(Player player)
        {
            float xoffset = 6f;
            float yoffset = -10f;
            
            if (player.itemAnimation < player.itemAnimationMax * 0.333)
                yoffset = 4f;
            else if (player.itemAnimation >= player.itemAnimationMax * 0.666)
                xoffset = -4f;
                
            player.itemLocation.X = player.Center.X + xoffset * player.direction;
            player.itemLocation.Y = player.MountedCenter.Y + yoffset;
            
            if (player.gravDir < 0)
                player.itemLocation.Y = player.Center.Y + (player.position.Y - player.itemLocation.Y);
        }

     /// <summary>
        /// 根据配方材料计算物品的价值
        /// </summary>
        /// <param name="item">要计算价值的物品</param>
        /// <param name="profitMargin">利润率，例如1.5表示50%利润</param>
        /// <returns>计算后的物品价值</returns>
        

}


}