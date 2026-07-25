using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.GuardChips;

namespace ExpansionKele.Content.Buff
{
    public class FighterBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {


        }
    }
    public class FighterBuffPlayer : ModPlayer
    {
        public int fighterStack=0;

        public static int MaxFighterStack=5;

        public override void ResetEffects()
        {
            if(!Player.HasBuff(ModContent.BuffType<FighterBuff>())){
                fighterStack=0;
            }
            if(fighterStack>MaxFighterStack){
                fighterStack=MaxFighterStack;
            }
        }
        

    }
}