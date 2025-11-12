using System.Drawing;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class SlicingBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = Main.pvpBuff[Type] = Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 减益效果会在SlicingNPC中处理
        }
    }

    public class SlicingNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // 如果目标有SlicingBuff，则受到的伤害增加10%
            if (npc.HasBuff(ModContent.BuffType<SlicingBuff>()))
            {
                modifiers.FinalDamage *= 1.1f; // 增加10%伤害
            }
            
            // 如果是ChromiumSword并且目标没有SlicingBuff，则增加20%伤害
            if (item.type == ModContent.ItemType<Content.Items.Weapons.Melee.ChromiumSword>() && 
                !npc.HasBuff(ModContent.BuffType<SlicingBuff>()))
            {
                modifiers.FinalDamage *= 1.2f; // 增加20%伤害
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 如果目标有SlicingBuff，则受到的伤害增加10%
            if (npc.HasBuff(ModContent.BuffType<SlicingBuff>()))
            {
                modifiers.FinalDamage *= 1.1f; // 增加10%伤害
            }
        }


    }
}