using System;
using System.Collections.Generic;
using ExpansionKele.Content.Audio;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 开辟明日的剑刃的三种形态。
    /// </summary>
    public enum TomorrowsEdgeStage
    {
        /// <summary>本体：纯近战形态，靠 SP 充能进入一阶段。</summary>
        Melee = 0,

        /// <summary>一阶段「塔纳托斯」：贴身斩击、伤害 ×2.25、无视防御。</summary>
        Thanatos = 1,

        /// <summary>二阶段「俄耳甫斯」：无法攻击，范围治疗与范围闪避。</summary>
        Orpheus = 2
    }

    /// <summary>
    /// 开辟明日的剑刃的全部状态机：SP 充能、形态切换、替身计时、致命伤拦截与二阶段光环。
    /// <para>刻意不用 <c>Item.AltFunctionUse</c> 做右键切换 —— 框架里返回 true 会置
    /// <c>controlUseItem = true</c>（<c>Player.cs:25526</c>），必然带出一次武器使用动画。</para>
    /// <para>联机同步尚未实现：替身状态目前只在本地客户端有效，先保证单人表现。</para>
    /// </summary>
    public class TomorrowsEdgePlayer : ModPlayer
    {
        /// <summary>SP 上限：15 秒 × 60 帧。</summary>
        public const int SpMax = 15 * 60;

        /// <summary>替身总时长：一阶段与二阶段合计 25 秒。</summary>
        public const int SubstituteMax = 25 * 60;

        /// <summary>进入形态时给予的无敌帧（1 秒）。</summary>
        public const int StageInvincibleFrames = 60;

        /// <summary>替身中切走后的宽限帧数。</summary>
        public const int SwitchGraceFrames = 30;

        /// <summary>闪避成功时给予的无敌帧。</summary>
        public const int DodgeImmunityFrames = 20;

        /// <summary>一阶段攻击与二阶段光环的半径（px）。</summary>
        public const float AttackRadius = 600f;

        /// <summary>替身结束时归宿斩击的目标数上限。</summary>
        public const int FinalSlashTargets = 6;

        /// <summary>归宿斩击的伤害倍率（一阶段普攻是 2.25）。</summary>
        public const float FinalSlashDamageRatio = 6f;

        /// <summary>二阶段每秒治疗的最大生命比例。</summary>
        public const float OrpheusHealRatio = 0.05f;

        /// <summary>二阶段光环的闪避概率。</summary>
        public const float OrpheusDodgeChance = 0.3f;

        /// <summary>当前 SP（0 ~ <see cref="SpMax"/>）。</summary>
        public float Sp;

        /// <summary>当前形态。</summary>
        public TomorrowsEdgeStage Stage = TomorrowsEdgeStage.Melee;

        /// <summary>替身剩余帧数。</summary>
        public int SubstituteTimeLeft;

        /// <summary>本 SP 周期内「死亡触发一阶段」是否已用过。</summary>
        public bool DeathTriggerUsed;

        /// <summary>本 SP 周期内「一阶段 → 二阶段」是否已用过。</summary>
        public bool TransitionUsed;

        /// <summary>本帧是否处于某个二阶段光环内（含自己）。由 <see cref="PostUpdate"/> 每帧重算。</summary>
        public bool InOrpheusAura;

        /// <summary>替身中未手持本武器时的剩余宽限帧数。</summary>
        public int SwitchGrace;

        /// <summary>二阶段治疗计时（每 60 帧治疗一次）。</summary>
        public int HealTimer;

        private int _itemType = -1;
        private int _thanatosBuff = -1;
        private int _orpheusBuff = -1;
        private int _hintCooldown;

        /// <summary>本武器的物品类型。</summary>
        public int ItemType
        {
            get
            {
                if (_itemType <= 0)
                {
                    _itemType = ModContent.ItemType<TomorrowsEdge>();
                }
                return _itemType;
            }
        }

        /// <summary>一阶段契约 Buff 的类型。</summary>
        public int ThanatosBuffType
        {
            get
            {
                if (_thanatosBuff <= 0)
                {
                    _thanatosBuff = ModContent.BuffType<ThanatosBuff>();
                }
                return _thanatosBuff;
            }
        }

        /// <summary>二阶段契约 Buff 的类型。</summary>
        public int OrpheusBuffType
        {
            get
            {
                if (_orpheusBuff <= 0)
                {
                    _orpheusBuff = ModContent.BuffType<OrpheusBuff>();
                }
                return _orpheusBuff;
            }
        }

        /// <summary>当前是否手持本武器。</summary>
        public bool HoldingEdge => Player.HeldItem.type == ItemType;

        public override void PostUpdate()
        {
            if (Player.dead)
            {
                return;
            }

            bool holding = HoldingEdge;

            if (_hintCooldown > 0)
            {
                _hintCooldown--;
            }

            // 替身计时与切走宽限
            if (Stage != TomorrowsEdgeStage.Melee)
            {
                if (holding)
                {
                    SwitchGrace = SwitchGraceFrames;
                }
                else if (--SwitchGrace <= 0)
                {
                    ForceDeath();
                    return;
                }

                if (--SubstituteTimeLeft <= 0)
                {
                    ExitSubstitute();
                }

                // Buff 只负责显示：被外力清掉就按内部计时器补回来。
                SyncStageBuff();
            }

            // SP 只在「本体 + 手持」时累加；切到其他物品时暂停且不清空。
            if (Stage == TomorrowsEdgeStage.Melee && holding && Sp < SpMax)
            {
                Sp += 1f;
            }

            UpdateOrpheusAura();
            UpdateOrpheusHeal(holding);
            HandleRightClick(holding);
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            // 致命伤触发替身只要求「手持本武器」，不限制伤害来源。
            if (!HoldingEdge)
            {
                return true;
            }

            if (Stage == TomorrowsEdgeStage.Melee)
            {
                if (DeathTriggerUsed)
                {
                    return true;
                }

                DeathTriggerUsed = true;
                EnterThanatos();
                return false;
            }

            if (Stage == TomorrowsEdgeStage.Thanatos && !TransitionUsed)
            {
                EnterOrpheus();
                return false;
            }

            // 二阶段受致命伤 = 彻底死亡。
            return true;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!InOrpheusAura)
            {
                return false;
            }

            // FreeDodge 由 Player.Hurt 在 whoAmI == Main.myPlayer 时调用（Player.cs:31480），
            // 全游戏只有受击者本端执行一次，两端不会分叉 —— 不要在这里加端判断。
            if (Main.rand.NextFloat() >= OrpheusDodgeChance)
            {
                return false;
            }

            Player.immune = true;
            Player.immuneTime = DodgeImmunityFrames;
            Player.immuneNoBlink = false;

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.FireworksRGB, Main.rand.NextVector2Circular(4f, 4f), 100, new Color(190, 160, 255), 1.2f);
                dust.noGravity = true;
            }

            return true;
        }

        public override void OnRespawn()
        {
            ResetAll();
        }

        /// <summary>
        /// 收集 <paramref name="player"/> 周围半径 <see cref="AttackRadius"/> 内、有视线、且按鼠标朝向优先排序的目标。
        /// </summary>
        /// <param name="player">施法者。</param>
        /// <param name="maxCount">最多返回的目标数。</param>
        /// <returns>已按优先级排序的目标列表。</returns>
        public static List<NPC> FindTargets(Player player, int maxCount)
        {
            List<NPC> candidates = new List<NPC>();
            Vector2 aim = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy())
                {
                    continue;
                }

                if (Vector2.DistanceSquared(npc.Center, player.Center) > AttackRadius * AttackRadius)
                {
                    continue;
                }

                // 一阶段不能穿墙选敌。
                if (!Collision.CanHitLine(player.Center, 1, 1, npc.Center, 1, 1))
                {
                    continue;
                }

                candidates.Add(npc);
            }

            candidates.Sort((a, b) =>
            {
                float dotA = Vector2.Dot((a.Center - player.Center).SafeNormalize(Vector2.UnitX), aim);
                float dotB = Vector2.Dot((b.Center - player.Center).SafeNormalize(Vector2.UnitX), aim);
                return dotB.CompareTo(dotA);
            });

            if (candidates.Count > maxCount)
            {
                candidates.RemoveRange(maxCount, candidates.Count - maxCount);
            }

            return candidates;
        }

        /// <summary>进入一阶段：回满生命、1 秒无敌、SP 清空、替身计时启动。</summary>
        public void EnterThanatos()
        {
            Stage = TomorrowsEdgeStage.Thanatos;
            Sp = 0f;
            SubstituteTimeLeft = SubstituteMax;
            SwitchGrace = SwitchGraceFrames;
            HealTimer = 0;
            ClearStageBuffs();
            Player.AddBuff(ThanatosBuffType, SubstituteMax);
            RestoreLife();
            PlaySwitchEffect();
        }

        /// <summary>进入二阶段：回满生命、1 秒无敌、继承剩余替身时间。</summary>
        public void EnterOrpheus()
        {
            Stage = TomorrowsEdgeStage.Orpheus;
            TransitionUsed = true;
            HealTimer = 0;
            ClearStageBuffs();
            Player.AddBuff(OrpheusBuffType, Math.Max(1, SubstituteTimeLeft));
            RestoreLife();
            PlaySwitchEffect();
        }

        /// <summary>替身时间耗尽：回到本体，SP 从 0 重新积累。</summary>
        public void ExitSubstitute()
        {
            Stage = TomorrowsEdgeStage.Melee;
            Sp = 0f;
            SubstituteTimeLeft = 0;
            SwitchGrace = 0;
            HealTimer = 0;
            DeathTriggerUsed = false;
            TransitionUsed = false;
            ClearStageBuffs();

            SoundEngine.PlaySound(ExpansionKeleSounds.EdgeOutStage, Player.Center);

            // 退出时回满生命 + 1 秒无敌：替身结束即解绑武器，得给玩家一个窗口接住归宿斩击。
            RestoreLife();

            // 退场一击：补回替身期间被压缩的输出窗口。
            ReleaseFinalSlash();
        }

        /// <summary>
        /// 替身结束时对周围最多 <see cref="FinalSlashTargets"/> 个敌人各打一发
        /// <see cref="FinalSlashDamageRatio"/> 倍、无视防御的斩击。
        /// 倍率放在 <c>ai[0]</c> 里交给 <see cref="ThanatosSlash"/> 结算。
        /// </summary>
        private void ReleaseFinalSlash()
        {
            // 只在本机生成；其他端重复生成会导致伤害翻倍。
            if (Player.whoAmI != Main.myPlayer || !Player.active || Player.dead)
            {
                return;
            }

            Item weapon = Player.HeldItem;
            if (weapon.type != ModContent.ItemType<TomorrowsEdge>())
            {
                return;
            }

            // 退场光环：即使附近没有可打的目标，也让玩家看到这一下确实发动了。
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.FireworksRGB, velocity, 100,
                    new Color(0xD3, 0xE2, 0xFF), 1.3f);
                dust.noGravity = true;
            }

            List<NPC> targets = FindTargets(Player, FinalSlashTargets);
            if (targets.Count == 0)
            {
                return;
            }

            int slashType = ModContent.ProjectileType<ThanatosSlash>();

            // 这里传武器原始伤害，倍率由 ThanatosSlash.ModifyHitNPC 统一乘。
            int damage = Player.GetWeaponDamage(weapon);

            foreach (NPC target in targets)
            {
                if (!target.active)
                {
                    continue;
                }

                Projectile.NewProjectile(Player.GetSource_FromThis(), target.Center, Vector2.Zero,
                    slashType, damage, weapon.knockBack, Player.whoAmI,
                    FinalSlashDamageRatio, target.whoAmI);
            }
        }

        private void ResetAll()
        {
            Stage = TomorrowsEdgeStage.Melee;
            Sp = 0f;
            SubstituteTimeLeft = 0;
            SwitchGrace = 0;
            HealTimer = 0;
            DeathTriggerUsed = false;
            TransitionUsed = false;
            InOrpheusAura = false;
            _hintCooldown = 0;
            ClearStageBuffs();
        }

        private void ClearStageBuffs()
        {
            Player.ClearBuff(ThanatosBuffType);
            Player.ClearBuff(OrpheusBuffType);
        }

        /// <summary>
        /// 把契约 Buff 的剩余时间对齐内部计时器。
        /// Buff 只是显示载体：被外力清掉时补回来，但不能反过来决定替身是否结束。
        /// </summary>
        private void SyncStageBuff()
        {
            if (Stage == TomorrowsEdgeStage.Melee)
            {
                return;
            }

            int activeBuff = Stage == TomorrowsEdgeStage.Thanatos ? ThanatosBuffType : OrpheusBuffType;
            int remaining = Math.Max(1, SubstituteTimeLeft);

            int index = Player.FindBuffIndex(activeBuff);
            if (index < 0)
            {
                Player.AddBuff(activeBuff, remaining);
                return;
            }

            Player.buffTime[index] = remaining;
        }

        private void RestoreLife()
        {
            Player.statLife = Player.statLifeMax2;
            Player.immune = true;
            Player.immuneTime = StageInvincibleFrames;
            Player.immuneNoBlink = true;
        }

        private void ForceDeath()
        {
            ResetAll();
            // 已经清过状态，这里再锁一次，保证 PreKill 不会把它当成「死亡触发一阶段」。
            DeathTriggerUsed = true;
            Player.statLife = 0;
            Player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(Language.GetTextValue("Mods.ExpansionKele.Items.Weapons.TomorrowsEdge.DeathReason", Player.name))), 9999, 0);
        }

        private void PlaySwitchEffect()
        {
            // 本体→一阶段、一阶段→二阶段都用这一声。
            SoundEngine.PlaySound(ExpansionKeleSounds.EdgeTurnStage, Player.Center);

            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.FireworksRGB, velocity, 100, new Color(190, 160, 255), 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        private void HandleRightClick(bool holding)
        {
            if (Player.whoAmI != Main.myPlayer || !holding)
            {
                return;
            }

            // 鼠标在 UI 上、或本帧右键已被方块 / 智能交互吃掉时，不当作形态切换。
            if (Player.mouseInterface || Player.tileInteractionHappened || Main.SmartInteractShowingGenuine)
            {
                return;
            }

            if (!PlayerInput.Triggers.JustPressed.MouseRight)
            {
                return;
            }

            switch (Stage)
            {
                case TomorrowsEdgeStage.Melee:
                    if (Sp >= SpMax)
                    {
                        EnterThanatos();
                    }
                    else
                    {
                        ShowHint("Mods.ExpansionKele.Items.Weapons.TomorrowsEdge.HintSpNotFull");
                    }
                    break;

                case TomorrowsEdgeStage.Thanatos:
                    if (!TransitionUsed)
                    {
                        EnterOrpheus();
                    }
                    break;

                case TomorrowsEdgeStage.Orpheus:
                    // 二阶段不允许切回，右键无效。
                    break;
            }
        }

        private void UpdateOrpheusAura()
        {
            InOrpheusAura = false;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || other.dead)
                {
                    continue;
                }

                if (other.GetModPlayer<TomorrowsEdgePlayer>().Stage != TomorrowsEdgeStage.Orpheus)
                {
                    continue;
                }

                if (Vector2.DistanceSquared(other.Center, Player.Center) > AttackRadius * AttackRadius)
                {
                    continue;
                }

                // 光环不能穿墙。
                if (!Collision.CanHitLine(other.Center, 1, 1, Player.Center, 1, 1))
                {
                    continue;
                }

                InOrpheusAura = true;
                return;
            }
        }

        private void UpdateOrpheusHeal(bool holding)
        {
            if (Stage != TomorrowsEdgeStage.Orpheus || !holding)
            {
                HealTimer = 0;
                return;
            }

            if (++HealTimer < 60)
            {
                return;
            }

            HealTimer = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || other.dead)
                {
                    continue;
                }

                if (other.whoAmI != Player.whoAmI && !IsTeammate(other))
                {
                    continue;
                }

                if (Vector2.DistanceSquared(other.Center, Player.Center) > AttackRadius * AttackRadius)
                {
                    continue;
                }

                // 治疗同样不能穿墙。
                if (!Collision.CanHitLine(other.Center, 1, 1, Player.Center, 1, 1))
                {
                    continue;
                }

                int heal = (int)(other.statLifeMax2 * OrpheusHealRatio);
                if (heal <= 0)
                {
                    continue;
                }

                other.statLife = Math.Min(other.statLifeMax2, other.statLife + heal);
                other.HealEffect(heal, true);
            }
        }

        private bool IsTeammate(Player other)
        {
            return Player.team != 0 && other.team == Player.team;
        }

        private void ShowHint(string key)
        {
            if (_hintCooldown > 0)
            {
                return;
            }

            _hintCooldown = 45;
            CombatText.NewText(Player.Hitbox, new Color(190, 160, 255), Language.GetTextValue(key));
        }
    }
}
