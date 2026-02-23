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

        public static void ToggleMode()
        {
            IsActive = !IsActive;
            _savedIsActive = IsActive;
            
            // 使用本地化的提示消息
            string messageKey = IsActive ? "Mods.ExpansionKele.Items.OtherItem.DeathOfPlace.ModeEnabled" 
                                        : "Mods.ExpansionKele.Items.OtherItem.DeathOfPlace.ModeDisabled";
            string message = Language.GetTextValue(messageKey);
            Color color = IsActive ? Color.Red : Color.Green;
            
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(message, color);
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral(message), 
                    color);
            }
            
            // 播放音效
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
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