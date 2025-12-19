using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Bosses.BossKele
{
	public class DownedBossKele : ModSystem
	{
		public static bool downedBossKele = false;

		public override void OnWorldLoad() {
			downedBossKele = false;
		}

		public override void OnWorldUnload() {
			downedBossKele = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			tag["downedBossKele"] = downedBossKele;
		}

		public override void LoadWorldData(TagCompound tag) {
			downedBossKele = tag.ContainsKey("downedBossKele") ? tag.GetBool("downedBossKele") : false;
		}

		public override void PostUpdateEverything() {
			
		}
	}
}