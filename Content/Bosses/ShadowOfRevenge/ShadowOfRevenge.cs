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
using System.Configuration;
using Microsoft.Xna.Framework.Graphics;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Terraria.Localization;
using ReLogic.Content;

namespace ExpansionKele.Content.Bosses.ShadowOfRevenge
{
    // 自动加载Boss头像
    [AutoloadBossHead]
    public class ShadowOfRevenge : ModNPC
    {
        public override string LocalizationCategory => "Bosses.ShadowOfRevenge";
        public static LocalizedText SpawnCondition { get; private set; }
        public static string texturePath = $"ExpansionKele/Content/Bosses/ShadowOfRevenge/ShadowOfRevengeMoon";
        private Asset<Texture2D> _cachedTextureMoon;
        
        public override void Load()
        {
            // 预加载纹理
            _cachedTextureMoon=ModContent.Request<Texture2D>(texturePath);
        }
        
        public override void Unload()
        {
            // 可选：清空引用
            _cachedTextureMoon = null;
        }

        // 定义Boss的攻击类型枚举（暂时留空，后续添加）
        public enum ShadowOfRevengeAttackType
        {
            Die = -2,
            Phase1Intro,
            // TODO: 添加更多攻击类型
        }

        // 使用NPC.localAI[0]存储当前攻击阶段索引
        public ref float Phase => ref NPC.ai[0];

        // 移动逻辑计时器数组 (8个元素)
        private short[] MovingAITimer = new short[8];
        
        // 技能计时器数组 (8个元素)
        private short[] SkillAITimer = new short[8];
                // Skill4状态数组 (用于存储技能4的状态信息)
        // Skill4Phase[0] 存储技能阶段状态 (0=未激活, 1=加速阶段, 2=减速阶段)
        private byte[] Skill4Phase = new byte[1];
        // Skill4Float[0] 存储当前速度值
        // Skill4Float[1] 存储初始距离到目标
        private float[] Skill4Float = new float[2];
        private float moonRingRotation = 0f;


        // 封装 Timer 和 SubTimer 属性以实现类型安全访问

        // 不同模式下的基础生命值设定
        // ... existing code ...
        // 不同模式下的基础生命值设定
        private int NormalHP = ExpansionKele.ATKTool(27000, 40000);
        // private const int ExpertHP = 50000;
        // private const int MasterHP = 75000;

        // 不同模式下的基础防御值设定
        private int NormalDefense = ExpansionKele.ATKTool(15, 25);
        private int ExpertDefense = ExpansionKele.ATKTool(15, 25);
        private int MasterDefense = ExpansionKele.ATKTool(15, 25);
// ... existing code ...
        // 发光效果相关变量
        private const int FadeOutDuration = 10;
        private const int FadeInDuration = 10;
        private const int TeleportInterval = 250; // 300帧瞬移一次

        private const int OrbitProjectileCount = 6; // 环绕的投射物数量

        private  int Skill1damage=ExpansionKele.ATKTool(40, 50);
        private  int Skill2damage=ExpansionKele.ATKTool(40, 50);
        private  int Skill3damage=ExpansionKele.ATKTool(40, 50);

        
        // 静态默认设置（在游戏加载时调用一次）
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1; // 设置为单帧纹理
            NPCID.Sets.BossBestiaryPriority.Add(NPC.type); // 添加到图鉴优先级列表
            SpawnCondition = this.GetLocalization("SpawnCondition");
            
            // 指定Boss是否免疫某些Debuff
            // NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        // 直接设置NPC造成防御损伤
        if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
        {
            calamity.Call("SetDefenseDamageNPC", NPC, true);
        }
    }

        // 每次生成NPC时调用，用于初始化NPC属性
        // ... existing code ...
        // 每次生成NPC时调用，用于初始化NPC属性
        public override void SetDefaults()
        {
            // 基础尺寸和碰撞箱设置 (按用户要求设置为68*90)
            NPC.width = 136;
            NPC.height = 180;
            
            // 攻击伤害与防御属性
            NPC.damage = 60;
            NPC.defense = NormalDefense;

            // 生命值根据游戏模式调整
            NPC.lifeMax = NormalHP;
            // if (Main.masterMode)
            // {
            //     NPC.lifeMax = MasterHP;
                NPC.defense = MasterDefense;
            // }
            // else if (Main.expertMode)
            // {
            //     NPC.lifeMax = ExpertHP;
                NPC.defense = ExpertDefense;
            // }

            // 音效与物理属性
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            
            // AI和移动属性
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            
            // Boss相关设置
            NPC.boss = true;
            NPC.npcSlots = 10f; // 占用10个NPC槽位
            
            // 其他属性
            NPC.value = Item.buyPrice(0, 5, 0, 0); // 击败后获得的钱币价值
            NPC.scale = 1f; // 缩放比例
            
            // 战斗AI设置
            NPC.aiStyle = -1; // 使用自定义AI
            
            // 发光效果
            // NPC.alpha = 0;
        }
// ... existing code ...

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            // // 多人游戏时的生命值调整
            // NPC.lifeMax = (int)(NPC.lifeMax * bossAdjustment * 1.1f);
            // NPC.damage = (int)(NPC.damage * 1.2f);
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            // Boss可以击中玩家
            return true;
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            // 修改悬停框以匹配视觉外观
            boundingBox = NPC.Hitbox;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            // 绘制Boss血条
            return true;
        }
        public override void OnKill()
        {
            // 标记Shadow of Revenge Boss已被击败
            DownedShadowOfRevengeBoss.downedShadowOfRevenge = true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // 添加宝藏袋掉落（仅限专家模式）
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<ShadowOfRevengeTreasureBag>()));
            
            // 在经典模式下直接掉落奖励物品
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<ShadowOfRevengeTreasureBag>()));
            
            // 添加非专家模式下的常规掉落
            LeadingConditionRule rule = new LeadingConditionRule(new Conditions.NotExpert());
            // 添加30-40个FullMoonBar
            rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<FullMoonBar>(), 1, 30, 40));
            // 添加10-20瓶治疗药水
            rule.OnSuccess(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 10, 20));
            // 添加70金币
            rule.OnSuccess(ItemDropRule.Common(ItemID.GoldCoin, 1, 70, 70));
            
            npcLoot.Add(rule);
        }
        
                public override void SendExtraAI(BinaryWriter writer)
        {
            // 发送移动逻辑计时器数组
            for (int i = 0; i < MovingAITimer.Length; i++)
            {
                writer.Write(MovingAITimer[i]);
            }
            
            // 发送技能计时器数组
            for (int i = 0; i < SkillAITimer.Length; i++)
            {
                writer.Write(SkillAITimer[i]);
            }
            
            // 发送Skill4状态数组
            for (int i = 0; i < Skill4Phase.Length; i++)
            {
                writer.Write(Skill4Phase[i]);
            }
            
            // 发送Skill4浮点数组
            for (int i = 0; i < Skill4Float.Length; i++)
            {
                writer.Write(Skill4Float[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            // 接收移动逻辑计时器数组
            for (int i = 0; i < MovingAITimer.Length; i++)
            {
                MovingAITimer[i] = reader.ReadInt16();
            }
            
            // 接收技能计时器数组
            for (int i = 0; i < SkillAITimer.Length; i++)
            {
                SkillAITimer[i] = reader.ReadInt16();
            }
            
            // 接收Skill4状态数组
            for (int i = 0; i < Skill4Phase.Length; i++)
            {
                Skill4Phase[i] = reader.ReadByte();
            }
            
            // 接收Skill4浮点数组
            for (int i = 0; i < Skill4Float.Length; i++)
            {
                Skill4Float[i] = reader.ReadSingle();
            }
        }
        
        // ... existing code ...
        #region MovingAI  
        // ... existing code ...
        // ... existing code ...
        public override void AI()
        {
            if (CheckAllPlayersDead())
            {
                NPC.active = false;
                return;
            }
            // 计算当前血量百分比
            float lifeRatio = (float)NPC.life / NPC.lifeMax;
            
            // 根据血量确定阶段
            int phase = 1;
            if (lifeRatio <= 0.6f) phase = 2;
            if (lifeRatio <= 0.25f) phase = 3;
            
            // 检查阶段变化并播放吼叫声音
            if ((int)Phase != phase)
            {
                // 阶段发生变化
                Phase = phase;
                
                // 播放吼叫声音
                switch (phase)
                {
                    case 2:
                        // 进入二阶段
                        SoundEngine.PlaySound(SoundID.Roar, NPC.position);
                        break;
                    case 3:
                        // 进入三阶段
                        SoundEngine.PlaySound(SoundID.ForceRoar, NPC.position);
                        break;
                }
            }
            
            // 维持环绕投射物 (所有阶段都保持)
            
            
            // 根据阶段执行不同行为
            switch (phase)
            {
                case 1:
                    Phase1AI();
                    // 在一阶段使用技能
                    UseSkillPhase(1,UseSkill2,0);
                    UseSkillPhase(1,UseSkill3,1);
                    break;
                case 2:
                    Phase2AI();
                    // 在二阶段直接使用技能2，间隔60帧
                    UseSkillPhase(3,UseSkill2,0);
                    UseSkillPhase(3,UseSkill3,1);
                    UpdateOrbitProjectiles();
                    break;
                case 3:
                    Phase3AI();
                    // 在三阶段使用技能3，间隔60帧
                    UseSkillPhase(2,UseSkill2,0);
                    UseSkillPhase(2,UseSkill3,1);
                    UseSkillPhase(1,UseSkill5,2);
                    break;
            }
            
            // 添加发光效果
            Lighting.AddLight(NPC.Center, 1f, 0.2f, 0.8f);
        }
        
        private void Phase1AI()
        {
            // 阶段1: 每300帧瞬移一次
            MovingAITimer[0]++; // 使用移动计时器数组的第一个元素
            
            if (MovingAITimer[0] <= TeleportInterval)
            {
                // 正常移动逻辑 - 简单的悬停
                NPC.velocity *= 0.95f;
            }
            else if (MovingAITimer[0] <= TeleportInterval + FadeOutDuration)
            {
                // 淡出阶段
                int fadeOutElapsed = MovingAITimer[0] - TeleportInterval;
                NPC.alpha = (int)(255f * fadeOutElapsed / FadeOutDuration);
                NPC.velocity = Vector2.Zero;
            }
            else if (MovingAITimer[0] <= TeleportInterval + FadeOutDuration + FadeInDuration)
            {
                // 淡入阶段
                int fadeInElapsed = MovingAITimer[0] - TeleportInterval - FadeOutDuration;
                NPC.alpha = (int)(255f - 255f * fadeInElapsed / FadeInDuration);
                
                // 在淡入阶段完成瞬移
                if (fadeInElapsed == 1)
                {
                    TeleportToRandomPlayer();
                }
                
                NPC.velocity = Vector2.Zero;
            }
            else
            {
                // 重置计时器
                MovingAITimer[0] = 0;
                NPC.alpha = 0;
            }
        }
        
        // ... existing code ...
        private void Phase2AI()
        {
            Player target = GetNearestPlayer();
            if (target == null) return;
            
            float maxSpeed = 4f; // 移动速度上限变为60
            // 转向速度解除限制
            
            // 计算目标方向
            Vector2 directionToTarget = target.Center - NPC.Center;
            directionToTarget.Normalize();
            
            // 直接设置速度朝向目标（无转向限制）
            NPC.velocity = directionToTarget * maxSpeed;
// ... existing code ...
        }
// ... existing code ...
        
        // ... existing code ...
        private void Phase3AI()
        {
            // 阶段3: 血量低于25%，激怒状态
            // 屏幕变红效果由其他系统处理，这里只处理移动
            
            Player target = GetNearestPlayer();
            if (target == null) return;
            
            float maxSpeed = 8f; // 移动速度上限变为60
            // 转向速度解除限制
            
            // 计算目标方向
            Vector2 directionToTarget = target.Center - NPC.Center;
            directionToTarget.Normalize();
            
            // 直接设置速度朝向目标（无转向限制）
            NPC.velocity = directionToTarget * maxSpeed;
        }
// ... existing code ...
        
        // ... existing code ...
        private void TeleportToRandomPlayer()
        {
            // 找到一个随机玩家
            Player target = GetNearestPlayer();
            if (target == null) return;
            
            // 计算随机偏移 (-400 到 400 像素)
            float offsetX = Main.rand.NextFloat(-300f, 300f);
            float offsetY = -300f; // 上方400像素
            
            // 计算新的位置
            Vector2 newPosition = target.Center + new Vector2(offsetX, offsetY);
            
            // 更新位置，使贴图中心与目标位置对齐
            NPC.position = newPosition - new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
            
            // 重置速度
            NPC.velocity = Vector2.Zero;
        }
// ... existing code ...
        
        private Player GetNearestPlayer()
        {
            Player nearestPlayer = null;
            float minDistance = float.MaxValue;
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    float distance = Vector2.Distance(NPC.Center, player.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestPlayer = player;
                    }
                }
            }
            
            return nearestPlayer;
        }

        private bool CheckAllPlayersDead()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                // 如果有任何一个玩家处于活跃且未死亡状态，则返回false
                if (player.active && !player.dead)
                {
                    return false;
                }
            }
            // 所有玩家都已死亡或不活跃
            return true;
        }
        #endregion
        #region Skills

        // ... existing code ...
        private void UpdateOrbitProjectiles()
        {
            // 查找现有的环绕投射物
            int orbitProjCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<BossMoonProj1>() && proj.ai[1] == NPC.whoAmI)
                {
                    orbitProjCount++;
                }
            }
            
            // 创建缺少的环绕投射物
            while (orbitProjCount < OrbitProjectileCount)
            {
                // 计算初始角度
                float initialAngle = MathHelper.TwoPi / OrbitProjectileCount * orbitProjCount;
                
                // 创建投射物
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center, // 从Boss中心生成
                    Vector2.Zero,
                    ModContent.ProjectileType<BossMoonProj1>(),
                    Skill1damage, // 伤害值
                    0,
                    Main.myPlayer,
                    initialAngle, // ai[0] 角度
                    NPC.whoAmI   // ai[1] Boss索引
                );
                
                orbitProjCount++;
            }
        }
        private void UseSkill2()
        {
            // 技能2：向锁定距离最近玩家的位置，60帧后向该位置射出三发月亮弹幕
            // 直接调用技能2的发射部分
            Player target = GetNearestPlayer();
            if (target != null)
            {
                Vector2 direction = target.Center - NPC.Center;
                direction.Normalize();
                
                float speed = 18f;
                
                // 计算一个目标点，距离Boss一定距离
                Vector2 targetPos = NPC.Center + direction * 300f;
                
                // 中间投射物 (0度)
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    direction * speed,
                    ModContent.ProjectileType<BossMoonProj2>(),
                    Skill2damage, // 伤害值
                    0,
                    Main.myPlayer,
                    0f, // ai[0]
                    targetPos.X, // ai[1]
                    targetPos.Y  // localAI[0]
                );
                
                // 左侧投射物 (+7.5度)
                Vector2 leftDir = direction.RotatedBy(MathHelper.ToRadians(7.5f));
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    leftDir * speed,
                    ModContent.ProjectileType<BossMoonProj2>(),
                    Skill2damage, // 伤害值
                    0,
                    Main.myPlayer,
                    0f, // ai[0]
                    targetPos.X, // ai[1]
                    targetPos.Y  // localAI[0]
                );
                
                // 右侧投射物 (-7.5度)
                Vector2 rightDir = direction.RotatedBy(MathHelper.ToRadians(-7.5f));
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    rightDir * speed,
                    ModContent.ProjectileType<BossMoonProj2>(),
                    Skill2damage, // 伤害值
                    0,
                    Main.myPlayer,
                    0f, // ai[0]
                    targetPos.X, // ai[1]
                    targetPos.Y  // localAI[0]
                );
            }
        }
        
       private void UseSkill3()
        {
            // 技能3：向随机方向发射三个月亮弹幕，这些月亮弹幕发射后10帧开始追踪玩家
            for (int i = 0; i < 3; i++)
            {
                // 随机方向
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                float speed = 15f;
                
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center, // 从Boss中心发射
                    direction * speed,
                    ModContent.ProjectileType<BossMoonProj3>(),
                    Skill3damage, // 伤害值
                    0,
                    Main.myPlayer
                );
            }
        }
        private void UseSkill5()
        {
            float angle =15f;
            // 技能5：向360度方向均发射7.5度间隔的弹幕，从正上方0度开始，其余属性参考Skill2
            float angleStep = MathHelper.ToRadians(angle); // 7.5度转换为弧度
            int totalProjectiles = (int)(360/angle); // 360/7.5 = 48个弹幕
            float speed = 8f;
            Vector2 direction = new Vector2(0, -1); // 从正上方开始(0度方向)

            for (int i = 0; i < totalProjectiles; i++)
            {
                // 计算当前角度
                float currentAngle = angleStep * i;
                
                // 旋转方向向量
                Vector2 projectileDirection = direction.RotatedBy(currentAngle);
                
                // 计算一个目标点，距离Boss一定距离
                Vector2 targetPos = NPC.Center + projectileDirection * 300f;
                
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    projectileDirection * speed,
                    ModContent.ProjectileType<BossMoonProj2>(),
                    Skill2damage, // 伤害值与Skill2相同
                    0,
                    Main.myPlayer,
                    0f, // ai[0]
                    targetPos.X, // ai[1]
                    targetPos.Y  // localAI[0]
                );
            }
        }
                private void UseSkill4()
        {
            // 技能4：瞄准一位最近的玩家，并标记位置，确定boss与玩家之间的距离t，
            // 并确定冲刺距离为max（1.2*t,200），冲刺速度为25
            Player target = GetNearestPlayer();
            if (target == null) return;

            // Skill4Phase[0] 存储技能阶段状态 (0=未激活, 1=冲刺阶段)
            // Skill4Float[0] 存储当前冲刺距离
            // Skill4Float[1] 存储总冲刺距离
            
            const byte DASH_PHASE = 1;
            const float DASH_SPEED = 25f;
            
            if (Skill4Phase[0] == 0)
            {
                // 初始化冲撞技能
                Skill4Phase[0] = DASH_PHASE; // 设置为冲刺阶段
                
                // 计算Boss与玩家之间的距离（只在初始化时计算一次）
                Vector2 toTarget = target.Center - NPC.Center;
                float distanceToTarget = toTarget.Length();
                
                // 确定冲刺距离为max（1.2*t,200）（只在初始化时确定）
                Skill4Float[1] = Math.Max(1.2f * distanceToTarget, 200f);
                Skill4Float[0] = 0f; // 已冲刺距离初始化为0
                
                // 计算冲向玩家的方向
                Vector2 direction = toTarget;
                direction.Normalize();
                
                // 设置冲刺速度
                NPC.velocity = direction * DASH_SPEED;
            }
            else if (Skill4Phase[0] == DASH_PHASE)
            {
                // 更新已冲刺距离
                Skill4Float[0] += DASH_SPEED;
                
                // 检查是否已完成冲刺
                if (Skill4Float[0] >= Skill4Float[1])
                {
                    // 冲刺完成，重置状态
                    NPC.velocity = Vector2.Zero;
                    Skill4Phase[0] = 0; // 重置状态
                }
                // 如果未完成冲刺，保持当前速度
                // 不重新计算方向或距离，确保冲刺直线进行
            }
        }
        #endregion
        #region skill use
        // ... existing code ...
        // ... existing code ...
        private int PhaseCoolDown(int phase, int mul = 1){
            return phase switch
            {
                1 => (int)(125*mul),
                2 => (int)(80*mul),
                3 => (int)(50*mul),
                _ => 0
            };
        }

        private void UseSkillPhase(int phase, Action skill, int timerIndex)
        {
            // 根据阶段使用指定技能，间隔取决于PhaseCoolDown返回的帧数
            SkillAITimer[timerIndex]++;
            if (SkillAITimer[timerIndex] >= PhaseCoolDown(phase))
            {
                skill();
                SkillAITimer[timerIndex] = 0; // 重置计时器
            }
        }

        private void UseSkill2Phase(int phase)
        {
            // 根据阶段使用技能2，间隔取决于PhaseCoolDown返回的帧数
            UseSkillPhase(phase, UseSkill2, 3);
        }

        private void UseSkill3Phase(int phase)
        {
            // 根据阶段使用技能3，间隔取决于PhaseCoolDown返回的帧数
            UseSkillPhase(phase, UseSkill3, 3);
        }
// ... existing code ...
// ... existing code ...

        #endregion
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
        Texture2D moonRingTexture = _cachedTextureMoon.Value;
            
            // 每帧增加2度旋转角度
            moonRingRotation += MathHelper.ToRadians(2f);
            
            // 确保角度在0-2π范围内
            moonRingRotation = moonRingRotation % MathHelper.TwoPi;
            
            // 计算月光环的位置（在Boss中心向上20像素）
            Vector2 moonRingPosition = NPC.Center + new Vector2(0, -20);
            
            // 计算月光环的屏幕位置
            Vector2 moonRingScreenPos = moonRingPosition - screenPos;
            
            // 计算月光环的原点（中心点）
            Vector2 moonRingOrigin = new Vector2(moonRingTexture.Width / 2, moonRingTexture.Height / 2);
            
            // 绘制月光环贴图在Boss背后（在正常NPC绘制之前）
            spriteBatch.Draw(
                moonRingTexture,
                moonRingScreenPos,
                null, // source rectangle
                drawColor, // color
                moonRingRotation, // rotation
                moonRingOrigin, // origin
                1f, // scale
                SpriteEffects.None, // effects
                0f // layer depth (0为最底层)
            );
            
            return true; // 继续正常绘制Boss
        }

        
    }
}