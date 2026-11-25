using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 让「标了 <see cref="SyncAttribute"/> 的字段自动跨端同步」对<b>全部弹幕</b>生效。
    /// <para>
    /// 因为它是 GlobalProjectile，TML 会对每个弹幕都调用它
    /// （<c>ProjectileLoader.WriteExtraAI</c> → <c>HookSendExtraAI.Enumerate</c>），
    /// 所以具体的 <c>ModProjectile</c> 类<b>不需要</b>覆写 SendExtraAI / ReceiveExtraAI。
    /// </para>
    /// <para>
    /// 没有同步字段的弹幕（含全部原版弹幕）会写 0 字节，
    /// TML 侧的 <c>bytes.Length == 1</c> 判空会直接跳过发包，因此没有网络开销。
    /// 唯一成本是每帧一次的 <see cref="NetSerializer.HasSync"/> 字典查找。
    /// </para>
    /// </summary>
    public class AutoSyncProjectile : GlobalProjectile
    {
        /// <inheritdoc/>
        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter writer)
        {
            if (projectile.ModProjectile is { } modProjectile && NetSerializer.HasSync(modProjectile.GetType()))
                NetSerializer.Write(modProjectile, writer);
        }

        /// <inheritdoc/>
        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader reader)
        {
            if (projectile.ModProjectile is { } modProjectile && NetSerializer.HasSync(modProjectile.GetType()))
                NetSerializer.Read(modProjectile, reader);
        }
    }

    /// <summary>NPC 版的自动同步，机制与 <see cref="AutoSyncProjectile"/> 相同。</summary>
    public class AutoSyncNPC : GlobalNPC
    {
        /// <inheritdoc/>
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
        {
            if (npc.ModNPC is { } modNpc && NetSerializer.HasSync(modNpc.GetType()))
                NetSerializer.Write(modNpc, writer);
        }

        /// <inheritdoc/>
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
        {
            if (npc.ModNPC is { } modNpc && NetSerializer.HasSync(modNpc.GetType()))
                NetSerializer.Read(modNpc, reader);
        }
    }

    /// <summary>
    /// 物品版的自动同步，走 <c>GlobalItem.NetSend / NetReceive</c> 通道。
    /// <para>
    /// 注意：<c>ItemIO.SendModData</c> 只会遍历「覆写了 NetSend 的 GlobalItem」，
    /// 本类覆写之后即满足该条件。
    /// </para>
    /// </summary>
    public class AutoSyncItem : GlobalItem
    {
        /// <inheritdoc/>
        public override void NetSend(Item item, BinaryWriter writer)
        {
            if (item.ModItem is { } modItem && NetSerializer.HasSync(modItem.GetType()))
                NetSerializer.Write(modItem, writer);
        }

        /// <inheritdoc/>
        public override void NetReceive(Item item, BinaryReader reader)
        {
            if (item.ModItem is { } modItem && NetSerializer.HasSync(modItem.GetType()))
                NetSerializer.Read(modItem, reader);
        }
    }
}
