using Terraria.ModLoader;
using Terraria.GameContent.Events;
using Terraria;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Items.Tiles;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Bosses.ShadowOfRevenge
{
	public class DownedShadowOfRevengeBoss : ModSystem
	{
		public static bool downedShadowOfRevenge = false;

		public override void OnWorldLoad() {
			downedShadowOfRevenge = false;
		}

		public override void OnWorldUnload() {
			downedShadowOfRevenge = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			tag["downedShadowOfRevenge"] = downedShadowOfRevenge;
		}

		public override void LoadWorldData(TagCompound tag) {
			downedShadowOfRevenge = tag.ContainsKey("downedShadowOfRevenge") ? tag.GetBool("downedShadowOfRevenge") : false;
		}

		public override void PostUpdateEverything() {
			
		}
	}
}