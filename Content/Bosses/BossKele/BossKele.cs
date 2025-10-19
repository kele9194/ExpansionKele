// 引入必要的命名空间
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Microsoft.Xna.Framework.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using Terraria.Audio;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Bosses.BossKele
{
    // 自动加载Boss头像
    [AutoloadBossHead]
    public class BossKele : ModNPC
    {
        public override string LocalizationCategory=>"Bosses.BossKele";
        // 定义Boss的攻击类型枚举
        public enum BossKeleAttackType
        {
            Die = -2,
            Phase1Intro,
            ColorfulOrbAttack,
            RedLaserWarning,
            RotatingCrossLaser,
            ChargeAttack
        }

        // BossKele.cs 文件中，位于类定义开始处添加如下字段

private float baseOrbitDistance;
private float orbitAngleOffset;
private float orbitDirection;
private float orbitSpeed;
private float orbitEccentricity;
private float orbitRotation;

        // 使用NPC.localAI[0]存储当前攻击阶段索引
         public ref float AttackIndex => ref NPC.localAI[0];
        public ref float Phase => ref NPC.localAI[1];

        // 定义 Timer 和 SubTimer 的索引位置
        private const int TimerIndex = 0;
        private const int SubTimerIndex = 1;

        // 封装 Timer 和 SubTimer 属性以实现类型安全访问
        public int Timer
        {
            get => (int)NPC.ai[TimerIndex];
            set => NPC.ai[TimerIndex] = value;
        }

        public int SubTimer
        {
            get => (int)NPC.ai[SubTimerIndex];
            set => NPC.ai[SubTimerIndex] = value;
        }


        // 不同模式下的基础生命值设定
        private const int NormalHP = 150000;
        private const int ExpertHP = 249600;
        private const int MasterHP = 309401;

        // 不同模式下的基础防御值设定
        private const int NormalDefense = 50;
        private const int ExpertDefense = 65;
        private const int MasterDefense = 80;

        // 静态默认设置（在游戏加载时调用一次）
        public override void SetStaticDefaults()
{
    Main.npcFrameCount[NPC.type] = 1; // 设置为单帧纹理
    NPCID.Sets.BossBestiaryPriority.Add(NPC.type); // 添加到图鉴优先级列表
}

//         s'sss

        // 每次生成NPC时调用，用于初始化NPC属性
        public override void SetDefaults()
{
    // 基础尺寸和碰撞箱设置
    NPC.width = 50;
    NPC.height = 70;

    // 攻击伤害与防御属性
    NPC.damage = 60;
    NPC.defense = NormalDefense;

    // 生命值根据游戏模式调整
    NPC.lifeMax = NormalHP;
    if (Main.masterMode)
    {
        NPC.lifeMax = MasterHP;
        NPC.defense = MasterDefense;
    }
    else if (Main.expertMode)
    {
        NPC.lifeMax = ExpertHP;
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

    // 初始无敌帧设置
    NPC.dontTakeDamage = true; // 开启无敌状态
    Timer = 0; // 初始化计时器
}


        // 图鉴信息设置
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            // {
            //     new FlavorTextBestiaryInfoElement("Mods.FargowiltasSouls.Bestiary.BossKele"),
            //     Biomes.Surface
            // });
        }

        // 网络数据同步 - 发送额外AI信息
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(AttackIndex);
            writer.Write(Phase);
            writer.Write7BitEncodedInt(Timer);
            writer.Write7BitEncodedInt(SubTimer);
        }
        // 网络数据同步 - 接收额外AI信息
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AttackIndex = reader.ReadSingle();
            Phase = reader.ReadSingle();
            Timer = reader.Read7BitEncodedInt();
            SubTimer = reader.Read7BitEncodedInt();
        }
        // 当NPC生成时调用
        public override void OnSpawn(IEntitySource source)
{
    if (!Main.dedServ && (Main.netMode == NetmodeID.SinglePlayer || Main.myPlayer == NPC.target))
    {
        Timer = 0;
        AttackIndex = (int)BossKeleAttackType.Phase1Intro;
    }
}

        // AI逻辑主函数
        // AI逻辑主函数
        // AI逻辑主函数
// 处理Boss的移动逻辑// AI逻辑主函数
public override void AI()
{
    Player target = Main.player[NPC.target]; // 获取目标玩家

    // 如果所有玩家都死亡，触发Boss清理逻辑
    if (AllPlayersDead())
    {
        Timer++; // 启动倒计时

        if (Timer >= 240) // 等待10秒（60帧/秒 * 10秒 = 600帧）
        {
            NPC.active = false; // 移除Boss
        }

        return; // 跳过其他AI逻辑
    }

    // 如果目标无效或死亡，尝试重新锁定
    if (!target.active || target.dead)
    {
        NPC.TargetClosest(true);
        target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.velocity.Y -= 0.04f; // 缓慢上升
            return;
        }
    }

    Timer++; // 主计时器递增

    // 控制初始无敌状态持续5帧
    if (Timer == 5)
    {
        NPC.dontTakeDamage = false; // 关闭无敌状态
        
        // 初始化随机轨道参数
        InitializeRandomOrbit(target);
    }

    // 每600帧重新选择最近的玩家并可能改变轨道参数
    if (Timer % 600 == 0)
    {
        NPC.TargetClosest(true);
        target = Main.player[NPC.target];
        
        // 有可能改变轨道参数
        if (Main.rand.NextBool(3)) // 1/3概率改变轨道距离
        {
            InitializeRandomOrbit(target);
        }
    }

    // 根据当前攻击阶段执行对应逻辑
    switch ((BossKeleAttackType)(int)AttackIndex)
    {
        case BossKeleAttackType.Phase1Intro:
            if (Timer == 1)
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center); // 播放咆哮音效

            if (Timer >= 120)
                ChooseNextAttack(); // 120帧后选择下一个攻击阶段

            break;

        case BossKeleAttackType.ColorfulOrbAttack:
            if (Timer % 30 == 0)
                SpawnColorfulOrbs(target.Center); // 每30帧发射彩色球形弹幕

            if (Timer >= 180)
                ChooseNextAttack(); // 180帧后选择下一个攻击阶段

            break;

        case BossKeleAttackType.RedLaserWarning:
            if (Timer == 1)
                //DrawLaserWarningLine(NPC.Center, target.Center, Color.Red);
                PrepareRedLaser(target); // 初始化红色激光预警

            if (Timer >= 120)
            {
                PrepareRedLaser(target); // 发射红色激光
                DrawLaserWarningLine(NPC.Center, target.Center, Color.Red);
                ChooseNextAttack(); // 120帧后选择下一个攻击阶段
            }

            break;

        case BossKeleAttackType.RotatingCrossLaser:
            if (Timer == 1)
                SoundEngine.PlaySound(SoundID.Item12, NPC.Center); // 播放准备音效

            if (Timer <= 300)
                RotateLasers(1.5f); // 每帧旋转1.5度，持续300帧

            if (Timer >= 360)
                ChooseNextAttack(); // 360帧后选择下一个攻击阶段

            break;

        case BossKeleAttackType.Die:

            if (Timer >= 120)
                NPC.active = false; // 120帧后移除NPC

            break;
            
        case BossKeleAttackType.ChargeAttack: // 新增的冲刺攻击
            if (Timer == 1)
                SoundEngine.PlaySound(SoundID.Item12, NPC.Center); // 播放准备音效
            
            if (Timer < 60)
            { 
                // 前60帧蓄力，缓慢接近玩家
                Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = direction * 5f; // 缓慢接近
            }
            else if (Timer == 60)
            {
                // 开始冲刺
                Vector2 chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = chargeDirection * 20f; // 快速冲向玩家
            }
            else if (Timer > 60 && Timer < 90)
            {
                // 维持冲刺速度
                Vector2 chargeDirection = NPC.velocity.SafeNormalize(Vector2.Zero);
                NPC.velocity = chargeDirection * 20f;
            }
            else if (Timer >= 90)
            {
                // 结束冲刺，恢复正常移动
                ChooseNextAttack();
            }
            
            break;
    }
    
    // 调用独立的移动逻辑
    HandleMovement(target);
}

// 新增方法：检查所有玩家是否死亡
private bool AllPlayersDead()
{
    for (int i = 0; i < Main.maxPlayers; i++)
    {
        Player player = Main.player[i];
        if (player != null && player.active && !player.dead)
        {
            return false; // 至少有一个玩家还活着
        }
    }
    return true; // 所有玩家都死亡
}

// 初始化随机轨道参数
private void InitializeRandomOrbit(Player target)
{
    // 随机基础轨道距离（200-300像素）
    baseOrbitDistance = Main.rand.NextFloat(250f, 350f);
    
    // 随机起始角度
    orbitAngleOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
    
    // 随机绕转方向（顺时针或逆时针）
    orbitDirection = Main.rand.NextBool() ? 1f : -1f;
    
    // 随机轨道速度（0.02-0.05之间）
    orbitSpeed = Main.rand.NextFloat(0.01f, 0.03f);
    
    // 随机轨道偏心率（椭圆效果）
    orbitEccentricity = Main.rand.NextFloat(0.7f, 0.9f);
    
    // 随机轨道旋转角度（用于椭圆旋转）
    orbitRotation = Main.rand.NextFloat(0f, MathHelper.PiOver2);
}

// 处理Boss的移动逻辑
private void HandleMovement(Player target)
{
    float maxDistance = 600f; // 最大距离阈值
    if (Vector2.Distance(NPC.Center, target.Center) > maxDistance)
    {
        // 随机生成160像素范围内的偏移量
        float randomAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
        float teleportDistance = 160f; // 固定瞬移距离
        
        // 计算目标位置
        Vector2 teleportPosition = target.Center + new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * teleportDistance;
        NPC.position = teleportPosition - new Vector2(NPC.width / 2, NPC.height / 2); // 瞬移至玩家附近的随机方向
        NPC.velocity = Vector2.Zero; // 停止移动
    }
    // 计算椭圆轨道
    float angle = orbitAngleOffset + orbitDirection * orbitSpeed * Timer;
    float adjustedDistance = baseOrbitDistance * (1f + orbitEccentricity * (float)Math.Sin(2 * (angle - orbitRotation)));
    
    Vector2 orbitCenter = target.Center;
    Vector2 newPosition = orbitCenter + new Vector2((float)Math.Cos(angle) * adjustedDistance, (float)Math.Sin(angle) * adjustedDistance);

    NPC.velocity = (newPosition - NPC.Center) * 0.1f; // 平滑移动至目标轨道位置
    // 在 HandleMovement 方法中添加
    NPC.spriteDirection = (target.Center.X < NPC.Center.X) ? 1 : -1;
    
}

// 随机选择下一个攻击阶段
private void ChooseNextAttack()
{
    int[] possibleAttacks = { 
        (int)BossKeleAttackType.ColorfulOrbAttack, 
        (int)BossKeleAttackType.RedLaserWarning, 
        (int)BossKeleAttackType.RotatingCrossLaser,
        (int)BossKeleAttackType.ChargeAttack // 添加冲刺攻击选项
    };
    AttackIndex = possibleAttacks[Main.rand.Next(possibleAttacks.Length)]; // 随机选择
    Timer = 0; // 重置计时器
}

private void SpawnColorfulOrbs(Vector2 targetPos)
{
    for (int i = 0; i < 4; i++)
    {
        Vector2 velocity = (targetPos - NPC.Center).SafeNormalize(Vector2.Zero) * 10f; // 计算朝向目标的速度
        int damage = 60; // 根据难度缩放伤害

        // 使用新的彩色弹幕
        int type = ModContent.ProjectileType<BossKeleColorfulOrb>();

        // 偏移发射点位置以避免与Boss碰撞
        Vector2 spawnPosition = NPC.Center + velocity * 20f; // 将发射点向外偏移20像素

        // 创建弹幕并设置为非友好类型
        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPosition, velocity, type, damage, 0f, Main.myPlayer);
        Main.projectile[proj].friendly = false;
        Main.projectile[proj].hostile = true;
    }
}

// 发射红色激光攻击
private void PrepareRedLaser(Player target)
{
    // 计算从Boss到玩家的归一化方向向量
    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);

    // 偏移发射点位置以避免与Boss碰撞
    Vector2 spawnPosition = NPC.Center + direction * 20f; // 将发射点向外偏移20像素

    // 设置弹幕伤害值
    int damage = 60;

    // 使用新的红色激光弹幕
    int type = ModContent.ProjectileType<BossKeleRedLaser>();

    // 发射高伤害直线激光
    Projectile.NewProjectile(
        NPC.GetSource_FromThis(),
        spawnPosition,
        direction * 10f,
        type,
        damage,
        0f,
        Main.myPlayer
    );
}

// 旋转激光攻击逻辑
private void RotateLasers(float rotationSpeed)
{
    // 每隔一定帧发射一道激光
    if (Timer % 30 == 0)
    {
        float angle = MathHelper.ToRadians(Timer * rotationSpeed); // 计算当前角度
        Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)); // 方向向量

        // 偏移发射点位置以避免与Boss碰撞
        Vector2 spawnPosition = NPC.Center + direction * 20f; // 将发射点向外偏移20像素

        int damage = 70; // 根据难度缩放伤害
        
        // 使用新的旋转激光弹幕
        int type = ModContent.ProjectileType<BossKeleRotatingLaser>();

        Projectile.NewProjectile(
            NPC.GetSource_FromThis(),
            spawnPosition,
            direction * 15f,
            type,
            damage,
            0f,
            Main.myPlayer
        );
    }
}

private void DrawLaserWarningLine(Vector2 start, Vector2 end, Color color, int dustType = 60)
{
    const int MaxDust = 500;
    float distance = Vector2.Distance(start, end);
    Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);

    for (int i = 0; i < distance / 4f && i < MaxDust; i++)
    {
        Vector2 position = start + direction * i * 4f;
        Dust dust = Dust.NewDustDirect(position - Vector2.One * 4f, 8, 8, dustType, 0f, 0f, 100, color, 1.5f);
        dust.noGravity = true;
        dust.velocity *= 0.1f;
    }
}

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // 添加宝藏袋掉落（仅专家模式及以上）
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<BossKeleBag>()));
            
            // 普通模式掉落
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarryBar>(), 1, 30, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.SuperHealingPotion, 1, 15, 15));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.PlatinumCoin, 1, 2, 2));
            npcLoot.Add(notExpertRule);
        }
    }
}