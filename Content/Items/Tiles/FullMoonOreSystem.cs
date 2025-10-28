using System.Collections.Generic;
using System.Threading;
using ExpansionKele.Content.Bosses.ShadowOfRevenge;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace ExpansionKele.Content.Items.Tiles
{
	public class FullMoonOreSystem : ModSystem
	{
		public static LocalizedText BlessedWithFullMoonOreMessage { get; private set; }
		public static bool oreGenerated = false;

        public override void PreWorldGen() {
			// 在生成新世界时将oreGenerated设为false
			oreGenerated = false;
		}

		public override void SetStaticDefaults() {
			BlessedWithFullMoonOreMessage = Mod.GetLocalization($"WorldGen.{nameof(BlessedWithFullMoonOreMessage)}");
		}

		public override void PostUpdateWorld() {
			// 检查是否已经生成过矿石
			if (DownedShadowOfRevengeBoss.downedShadowOfRevenge && !oreGenerated) {
				var fullMoonOreSystem = ModContent.GetInstance<FullMoonOreSystem>();
				fullMoonOreSystem.BlessWorldWithFullMoonOre();
				oreGenerated = true;
			}
		}

		// 在击败Boss后调用此方法生成更多满月矿石
		public void BlessWorldWithFullMoonOre() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return; // 客户端不应该执行此操作
			}

			// 由于这是在游戏过程中发生的，我们需要在另一个线程上运行此代码以避免游戏卡顿
			ThreadPool.QueueUserWorkItem(_ => {
				// 广播消息通知玩家
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(BlessedWithFullMoonOreMessage.Value, 100, 200, 255);
				}
				else if (Main.netMode == NetmodeID.Server) {
					ChatHelper.BroadcastChatMessage(BlessedWithFullMoonOreMessage.ToNetworkText(), new Color(100, 200, 255));
				}

				// 控制生成多少矿石团，根据世界大小缩放
				int splotches = (int)(100 * (Main.maxTilesX / 4200f));
				int highestY = (int)Utils.Lerp(Main.rockLayer, Main.UnderworldLayer, 0.5);
				for (int iteration = 0; iteration < splotches; iteration++) {
					// 在岩层下半部分但高于地狱层的范围内找到一个点
					int i = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
					int j = WorldGen.genRand.Next(highestY, Main.UnderworldLayer);

					// 使用OreRunner生成满月矿石团
					WorldGen.OreRunner(i, j, WorldGen.genRand.Next(5, 9), WorldGen.genRand.Next(5, 9), (ushort)ModContent.TileType<FullMoonOreTile>());
				}
			});
		}

		// 保存数据到世界文件
		public override void SaveWorldData(TagCompound tag) {
			tag["oreGenerated"] = oreGenerated;
		}
        

		// 从世界文件加载数据
		public override void LoadWorldData(TagCompound tag) {
			oreGenerated = tag.ContainsKey("oreGenerated") ? tag.GetBool("oreGenerated") : false;
		}

        

		// 当进入世界时重置状态（可选，用于调试）
		// public override void OnWorldLoad() {
		// 	oreGenerated = false;
		// }

		// // 当离开世界时重置状态
		// public override void OnWorldUnload() {
		// 	oreGenerated = false;
		// }
		
		// 移除ModifyWorldGenTasks方法，不再在世界生成时添加满月矿石
	}
}