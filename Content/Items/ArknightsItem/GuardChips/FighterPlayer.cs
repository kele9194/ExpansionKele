using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    public class FighterPlayer : ModPlayer
    {
        public bool HasFighterWeaponEquipped { get; private set; } = false;


        // ==================== 每帧重置 ====================

        public override void ResetEffects()
        {
            HasFighterWeaponEquipped = false;
            // 注意：FighterStacks 不由这里重置，由 Buff 过期管理
        }
        //斗士刻印更高攻速
        public override float UseSpeedMultiplier(Item item)
        {
            if(HasFighterWeaponEquipped){
                return 1.25f;
            }
            return 1f ;
        }

        // ==================== 装备检测 ====================

        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasFighterWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasFighterWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Fighter)
            {
                HasFighterWeaponEquipped = false;
                return;
            }

            HasFighterWeaponEquipped = true;

            // ===== 同步阻挡 =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Fighter); // 挡1
            blocking.CurrentGuardSubclass = GuardSubclass.Fighter;

            Player.GetModPlayer<ExpansionKeleDamageMulti>().MultiplyMultiplicativeDamageBonus(0.8f);


            int stack=Player.GetModPlayer<FighterBuffPlayer>().fighterStack;
            // ===== 每层 Buff：+2% 攻速（始终生效） =====
            Player.GetAttackSpeed(DamageClass.Melee) +=stack * 0.03f;

            // ===== 半血以上额外 +10% =====
            if (Player.statLife > Player.statLifeMax2 * 0.5f)
            {
                Player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
            }


        }

        // ==================== 命中叠层 ====================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasFighterWeaponEquipped) return;
            if (!target.active || target.friendly) return;



            // 刷新 Buff 持续时间（60 tick = 1秒）
            Player.AddBuff(ModContent.BuffType<FighterBuff>(), 120); // 2秒
            if(Player.GetModPlayer<FighterBuffPlayer>().fighterStack<FighterBuffPlayer.MaxFighterStack){
            Player.GetModPlayer<FighterBuffPlayer>().fighterStack++;}
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!HasFighterWeaponEquipped) return base.FreeDodge(info);
            int stack=Player.GetModPlayer<FighterBuffPlayer>().fighterStack;
            if(Main.rand.NextDouble()<0.15f+stack*0.01f){
                // 给予30帧无敌时间
                Player.immune = true;
                Player.immuneTime = 20;
                
                return true; // 完全闪避成功
            }
            
            
            return base.FreeDodge(info); // 无法完全闪避
        }

        // ==================== 闪避判定 ====================

        
        }
}
