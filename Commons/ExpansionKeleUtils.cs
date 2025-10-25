using Terraria;

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
    } 
}