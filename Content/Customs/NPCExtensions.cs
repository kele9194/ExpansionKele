// 在文件底部或新建一个文件
using Terraria;

public static class NPCExtensions
{
    public static NPC GetNPCOwner(this int npcIndex)
    {
        if (npcIndex >= 0 && npcIndex < Main.npc.Length && Main.npc[npcIndex].active)
        {
            return Main.npc[npcIndex];
        }
        else return null;
    }
}