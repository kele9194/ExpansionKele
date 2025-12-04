using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class SeismicBlastDetonator : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        // 物品的激活状态
        public bool IsActivated = false;
        
        // 添加一个字段来跟踪是否已经触发过爆炸
        public bool HasDetonated = false;

        public override void SetDefaults()
        {
            //Item.SetNameOverride("震波雷管");
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1; // 无法堆叠
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        // ... existing code ...
        public override void UpdateInventory(Player player)
        {
            var modPlayer = player.GetModPlayer<SeismicBlastDetonatorPlayer>();
            modPlayer.HasSeismicBlastDetonator = true;
            modPlayer.DetonatorActivated = IsActivated;
            modPlayer.Detonator = this;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                IsActivated = !IsActivated;
                HasDetonated = false; // 重置爆炸状态
                if (IsActivated)
                {
                    Main.NewText("震波雷管已激活！死亡时将引发大爆炸。", Color.Red);
                }
                else
                {
                    Main.NewText("震波雷管已关闭。", Color.Green);
                }
            }
            return true;
        }
// ... existing code ...

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipData = new Dictionary<string, string>
            {
                {"Description1", "由五个雷管组成的高能爆炸装置"},
                {"Description2", "使用后可在激活模式和待机模式间切换"},
                {"Status", IsActivated ? "当前状态：激活模式" : "当前状态：待机模式"},
                {"Effect1", "激活模式下死亡时会引发大爆炸"},
                {"Effect2", "爆炸将杀死半径1000格内所有玩家"},
                {"Effect3", "对生命值低于12000的敌人直接秒杀"},
                {"Effect4", "对生命值高于12000的敌人造成百分比伤害"},
                {"Formula", "伤害公式：(20+80*(12000/h)^0.5)%最大生命值"},
                {"intro","在使用这个之前最好和其他人说好，这个是实验品，比较混乱"}
            };

            foreach (var kvp in tooltipData)
            {
                tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Dynamite, 5) // 使用5个雷管合成
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    // ... existing code ...
    public class SeismicBlastDetonatorPlayer : ModPlayer
    {
        public bool HasSeismicBlastDetonator = false;
        public bool DetonatorActivated = false;
        public SeismicBlastDetonator Detonator = null;
        public bool HasDetonatedInThisLife = false;

        public override void ResetEffects()
        {
            // 检查玩家是否拥有任何激活的震波雷管
            bool hasActivatedDetonator = false;
            SeismicBlastDetonator activatedDetonator = null;
            
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                Item item = Player.inventory[i];
                if (item.ModItem is SeismicBlastDetonator detonator && detonator.IsActivated)
                {
                    hasActivatedDetonator = true;
                    activatedDetonator = detonator;
                    break;
                }
            }
            
            HasSeismicBlastDetonator = hasActivatedDetonator;
            DetonatorActivated = hasActivatedDetonator;
            Detonator = activatedDetonator;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            // 玩家死亡时检查是否拥有激活的震波雷管
            if (HasSeismicBlastDetonator && DetonatorActivated && Detonator != null && !Detonator.HasDetonated)
            {
                Detonator.HasDetonated = true;
                TriggerSeismicBlast();
            }
        }

        private void TriggerSeismicBlast()
        {
            // 播放爆炸音效
            SoundEngine.PlaySound(SoundID.Item14, Player.Center);
            
            // 创建爆炸视觉效果
            for (int i = 0; i < 100; i++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
            }

            // 获取玩家位置作为爆炸中心
            Vector2 explosionCenter = Player.Center;
            
            // 杀死范围内所有玩家（包括自己，但自己已经死了）
            foreach (Player player in Main.player)
            {
                if (player.active && player.whoAmI != Player.whoAmI)
                {
                    float distance = Vector2.Distance(explosionCenter, player.Center);
                    if (distance <= 1000 * 16) // 1000格范围
                    {
                        player.KillMe(PlayerDeathReason.ByCustomReason(player.name + "被震波雷管的爆炸吞噬"), 9999, 0);
                    }
                }
            }

            // 对NPC造成伤害
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(explosionCenter, npc.Center);
                    if (distance <= 1000 * 16) // 1000格范围
                    {
                        if (npc.lifeMax <= 12000)
                        {
                            // 直接秒杀生命值低于12000的敌人
                            npc.life = 0;
                            npc.HitEffect(0, 10.0);
                            npc.active = false;
                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, npc.whoAmI, -1);
                            }
                        }
                        else
                        {
                            // 对生命值高于12000的敌人造成百分比伤害
                            float h = npc.lifeMax;
                            float damagePercent = (20 + 80 * (float)System.Math.Pow(12000 / h, 0.5f)) / 100f;
                            int damage = (int)(npc.lifeMax * damagePercent);

                            npc.StrikeNPC(new NPC.HitInfo() { Damage = damage, Knockback = 0f, HitDirection = 0 });
                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, npc.whoAmI, damage);
                            }
                        }
                    }
                }
            }

            // 消耗已激活的震波雷管 - 优先消耗位置靠前的已激活雷管
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                Item item = Player.inventory[i];
                if (item.ModItem is SeismicBlastDetonator detonator && detonator.IsActivated)
                {
                    // 标记为已引爆
                    detonator.HasDetonated = true;
                    
                    // 消耗物品
                    item.stack--;
                    if (item.stack <= 0)
                        item.TurnToAir();
                    break;
                }
            }

            // 显示爆炸消息
            if (Main.netMode == NetmodeID.Server)
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral("震波雷管爆炸了！"), Color.Red);
            }
            else
            {
                Main.NewText("震波雷管爆炸了！", Color.Red);
            }
        }
    }
// ... existing code ...
    
}