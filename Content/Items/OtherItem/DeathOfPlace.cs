using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class DeathOfPlace : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        
        public override void SetStaticDefaults()
        {
            // 使用本地化文件中的条目
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.autoReuse = false;
            Item.consumable = false; // 不消耗
            Item.UseSound = SoundID.Item1;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.maxStack = 1;
        }

        // 删除AltFunctionUse方法，不再需要右键功能
        /*
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        */

        public override bool? UseItem(Player player)
        {
            // 改为左键点击切换（默认使用）
            DeathOfPlaceSystem.ToggleMode();
            return true;
        }

        // 删除ModifyTooltips方法，改为使用本地化文件
        /*
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
        */

        public override void AddRecipes()
        {

        }
    }

    [Autoload(Side = ModSide.Both)]
    public class DeathOfPlaceSystem : ModSystem
    {
        public static bool IsActive { get; private set; } = false;
        private static bool _savedIsActive = false;


        /// <summary>自定义包类型。两端必须一致，只能追加，不要调序。</summary>
        public enum MessageType : byte
        {
            /// <summary>客户端 -> 服务端：请求翻转模式。</summary>
            ToggleRequest,

            /// <summary>服务端 -> 客户端：同步翻转后的状态（bool = IsActive）。</summary>
            StateChanged,
        }

        /// <summary>
        /// 由物品使用入口调用。
        /// <para>
        /// 多人下客户端只发翻转请求 —— 本地直接翻转只会让点的人自己进入模式，
        /// 服务端和其他玩家都不知道。真正翻转由服务端完成后再广播回来。
        /// </para>
        /// </summary>
        public static void ToggleMode()
        {
            // 音效不等服务端往返，本地立即播
            if (!Main.dedServ)
                Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                WritePacket(MessageType.ToggleRequest).Send();
                return;
            }

            ApplyToggle();
            Announce();
            BroadcastState();
        }

        /// <summary>服务端收到翻转请求：校验来源持有该物品后翻转并广播。</summary>
        public static void OnToggleRequested(int whoAmI)
        {
            // 校验来源确实手持该物品，避免伪造包随意切换世界模式
            if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
                return;

            if (Main.player[whoAmI].HeldItem.type != ModContent.ItemType<DeathOfPlace>())
                return;

            ApplyToggle();
            Announce();
            BroadcastState();
        }

        /// <summary>客户端收到状态同步：只改状态，提示由服务端广播聊天统一发出。</summary>
        public static void OnStateReceived(bool active)
        {
            IsActive = active;
            _savedIsActive = active;
        }

        private static void ApplyToggle()
        {
            IsActive = !IsActive;
            _savedIsActive = IsActive;
        }

        private static void Announce()
        {
            string messageKey = IsActive
                ? "Mods.ExpansionKele.Items.OtherItem.DeathOfPlace.ModeEnabled"
                : "Mods.ExpansionKele.Items.OtherItem.DeathOfPlace.ModeDisabled";
            Color color = IsActive ? Color.Red : Color.Green;
            string message = Language.GetTextValue(messageKey);

            if (Main.netMode == NetmodeID.Server)
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral(message),
                    color);
            }
            else
            {
                Main.NewText(message, color);
            }
        }

        private static void BroadcastState()
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            ModPacket packet = WritePacket(MessageType.StateChanged);
            packet.Write(IsActive);
            packet.Send();
        }

        private static ModPacket WritePacket(MessageType type)
        {
            ModPacket packet = ModContent.GetInstance<DeathOfPlaceSystem>().Mod.GetPacket();
            packet.Write((byte)type);
            return packet;
        }

        public override void OnWorldLoad()
        {
            // 恢复之前保存的状态
            IsActive = _savedIsActive;
        }

        public override void OnWorldUnload()
        {
            // 保存当前状态
            _savedIsActive = IsActive;
        }

        // 保存数据到世界文件
        public override void SaveWorldData(TagCompound tag)
        {
            tag["deathOfPlaceActive"] = IsActive;
        }

        // 从世界文件加载数据
        public override void LoadWorldData(TagCompound tag)
        {
            IsActive = tag.ContainsKey("deathOfPlaceActive") ? tag.GetBool("deathOfPlaceActive") : false;
            _savedIsActive = IsActive;
        }
    }

    public class DeathOfPlacePlayer : ModPlayer
    {
        // 删除闪避相关代码，改用ModifyHitByXXX实现减伤
        
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!DeathOfPlaceSystem.IsActive) return;
            
            // 90%概率完全免疫伤害
            if (Main.rand.NextFloat() < 0.9f)
            {
                modifiers.FinalDamage *= 0f;
                modifiers.Knockback *= 0f;
                CreateImmuneEffect(Player.Center);
            }
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (!DeathOfPlaceSystem.IsActive) return;
            
            // 90%概率完全免疫伤害
            if (Main.rand.NextFloat() < 0.9f)
            {
                modifiers.FinalDamage *= 0f;
                modifiers.Knockback *= 0f;
                CreateImmuneEffect(Player.Center);
            }
        }

        private void CreateImmuneEffect(Vector2 position)
        {
            // 创建免疫特效
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustPerfect(position, DustID.Ghost, 
                    Main.rand.NextVector2Circular(4f, 4f), 
                    Scale: 1.8f);
            }
        }

        public override void PostUpdate()
        {
            // 添加持续的视觉效果
            if (DeathOfPlaceSystem.IsActive && Main.rand.NextBool(10))
            {
                Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(25f, 25f), 
                    DustID.Ghost, Scale: 1.2f);
            }
        }
    }

    public class DeathOfPlaceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // 添加标记防止重复处理
        private bool _damageAlreadyReduced = false;

        public override void ResetEffects(NPC npc)
        {
            _damageAlreadyReduced = false;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 只对敌对NPC生效，并防止重复削减
            if (DeathOfPlaceSystem.IsActive && !npc.friendly && !_damageAlreadyReduced && Main.rand.NextFloat() < 0.9f)
            {
                _damageAlreadyReduced = true;
                modifiers.FinalDamage *= 0f;
                modifiers.Knockback *= 0f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // 只对敌对NPC生效，并防止重复削减
            if (DeathOfPlaceSystem.IsActive && !npc.friendly && !_damageAlreadyReduced && Main.rand.NextFloat() < 0.9f)
            {
                _damageAlreadyReduced = true;
                modifiers.FinalDamage *= 0f;
                modifiers.Knockback *= 0f;
            }
        }

        public override void PostAI(NPC npc)
        {
            // 为敌对NPC添加视觉效果
            if (DeathOfPlaceSystem.IsActive && !npc.friendly && Main.rand.NextBool(15))
            {
                Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width/2f, npc.height/2f), 
                    DustID.Ghost, Scale: 1.0f);
            }
        }
    }

}