using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class TrackingLocator : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override void SetDefaults()
        {
            //Item.SetNameOverride("回溯定位仪");
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
        }

        public override void UpdateInventory(Player player)
        {
            var modPlayer = player.GetModPlayer<TrackingLocatorPlayer>();
            modPlayer.IsTrackingLocatorInInventory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipData = new Dictionary<string, string>
            {
                //{ "Defense", $"防御力 +{Item.defense}" },
				{"GenericDamageBonus", $"回溯到1s前的位置，速度方向和血量，按下你设置的快捷键进行回溯"},
				//{"Tooltip",$"星元套装的第一个系列的头盔"}
            };

            foreach (var kvp in tooltipData)
            {
                tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Compass, 1)
                .AddIngredient(ItemID.DepthMeter, 1)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }

    public class TrackingLocatorPlayer : ModPlayer
    {
        // 存储玩家状态数据，最多保存60帧的数据
        public Queue<PlayerStateData> StateHistory = new Queue<PlayerStateData>();
        public bool IsTrackingLocatorInInventory = false;
        public bool CanTeleport => StateHistory.Count >= 60;

        public override void ResetEffects()
        {
            // 每帧重置标记
            IsTrackingLocatorInInventory = false;
        }

        public override void PostUpdate()
        {
            // 如果物品在背包中，则更新追踪数据
            if (IsTrackingLocatorInInventory)
            {
                UpdateTrackingData(Player);
            }
            else
            {
                // 如果物品不在背包中，清空历史记录
                StateHistory.Clear();
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // 检查是否按下了Y键（需要在ModSystem中注册按键绑定）
            if (ExpansionKele.TrackingKeyBind.JustPressed)
            {
                TryTeleport();
            }
        }

        public void UpdateTrackingData(Player player)
        {
            // 添加当前帧的数据到队列
            StateHistory.Enqueue(new PlayerStateData
            {
                Position = player.position,
                Velocity = player.velocity,
                Life = player.statLife,
                Direction = player.direction,
                GravityDirection = player.gravDir
            });

            // 如果队列超过60帧的数据，则移除最旧的数据
            if (StateHistory.Count > 60)
            {
                StateHistory.Dequeue();
            }
        }

        private void TryTeleport()
        {
            // 只有当有足够的历史数据时才能回溯
            if (CanTeleport && IsTrackingLocatorInInventory)
            {
                // 获取60帧前的状态数据
                PlayerStateData pastState = null;
                
                // 创建临时列表来访问队列中的元素
                var tempList = new List<PlayerStateData>(StateHistory);
                if (tempList.Count >= 60)
                {
                    pastState = tempList[0]; // 获取最旧的数据（60帧前）
                }

                // 应用60帧前的状态
                if (pastState != null)
                {
                    Player.Teleport(pastState.Position);
                    Player.velocity = pastState.Velocity;
                    Player.statLife = pastState.Life;
                    Player.direction = pastState.Direction;
                    Player.gravDir = pastState.GravityDirection;
                    
                    // 视觉效果
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.Teleporter);
                    }
                }
            }
        }
    }

    public class PlayerStateData
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public int Life;
        public int Direction;
        public float GravityDirection;
    }
}