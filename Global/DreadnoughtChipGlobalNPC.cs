using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 无畏者芯片的「附近只剩一个敌人」增伤。
    /// <para/>
    /// 只影响装备者自己造成的近战伤害：改的是本次命中的 <c>HitModifiers</c>，
    /// 不写 <c>npc.defense</c>、不碰 NPC 状态，所以队友与其他伤害来源完全不受影响。
    /// <para/>
    /// 圆心是<b>玩家</b>而不是弹幕落点 —— 芯片描述的是「我身边只剩一个敌人」，
    /// 与这一击飞了多远无关，所以近战弹幕武器（泰拉刃一类）同样适用。
    /// <para/>
    /// 「标记最近的敌人」不需要单独找最近：圈内只剩一个时，它必然就是离玩家最近的那个。
    /// </summary>
    public class DreadnoughtChipGlobalNPC : GlobalNPC
    {
        /// <summary>标记在敌人头顶的额外向上偏移（像素）。</summary>
        private const float TagExtraOffset = 20f;

        private static Texture2D _tagTexture;

        /// <summary>标记贴图（懒加载；热重载后会自动重新请求）。</summary>
        private static Texture2D TagTexture
        {
            get
            {
                if (_tagTexture == null || _tagTexture.IsDisposed)
                {
                    _tagTexture = ModContent.Request<Texture2D>(
                        "ExpansionKele/Global/DreadnoughtChipTag",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }

                return _tagTexture;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // 直接命中正常只来自近战武器，仍做一次分类判定，避免以后别的直接伤害类型混进来。
            if (!item.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Apply(npc, player, ref modifiers);
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 无主弹幕的 owner 是 255，取 Main.player 之前必须先挡掉非玩家来源。
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            // 只认近战弹幕；召唤、魔法、远程都不触发。
            if (!projectile.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Apply(npc, Main.player[projectile.owner], ref modifiers);
        }

        /// <summary>命中的目标正是「被标记的那一个」时，按档位写入额外最终伤害。</summary>
        private static void Apply(NPC npc, Player player, ref NPC.HitModifiers modifiers)
        {
            if (!player.active || player.dead || player.ghost)
                return;

            DreadnoughtChipPlayer modPlayer = player.GetModPlayer<DreadnoughtChipPlayer>();
            int level = modPlayer.Level;
            if (level == 0)
                return;

            // 圈内出现第二个敌人时这里返回 -1，标记连带加成一起失效。
            if (DreadnoughtChipPlayer.FindMarkedTargetIndex(player) != npc.whoAmI)
                return;

            float bonus = level >= 3
                ? DreadnoughtChip.EquippedSoleBonus
                : DreadnoughtChip.FavoritedSoleBonus;

            modifiers.FinalDamage *= 1f + bonus;
        }

        /// <summary>
        /// 在「被标记的那一个」头顶画倒三角。
        /// <para/>
        /// 位置 = NPC 中心上方「半高 + 20 像素」，取自 RAc 阻挡标记的同一套标定。
        /// </summary>
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                return;

            // 标记索引每帧由本地玩家的 ModPlayer 算一次，这里只做比较，避免逐 NPC 扫全场。
            if (Main.LocalPlayer.GetModPlayer<DreadnoughtChipPlayer>().MarkedTargetWhoAmI != npc.whoAmI)
                return;

            Texture2D texture = TagTexture;
            if (texture == null)
                return;

            Vector2 worldPos = npc.Center + new Vector2(0f, -npc.height / 2f - TagExtraOffset);

            spriteBatch.Draw(
                texture,
                worldPos - screenPos,
                null,
                Color.White,
                0f,
                new Vector2(texture.Width / 2f, texture.Height / 2f),
                1f,
                SpriteEffects.None,
                0f);
        }
    }
}
