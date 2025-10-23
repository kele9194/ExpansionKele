using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class BrainOfMonster : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = 100; // 设置基础价值
            Item.rare = ItemRarityID.White; // 普通稀有度
            Item.consumable = false;
             // 确保不能放置
        }

        public override bool CanBeConsumedAsAmmo(Item ammo, Player player)
        {
            return false; // 确保不能作为弹药使用
        }
    }
    public class FaceMonsterDrop : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npc, npcLoot);
            
            // 检查是否是原版的血腥僵尸(FaceMonster), NPCID:181
            if (npc.type == NPCID.FaceMonster)
            {
                // 添加10%概率掉落1个怪物大脑
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BrainOfMonster>(), 10, 1, 1));
            }
        }
    }
}