using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 重射手（HeavyShooter）天赋的弹幕处理类。
    /// 效果：
    /// 1. 攻击力 ×1.3
    /// 2. 近身距离增伤（400px → 240px 线性插值，最高 ×1.25）
    /// 3. 角度判定增伤（夹角 &lt; 45° 额外 ×1.05）
    /// </summary>
    public class HeavyShooterGlobalProjectile : GlobalProjectile
    {
        // 记录每个弹幕的来源武器和玩家（key = projectile.whoAmI）
        private static readonly Dictionary<int, (Item item, Player player)> projectileSourceMap
            = new Dictionary<int, (Item, Player)>();

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem statsSource)
            {
                Item sourceItem = statsSource.Item;
                Player owner = statsSource.Player;

                projectileSourceMap[projectile.whoAmI] = (sourceItem, owner);

                // 快速检查：如果不是重射手，直接移除记录
                int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                if (arkNum == 0) return;
                if ((SniperSubclass)arkNum != SniperSubclass.HeavyShooter)
                {
                    projectileSourceMap.Remove(projectile.whoAmI);
                }
            }
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!TryGetHeavyShooterInfo(projectile, out _, out Player owner))
                return;

            // ---- 效果1：攻击力 ×1.3 ----
            modifiers.FinalDamage *= 1.3f;

            // ---- 效果2：近身距离增伤 ----
            float dist = Vector2.Distance(owner.Center, target.Center);
            if (dist <= 240f)
            {
                // ≤ 240px：×1.25
                modifiers.FinalDamage *= 1.25f;
            }
            else if (dist <= 400f)
            {
                // 240 ~ 400px：线性插值 ×1.0 → ×1.25
                float t = (400f - dist) / (400f - 240f); // 1.0 at 240px, 0.0 at 400px
                float distanceMultiplier = 1f + t * 0.25f;
                modifiers.FinalDamage *= distanceMultiplier;
            }
            // > 400px：无距离增伤

            // ---- 效果3：角度判定增伤 ----
            // 仅客户端计算鼠标方向
            if (Main.netMode != NetmodeID.Server)
            {
                // 玩家鼠标方向
                Vector2 mouseDir = Main.MouseWorld - owner.Center;
                // 目标方向
                Vector2 targetDir = target.Center - owner.Center;

                if (mouseDir != Vector2.Zero && targetDir != Vector2.Zero)
                {
                    float mouseAngle = mouseDir.ToRotation();
                    float targetAngle = targetDir.ToRotation();
                    float angleDiff = Math.Abs(MathHelper.WrapAngle(targetAngle - mouseAngle));

                    // 夹角 < 45°（π/4）时触发 ×1.05
                    if (angleDiff < MathHelper.PiOver4)
                    {
                        modifiers.FinalDamage *= 1.1f;
                    }
                }
            }
        }

        public override void OnKill(Projectile projectile, int timeleft)
        {
            projectileSourceMap.Remove(projectile.whoAmI);
        }

        public override void Unload()
        {
            projectileSourceMap.Clear();
        }

        /// <summary>
        /// 尝试获取弹幕的重射手刻印信息。如果任何校验失败，返回 false。
        /// </summary>
        private static bool TryGetHeavyShooterInfo(Projectile projectile, out Item sourceItem, out Player owner)
        {
            sourceItem = null;
            owner = null;

            if (projectile == null || !projectile.active)
                return false;

            if (!projectileSourceMap.TryGetValue(projectile.whoAmI, out var sourceInfo))
                return false;

            sourceItem = sourceInfo.item;
            if (sourceItem == null || sourceItem.IsAir)
                return false;

            owner = sourceInfo.player ?? Main.player[projectile.owner];
            if (owner == null || !owner.active)
                return false;

            int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
            if (arkNum == 0)
                return false;

            if ((SniperSubclass)arkNum != SniperSubclass.HeavyShooter)
                return false;

            return true;
        }
    }

    /// <summary>
    /// 重射手（HeavyShooter）天赋的玩家效果处理类。
    /// 效果：
    /// - 攻速 ×0.75 惩罚
    /// - 切武器攻速爆发（切换后5秒内约150%递减至0%，冷却20秒）
    /// </summary>
    public class HeavyShooterPlayer : ModPlayer
    {
        /// <summary>玩家当前手持的武器是否为重射手刻印</summary>
        public bool HasHeavyShooterWeaponEquipped = false;

        // ---- 切武器爆发字段 ----

        /// <summary>上一帧手持物品的 type，用于检测武器切换</summary>
        private int lastHeldItemType = ItemID.None;

        /// <summary>爆发剩余时间（帧数，60帧=1秒）</summary>
        private int swapBurstTimer = 0;

        /// <summary>爆发冷却时间（帧数）</summary>
        private int swapBurstCooldown = 0;

        /// <summary>爆发持续总时长（5秒 = 300帧）</summary>
        private const int SwapBurstDuration = 300;

        /// <summary>爆发冷却总时长（20秒 = 1200帧）</summary>
        private const int SwapBurstCooldownDuration = 1200;

        /// <summary>爆发最大加速倍率（×2= 100%加速）</summary>
        private const float SwapBurstMaxMultiplier =2f;

        public override void ResetEffects()
        {
            HasHeavyShooterWeaponEquipped = false;
        }

        public override void PostUpdate()
        {
            // ⚠ 服务器跳过
            if (Main.dedServ) return;

            Item heldItem = Player.HeldItem;

            // ---- 检测武器切换（在递减计时器之前触发） ----
            DetectWeaponSwap(heldItem);

            // ---- 判断当前武器是否重射手 ----
            HasHeavyShooterWeaponEquipped = IsHeavyShooterWeapon(heldItem);

            // ---- 记录旧值，用于检测状态变化 ----
            int prevBurstTimer = swapBurstTimer;
            int prevCooldown = swapBurstCooldown;

            // ---- 递减爆发和冷却计时器 ----
            if (swapBurstTimer > 0)
                swapBurstTimer--;

            // 🔁 冷却时间只有在手持重射手武器时才会递减
            if (swapBurstCooldown > 0 && HasHeavyShooterWeaponEquipped)
                swapBurstCooldown--;

            // ---- 爆发结束通知 ----
            if (prevBurstTimer > 0 && swapBurstTimer == 0)
            {
                string text = Language.GetTextValue(
                    "Mods.ExpansionKele.Items.Chips.HeavyShooterChip.SwapBurstEnd");
                CombatText.NewText(Player.getRect(), Color.Gray, text, true);
            }

            // ---- 冷却就绪通知 ----
            if (prevCooldown > 0 && swapBurstCooldown == 0)
            {
                string text = Language.GetTextValue(
                    "Mods.ExpansionKele.Items.Chips.HeavyShooterChip.SwapBurstCooldownEnd");
                CombatText.NewText(Player.getRect(), Color.LimeGreen, text, true);
            }

            // 更新上一帧物品
            lastHeldItemType = heldItem?.type ?? ItemID.None;
        }

        /// <summary>
        /// 检测玩家是否切换到了新的重射手武器，触发攻速爆发。
        /// </summary>
        private void DetectWeaponSwap(Item currentItem)
        {
            // 没有手持物品或上一帧也没有 → 不处理
            if (currentItem == null || currentItem.IsAir)
                return;

            int currentType = currentItem.type;
            if (currentType == lastHeldItemType)
                return; // 没有切换武器

            // 检查新武器是否为重射手刻印
            if (!IsHeavyShooterWeapon(currentItem))
                return;

            // 检查冷却
            if (swapBurstCooldown > 0)
                return;

            // 触发爆发！
            swapBurstTimer = SwapBurstDuration;
            swapBurstCooldown = SwapBurstCooldownDuration;

            // 爆发开始通知（本地化）
            string startText = Language.GetTextValue(
                "Mods.ExpansionKele.Items.Chips.HeavyShooterChip.SwapBurstStart");
            CombatText.NewText(Player.getRect(), Color.OrangeRed, startText, true);
        }

        /// <summary>
        /// 攻速修正：基础惩罚 ×0.75 + 爆发加速（随时间递减）
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            float multiplier = 1f;

            // 只对重射手武器生效
            if (IsHeavyShooterWeapon(item))
            {
                // 基础攻速惩罚 ×0.75
                multiplier *= 0.75f;

                // 爆发加速（如果有）
                if (swapBurstTimer > 0)
                {
                    // 线性递减：从 ×1.5 到 ×1.0
                    float t = swapBurstTimer / (float)SwapBurstDuration; // 1.0 → 0.0
                    float burstBonus = 1f + (SwapBurstMaxMultiplier - 1f) * t; // 1.5 → 1.0
                    multiplier *= burstBonus;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// 判断指定物品是否为重射手刻印武器。
        /// </summary>
        private static bool IsHeavyShooterWeapon(Item item)
        {
            if (item == null || item.IsAir)
                return false;

            int arkNum = ArknightsGlobalItem.GetArknightsNum(item);
            if (arkNum == 0)
                return false;

            return (SniperSubclass)arkNum == SniperSubclass.HeavyShooter;
        }
    }
}