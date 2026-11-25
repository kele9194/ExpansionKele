using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 本源近卫芯片的「元素损伤」条：挂在敌人身上，攒满后由
    /// <see cref="Content.Items.ArknightsItem.Accessories.Guard.PrimalGuardChipPlayer"/> 引爆。
    /// <para/>
    /// 条挂在 NPC 上（<c>InstancePerEntity</c>），所以是<b>队伍共享</b>的 ——
    /// 队里任何人打上去的损伤都会累积到同一条，符合「这只怪的元素损伤在涨」的语义。
    /// <para/>
    /// 上限与爆发量都按目标最大生命取比例，因此收益与目标血量、玩家伤害都无关，
    /// 不会出现「打小怪秒爆、打 Boss 永远攒不满」的偏差。
    /// </summary>
    public class PrimalGuardGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>元素相关伤害数字的统一颜色（与损伤环同色）。</summary>
        public static readonly Color TrueDamageColor = new Color(190, 130, 255);

        /// <summary>已累积的元素损伤值。</summary>
        public float ElementDamage;

        /// <summary>爆发后的剩余冷却帧数，大于 0 时不再累积损伤。</summary>
        public int BurstCooldown;

        public override void PostAI(NPC npc)
        {
            if (BurstCooldown > 0)
                BurstCooldown--;
        }

        /// <summary>
        /// 冷却期（引爆发之后）把装备档玩家的近战伤害转成真实伤害。
        /// <para/>
        /// 这里不去调 <see cref="NPC.StrikeNPC(NPC.HitInfo, bool, bool)"/>：那样得先把原伤害压到 1 再用
        /// 真伤补一遍，会出现两个数字，还得手动反推暴击倍率。直接把减免层抹掉效果完全一致，
        /// 而且暴击/易伤/FinalDamage 这些玩家增益会由框架按正确顺序算好。
        /// </summary>
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (item is null)
                return;

            TryConvertToTrueDamage(player, item.DamageType, ref modifiers);
        }

        /// <summary>弹幕路径同上；<see cref="Projectile.owner"/> 可能是 255（无主），要先挡掉。</summary>
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile is null || !projectile.active)
                return;

            int owner = projectile.owner;
            if (owner < 0 || owner >= Main.maxPlayers)
                return;

            TryConvertToTrueDamage(Main.player[owner], projectile.DamageType, ref modifiers);
        }

        /// <summary>真伤期把原版伤害数字隐掉后，用紫色重绘一次。</summary>
        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (item is null)
                return;

            ShowTrueDamageText(npc, player, item.DamageType, damageDone);
        }

        /// <inheritdoc cref="OnHitByItem"/>
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile is null || !projectile.active)
                return;

            int owner = projectile.owner;
            if (owner < 0 || owner >= Main.maxPlayers)
                return;

            ShowTrueDamageText(npc, Main.player[owner], projectile.DamageType, damageDone);
        }

        private void ShowTrueDamageText(NPC npc, Player player, DamageClass damageClass, int damageDone)
        {
            if (Main.dedServ || damageDone <= 0)
                return;

            // 不在真伤期就不画：非真伤期原版数字没被 HideCombatText 隐掉，
            // 这里再画一个紫色会变成「一刀两个数字」，让玩家以为每刀都在引爆。
            if (BurstCooldown <= 0)
                return;

            if (player is null || !player.active)
                return;

            if (!player.GetModPlayer<PrimalGuardChipPlayer>().IsEquipped)
                return;

            if (!damageClass.CountsAsClass(DamageClass.Melee))
                return;

            CombatText.NewText(npc.getRect(), TrueDamageColor, damageDone, false, false);
        }

        /// <summary>
        /// 把各类「减伤」层抹掉，但保留「增益」层：
        /// <list type="number">
        /// <item>防御 —— <c>ScalingArmorPenetration = 1</c> 完全无视（框架文档原话）</item>
        /// <item><c>SourceDamage</c> / <c>TargetDamageMultiplier</c> / <c>FinalDamage</c> —— 只把小于 1 的部分拉回 1</item>
        /// </list>
        /// <para/>
        /// 为什么不无条件归一到 1：<c>TargetDamageMultiplier</c> 里装着旗帜加成（+50%~100%），
        /// <c>FinalDamage</c> 里装着 <c>takenDamageMultiplier</c> 以及玩家的最终乘区，无条件归一等于把增益一起抹了。
        /// 只处理 &lt;1 的部分，语义就变成「真伤不低于你的正常伤害」——增益照吃，减伤无效。
        /// <para/>
        /// 残留情况：玩家增益与敌人减伤复合后若仍 &gt;1（如 1.30 × 0.8 = 1.04），无法识别减伤，
        /// 但那说明玩家增益已经把减伤抵消掉了，结果仍然不低于普通伤害。
        /// </summary>
        private void TryConvertToTrueDamage(Player player, DamageClass damageClass, ref NPC.HitModifiers modifiers)
        {
            if (BurstCooldown <= 0)
                return;

            if (player is null || !player.active)
                return;

            // 只有装备档才提供真伤期，收藏档只保留爆条
            if (!player.GetModPlayer<PrimalGuardChipPlayer>().IsEquipped)
                return;

            if (!damageClass.CountsAsClass(DamageClass.Melee))
                return;

            // 原版伤害数字是橙红色的，真伤期要换成紫色才能与普通伤害区分。
            // 隐掉之后由 OnHitBy* 重新绘制。
            modifiers.HideCombatText();

            // 防御：直接抹零
            modifiers.ScalingArmorPenetration += 1f;

            // 三个乘区：只把「减伤」（<1）拉回 1，增益（>1）原样保留
            float sourceMult = modifiers.SourceDamage.ApplyTo(1f);
            if (sourceMult > 0f && sourceMult < 1f)
                modifiers.SourceDamage *= 1f / sourceMult;

            float targetMult = modifiers.TargetDamageMultiplier.Value;
            if (targetMult > 0f && targetMult < 1f)
                modifiers.TargetDamageMultiplier /= targetMult;

            float finalFactor = modifiers.FinalDamage.ApplyTo(1f);
            if (finalFactor > 0f && finalFactor < 1f)
                modifiers.FinalDamage *= 1f / finalFactor;
        }

        /// <summary>
        /// 在敌人脚下画一条元素损伤条（带 1px 白框）。
        /// <para/>
        /// 累积期：浅紫色，长度 = 已累积 / 阈值。
        /// 冷却期：深紫色，长度随 <see cref="BurstCooldown"/> 线性退至 0，
        /// 让玩家能看出「还在真伤期，暂时打不进」。
        /// </summary>
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Main.dedServ)
                return;

            float cap = npc.lifeMax * PrimalGuardChip.ThresholdRatio;
            if (cap <= 0f)
                return;

            float ratio;
            Color fill;

            if (BurstCooldown > 0)
            {
                // 冷却：条从满逐渐退回 0，颜色变深。
                ratio = BurstCooldown / (float)PrimalGuardChip.BurstCooldownFrames;
                fill = CooldownFillColor;
            }
            else
            {
                if (ElementDamage <= 0f)
                    return;

                ratio = Math.Clamp(ElementDamage / cap, 0f, 1f);
                fill = ActiveFillColor;
            }

            ratio = Math.Clamp(ratio, 0f, 1f);
            if (ratio <= 0f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // screenPos 是 Main.screenPosition，绘制坐标要自己减掉。
            Vector2 top = npc.Top - screenPos;
            int x = (int)(top.X - BarWidth * 0.5f);
            int y = (int)top.Y - BarOffsetY;

            // 1px 白框（外围）
            spriteBatch.Draw(pixel, new Rectangle(x - 1, y - 1, BarWidth + 2, BarHeight + 2), FrameColor);

            // 空槽底色
            spriteBatch.Draw(pixel, new Rectangle(x, y, BarWidth, BarHeight), EmptyColor);

            // 填充
            int filled = (int)Math.Round(BarWidth * ratio);
            if (filled > 0)
                spriteBatch.Draw(pixel, new Rectangle(x, y, filled, BarHeight), fill);
        }

        /// <summary>损伤条宽度（px）。</summary>
        private const int BarWidth = 50;

        /// <summary>损伤条高度（px）。</summary>
        private const int BarHeight = 6;

        /// <summary>损伤条相对敌怪头顶的垂直偏移（向上）。</summary>
        private const int BarOffsetY = 10;

        private static readonly Color FrameColor = Color.White * 0.9f;
        private static readonly Color EmptyColor = new Color(28, 18, 44) * 0.85f;

        /// <summary>累积期填充色（橙色）。</summary>
        private static readonly Color ActiveFillColor = new Color(255, 140, 50);

        /// <summary>冷却期填充色（红色）。</summary>
        private static readonly Color CooldownFillColor = new Color(220, 55, 55);
    }
}
