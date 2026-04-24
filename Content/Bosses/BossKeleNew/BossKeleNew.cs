// 引入必要的命名空间
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using Terraria.Audio;
using ExpansionKele.Content.Items.Placeables;
using Terraria.Localization;
using ExpansionKele.Content.Customs;
using InnoVault;
using Microsoft.Build.Tasks;
namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    // 自动加载Boss头像
    [AutoloadBossHead]
    public class BossKeleNew : ModNPC
    {
        public override string LocalizationCategory=>"Bosses.BossKeleNew";
        public override string Texture => this.GetRelativeTexturePath("./BossKeleNewSheet");
        public static LocalizedText SpawnCondition { get; private set; }


        // 定义Boss的攻击类型枚举
        public enum Phase
        {
            phase1=1,
            phase2=2,
            phase3=3,
            phase4=4


        }

        public enum AttackType
        {
            Melee=11,
            Ranged=12,
            Magic=13,
            Summon=14,
            Other=15,
        }

        public enum HorizaonalMove
        {
            None=20,
            Left=21,
            Right=22,
        }

        public enum VerticalMove
        {
            None=30,
            Up=31,
            Down=32,
        }

        public float totalTimer=>NPC.ai[0];
        public float coolDown => NPC.ai[1];
        public float subTimer=>NPC.ai[2];
        [SyncVar]
        public int _phaseNum;
        public int phaseNum
        {
            get => _phaseNum;
            set => _phaseNum = value;
        }
        public Phase phase => (Phase)phaseNum;
        
        [SyncVar]
        public int _attackTypeNum;
        public int attackTypeNum
        {
            get => _attackTypeNum;
            set => _attackTypeNum = value;
        }
        public AttackType attackType => (AttackType)attackTypeNum;
        [SyncVar]
        private int _horizontalMoveNum;
        public int horizontalMoveNum
        {
            get => _horizontalMoveNum;
            set => _horizontalMoveNum = value;
        }
        public HorizaonalMove horizontalMove => (HorizaonalMove)horizontalMoveNum;
        
        [SyncVar]
        private int _verticalMoveNum;
        public int verticalMoveNum
        {
            get => _verticalMoveNum;
            set => _verticalMoveNum = value;
        }
        public VerticalMove verticalMove => (VerticalMove)verticalMoveNum;
        
        [SyncVar]
        public int currentAttackIndex = 0;
         private const float MaxHorizontalSpeed = 40f;
        private const float HorizontalAcceleration = 0.5f;
        private const float HorizontalDeceleration = 3f;
        
        private const float MaxVerticalSpeed = 40f;
        private const float VerticalAcceleration = 0.5f;
        private const float VerticalDeceleration = 3f;
        

        


        // 不同模式下的基础生命值设定
        private const int NormalHP = 150000;

        // 不同模式下的基础防御值设定
        private const int NormalDefense = 50;
        private const int ExpertDefense = 65;
        private const int MasterDefense = 80;

        // 静态默认设置（在游戏加载时调用一次）
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4; // 设置为单帧纹理
            NPCID.Sets.BossBestiaryPriority.Add(NPC.type); // 添加到图鉴优先级列表
            SpawnCondition = this.GetLocalization("SpawnCondition");
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
        }
        // 每次生成NPC时调用，用于初始化NPC属性
        public override void SetDefaults()
            {
                // 基础尺寸和碰撞箱设置
                NPC.width = 30;
                NPC.height = 48;
                // 攻击伤害与防御属性
                NPC.damage = 60;
                NPC.defense = NormalDefense;

                // 生命值根据游戏模式调整
                NPC.lifeMax = NormalHP;
                if (Main.masterMode)
                {
                    NPC.defense = MasterDefense;
                }
                else if (Main.expertMode)
                {
                    NPC.defense = ExpertDefense;
                }

                // 音效与物理属性
                NPC.HitSound = SoundID.NPCHit54;
                NPC.DeathSound = SoundID.NPCDeath6;
                NPC.noGravity = true; // 禁用重力
                NPC.noTileCollide = true; // 禁止与方块碰撞
                NPC.boss = true; // 标记为Boss
                NPC.lavaImmune = true; // 免疫岩浆伤害
                NPC.aiStyle = -1; // 自定义AI样式
                NPC.netAlways = true; // 总是同步网络数据

                // 经济与音乐设置
                NPC.value = Item.buyPrice(0, 10, 0, 0);
                NPC.knockBackResist = 0f; // 完全免疫击退
                Music = MusicID.Boss5; // 背景音乐
                NPC.BossBar = null; // 可替换为自定义Boss血条
                NPC.timeLeft = 300;

                // 初始无敌帧设置
                NPC.dontTakeDamage = true; // 开启无敌状态
                NPC.netUpdate = true;
                NPC.knockBackResist=0f;
                
                
            }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return base.CanHitPlayer(target, ref cooldownSlot);
        }


        // 图鉴信息设置
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            
        }


        // 当NPC生成时调用 - 修复网络初始化问题
        public override void OnSpawn(IEntitySource source)
        {
            
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            // 直接设置NPC造成防御损伤
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                calamity.Call("SetDefenseDamageNPC", NPC, true);
            }
            
            // 触发网络同步
            NPC.netUpdate = true;
        }
        public override void AI()
        {
            if (totalTimer % 5 == 0)
            {
                NPC.frame.Y = ((int)(totalTimer / 5) % 4) * NPC.frame.Height;
            }

            if(NPC.timeLeft <= 0){
                NPC.active = false;
            }

            NPC.ai[0]++;
            if(totalTimer >= 5){
                NPC.dontTakeDamage = false;
            }

            if (NPC.ai[0] >=21600)
            {
                NPC.active = false;
            }
            CoolDown(ref NPC.ai[1]);
            CoolDown(ref NPC.ai[2]);
            
            // 让 NPC 寻找最近的玩家并面向它
            NPC.TargetClosest(true);

             NPC.spriteDirection =GetSpriteDirection(NPC);

             if (CanAttatck())
            {
                NPC.TargetClosest(true);
                attackTypeNum = (int)ChooseAttackType();
            }

             if (NPC.HasValidTarget)
            {
                NPC.velocity=Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Main.player[NPC.target].Center) * 5f, 0.3f);
            }
            else if(NPC.ai[0]%300==0){
                NPC.active = false;
            }
            // 更新Boss阶段
            phaseNum = (int)GetPhase(NPC);
            
            // 每180帧（3秒）选择一次攻击类型
            
            //必须是服务器才能获得boss的弹幕
            if(attackType==AttackType.Summon&&CanAttatck()){

                       Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        NPC.DirectionTo(Main.player[NPC.target].Center)*15f,
                        ModContent.ProjectileType<RinyaBossProjectile>(),
                        (int)(NPC.damage*0.3f),
                        0,
                        Main.myPlayer,
                        NPC.whoAmI,
                        (int)phase
                       );
                       NPC.ai[1]=300;
            }
            


            



            if (attackType == AttackType.Magic&&CanAttatck())
            {
                    if(phase == Phase.phase1)
                    { 
                    for(int i = 0; i < 4; i++)
                    Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center+MathHelper.ToRadians(i*90f).ToRotationVector2()*100f,           // 弹幕位置
                    NPC.DirectionTo(Main.player[NPC.target].Center)*15f,       // 速度（不需要，因为 ShouldUpdatePosition 返回 false）
                    ModContent.ProjectileType<MagicRedBossProjectile>(),
                    (int)(NPC.damage*0.3f),
                    0,
                    Main.myPlayer,
                    NPC.whoAmI,
                    (int)phase
                    );
                    }
                    if(phase == Phase.phase2)
                    {
                        for(int i=0;i<12;i++){
                            Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            NPC.Center+MathHelper.ToRadians(i*30f).ToRotationVector2()*720f,           // 弹幕位置
                            MathHelper.ToRadians(i*30f+80f).ToRotationVector2()*10f,       // 速度（不需要，因为 ShouldUpdatePosition 返回 false）
                            ModContent.ProjectileType<MagicBlueBossProjectile>(),
                            (int)(NPC.damage*0.2f),
                            0,
                            Main.myPlayer,
                            NPC.whoAmI,
                            (int)phase
                            );
                        } 
                    }
                    if(phase == Phase.phase3){
                        Vector2 targetCenter = Main.player[NPC.target].Center;
                        for(int i=0;i<6;i++){
                            Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            targetCenter+MathHelper.ToRadians(i*60f).ToRotationVector2()*180f,           // 弹幕位置
                            MathHelper.ToRadians(i*60f+180f).ToRotationVector2()*10f,       // 速度（不需要，因为 ShouldUpdatePosition 返回 false）
                            ModContent.ProjectileType<MagicPurpleBossProjectile>(),
                            (int)(NPC.damage*0.3f),
                            0,
                            Main.myPlayer,
                            NPC.whoAmI,
                            (int)phase
                            );
                        } 
                    }
                    if(Phase.phase4 == phase){ 
                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            NPC.Center+NPC.DirectionTo(Main.player[NPC.target].Center)*-15f,// 弹幕位置
                            NPC.DirectionTo(Main.player[NPC.target].Center)*25f,       // 速度（不需要，因为 ShouldUpdatePosition 返回 false）
                            ModContent.ProjectileType<CyanProjectileShooter>(),
                            (int)(NPC.damage*0.3f),
                            0,
                            Main.myPlayer,
                            NPC.whoAmI,
                            (int)phase
                            );
                
                    }
                    NPC.ai[1] = 120;
            }

            if(attackType == AttackType.Ranged&&CanAttatck()){
                Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,           // 弹幕位置
                NPC.DirectionTo(Main.player[NPC.target].Center)*15f,       // 速度（不需要，因为 ShouldUpdatePosition 返回 false）
                ModContent.ProjectileType<BossSniperRifle>(),
                NPC.damage,
                0,
                Main.myPlayer,
                NPC.whoAmI,
                (int)phase

                );
                NPC.ai[1] = 120;
            }


            if(attackType == AttackType.Melee&&CanAttatck()){
                SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
                Player targetPlayer = Main.player[NPC.target];
                float targetDirection = (targetPlayer.Center - NPC.Center).ToRotation();
                // 生成指向特定角度的 SuperSword
                Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,           // 弹幕位置
                targetDirection.ToRotationVector2()*15f,       // 速度（不需要，因为 ShouldUpdatePosition 返回 false）
                ModContent.ProjectileType<SuperSword>(),
                (int)(NPC.damage * 0.5f),             // 伤害
                0,          // 击退
                Main.myPlayer,      // 所有者
                NPC.whoAmI,
                (int)phase        // ai[0] = 目标角度（弧度）
                );
                NPC.ai[1] = 60;
            }
        }
        
        public bool CanAttatck(){
            return NPC.HasValidTarget && NPC.ai[1]<=0;
        }
         public AttackType ChooseAttackType(){
            AttackType[] attackTypes = new AttackType[] {
                AttackType.Melee,
                AttackType.Ranged,
                AttackType.Magic,
                AttackType.Summon,
                AttackType.Other
            };
            
            AttackType selectedType = attackTypes[currentAttackIndex];
            currentAttackIndex = (currentAttackIndex + 1) % attackTypes.Length;
            
            return selectedType;
        }
        
        public void CoolDown(ref float timer){
            if (timer > 0)
            {
                timer--;
            }
            if(timer<=0){
                timer = 0;
            }
        }
        public int GetSpriteDirection(NPC npc)
        {
            if (NPC.velocity.X >= 0)
            {
                return -1;  // 向右
            }
            else 
            {
                return 1; // 向左
            }
        }

        public Phase GetPhase(NPC npc)
        {
            float healthPercent = (float)npc.life / npc.lifeMax;
            
            if (healthPercent > 0.75f)
            {
                return Phase.phase1;
            }
            else if (healthPercent > 0.50f)
            {
                return Phase.phase2;
            }
            else if (healthPercent > 0.25f)
            {
                return Phase.phase3;
            }
            else
            {
                return Phase.phase4;
            }
        }
        








        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // 添加宝藏袋掉落（仅专家模式及以上）
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<BossKeleNewBag>()));
            
            // 普通模式掉落
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarryBar>(), 1, 30, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.SuperHealingPotion, 1, 15, 15));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.PlatinumCoin, 1, 2, 2));
            npcLoot.Add(notExpertRule);
        }

        public override void OnKill()
        {
            var downedBossKeleNew = ModContent.GetInstance<DownedBossKeleNew>();
            // 标记BossKele已被击败
            downedBossKeleNew.downedBossKeleNew = true;
            
            // 在多人模式下同步击败状态
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
    }
}