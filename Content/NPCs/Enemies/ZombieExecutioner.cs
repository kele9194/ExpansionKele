using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ExpansionKele.Content.NPCs.Enemies
{
    /// <summary>
    /// 僵尸处决者：沿用僵尸的 AI 与动画；血量 ×2、移速 ×0.66、基础接触伤害 ×1.2。
    /// 每只处决者只有一次"处决"机会：第一次命中玩家时额外追加其最大生命值的
    /// 25% / 33% / 50%（经典 / 专家 / 大师），用掉后不再追加。
    /// </summary>
    public class ZombieExecutioner : ModNPC
    {
        private const float SpeedMultiplier = 0.6f;//0.75对应70%基础僵尸移速
        /// <summary>原版僵尸水平速度上限——需实测标定后再定稿。</summary>
        private const float ZombieMaxRunSpeed = 1f; // TODO 标定

        /// <summary>本只处决者的处决标记（每个 NPC 实体一份，见下方 GlobalNPC）。</summary>
        private ZombieExecutionerExecutionTracker Execution
            => NPC.GetGlobalNPC<ZombieExecutionerExecutionTracker>();

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie]; // 16 帧

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers {
                Velocity = 1f
            });
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.Zombie); // 宽高 / 伤害 / 防御 / 击退 / aiStyle 全套
            AIType = NPCID.Zombie;           // 僵尸专属 AI 分支
            AnimationType = NPCID.Zombie;    // 行走 / 跳跃动画

            Banner = Item.NPCtoBanner(NPCID.Zombie);
            BannerItem = Item.BannerToItem(Banner);

            NPC.lifeMax *= 2;                             // 45 → 90（经典）
            NPC.damage = (int)(NPC.damage * 1.2f + 0.5f); // 14 → 17：处决改为一次性的补偿
        }

        public override void PostAI()
        {
            float max = ZombieMaxRunSpeed * SpeedMultiplier;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -max, max);
        }

        /// <summary>只读取处决状态，改数值；此时命中还可能被闪避/完全格挡。</summary>
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (Execution.HasUsedExecution) return;
            float ratio = Main.masterMode ? 0.4f : Main.expertMode ? 0.33f : 0.25f;
            modifiers.FinalDamage.Base += target.statLifeMax2 * ratio;
        }

        /// <summary>伤害真正结算后才消耗这一次处决机会，避免被闪避/格挡"骗掉"。</summary>
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage <= 0) return;
            Execution.HasUsedExecution = true;
            // TODO 视听反馈：第一次处决时给个音效 / 血色 Dust / CombatText，
            //      否则玩家无法感知"这只僵尸已经没有处决了"。
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
            => SpawnCondition.OverworldNightMonster.Chance * 0.1f; // ≈ 普通僵尸的 1/10

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            foreach (var rule in Main.ItemDropsDB.GetRulesForNPCID(NPCID.Zombie, false))
                npcLoot.Add(rule);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Mods.ExpansionKele.NPCs.ZombieExecutioner.Bestiary")
            });
        }
    }

    /// <summary>
    /// 每个处决者实体一份的标记。放在 GlobalNPC 而非 ModNPC 字段里，
    /// 因为 ModNPC 是按类型单例的，写在里面会被所有处决者共享
    /// （同 <c>ReaperGlobalNPC</c> 的 InstancePerEntity 先例）。
    /// </summary>
    public class ZombieExecutionerExecutionTracker : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>本只处决者是否已用掉那一次加伤处决。</summary>
        public bool HasUsedExecution { get; set; }
    }
}