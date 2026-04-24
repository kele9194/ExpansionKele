using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
	public class DownedBossKeleNew : ModSystem
	{
		public bool downedBossKeleNew = false;

		public override void OnWorldLoad() {
			downedBossKeleNew = false;
		}

		public override void OnWorldUnload() {
			downedBossKeleNew = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			tag["downedBossKeleNew"] = downedBossKeleNew;
		}

		public override void LoadWorldData(TagCompound tag) {
			downedBossKeleNew = tag.ContainsKey("downedBossKeleNew") ? tag.GetBool("downedBossKeleNew") : false;
		}

		public override void PostUpdateEverything() {
			
		}
	}
}