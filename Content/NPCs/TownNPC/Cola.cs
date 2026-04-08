using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.GameContent;
using Terraria.Utilities;
using ExpansionKele.Content.Projectiles.MeleeProj;
using ExpansionKele.Content.Items.OtherItem.BagItem;
using ExpansionKele.Content.Bosses.ShadowOfRevenge;

namespace ExpansionKele.Content.NPCs.TownNPC
{
	[AutoloadHead]
	public class Cola : ModNPC
	{
		public const string ShopName = "Shop";

		public override void SetStaticDefaults() {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Angler];
            NPCID.Sets.ExtraFramesCount[Type] = NPCID.Sets.ExtraFramesCount[NPCID.Angler];
            NPCID.Sets.AttackFrameCount[Type] = NPCID.Sets.AttackFrameCount[NPCID.Angler];
            NPCID.Sets.DangerDetectRange[Type] = 250;
            NPCID.Sets.AttackType[Type] = NPCID.Sets.AttackType[NPCID.Angler];
            NPCID.Sets.AttackTime[Type] = 60;
            NPCID.Sets.AttackAverageChance[Type] = 1;
            //NPCID.Sets.MagicAuraColor[base.NPC.type] = Color.Purple;
			
			//NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
			NPCID.Sets.ShimmerTownTransform[Type] = false; // Allows for this NPC to have a different texture after touching the Shimmer liquid.

			// // Influences how the NPC looks in the Bestiary
			// NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
			// 	Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			// 	Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
			// };

			// NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults() {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 24;
            NPC.height = 48;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.damage = 20;
            NPC.defense = 10;
            NPC.lifeMax = 820;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            AnimationType = NPCID.Angler;
		}

		// public override void SetBestiary(Terraria.GameContent.Bestiary.BestiaryDatabase database, Terraria.GameContent.Bestiary.BestiaryEntry bestiaryEntry) {
		// 	// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
		// 	bestiaryEntry.Info.AddRange(new Terraria.GameContent.Bestiary.IBestiaryInfoElement[] {
		// 		// Sets the preferred biomes of this town NPC listed in the bestiary.
		// 		// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
		// 		Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

		// 		// Sets your NPC's flavor text in the bestiary. (use localization keys)
		// 		new Terraria.GameContent.Bestiary.FlavorTextBestiaryInfoElement("A refreshing person who loves cola. Always brings happiness wherever he goes.")
		// 	});
		// }

		public override void HitEffect(NPC.HitInfo hit) {
			// TODO: Add particle effects when the NPC is hit
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {
			// TODO: Add conditions for the NPC to spawn
			// For now, allow spawning if another town NPC exists
			return numTownNPCs >= 1;
		}

		// public override ITownNPCProfile TownNPCProfile() {
		// 	// TODO: Add profile with textures for the NPC
		// 	// For now, return a basic profile
		// 	return new Profiles.DefaultNPCProfile("ExpansionKele/Content/NPCs/TownNPC/Cola", NPCHeadLoader.GetHeadSlot(HeadTexture));
		// }

		public override void FindFrame(int frameHeight) {
			// TODO: Customize NPC animation frames if needed
		}

		// ... existing code ...

	// ... existing code ...

	public override string GetChat() {
		WeightedRandom<string> chat = new WeightedRandom<string>();

		chat.Add(Language.GetTextValue("Mods.ExpansionKele.NPCs.Cola.Chat.Line1"));
		chat.Add(Language.GetTextValue("Mods.ExpansionKele.NPCs.Cola.Chat.Line2"));
		chat.Add(Language.GetTextValue("Mods.ExpansionKele.NPCs.Cola.Chat.Line3"));
		
		var downedShadowOfRevengeBoss = ModContent.GetInstance<DownedShadowOfRevengeBoss>();
		if (downedShadowOfRevengeBoss.downedShadowOfRevenge) {
			chat.Add(Language.GetTextValue("Mods.ExpansionKele.NPCs.Cola.Chat.DowneddShadowOfRevengeText1"));
		} else {
			chat.Add(Language.GetTextValue("Mods.ExpansionKele.NPCs.Cola.Chat.ShadowOfRevengeText1"));
		}
		
		if (ModLoader.HasMod("FargosWiltas")) {
			chat.Add(Language.GetTextValue("Mods.ExpansionKele.NPCs.Cola.Chat.FargoText1"));
		}
		
		return chat;
	}

// ... existing code ...
// ... existing code ...

		public override void SetChatButtons(ref string button, ref string button2) {
			// TODO: Add functionality to shop buttons
			button = Language.GetTextValue("LegacyInterface.28"); // Shop
			//button2 = "Cola!";
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				// TODO: Add shop functionality
				shop = ShopName;
			}
			// else {
			// 	// TODO: Add secondary button functionality
			// 	Main.npcChatText = "Glug glug~";
			// }
		}

		public override void AddShops() {
			// TODO: Add items to the shop
			var npcShop = new NPCShop(Type, ShopName)
                .Add<GiftOfLight>();
			npcShop.Register();
		}

		public override void ModifyNPCLoot(Terraria.ModLoader.NPCLoot npcLoot) {
			// TODO: Add loot drops
		}

		// Make this Town NPC teleport to the King and/or Queen statue when triggered.
		public override bool CanGoToStatue(bool toKingStatue) => false;

		public override void OnGoToStatue(bool toKingStatue) {
			// TODO: Add statue teleport effects
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
			projType =ModContent.ProjectileType<ColaProjectile>();
			attackDelay = 4;

		}
		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            if (Main.zenithWorld)
            {
                multiplier = 24f;
            }
            gravityCorrection = 0f;
            randomOffset = 0f;
        }
		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 20;
        }
		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = Main.zenithWorld ? 40 : 20;

            knockback = 3f;

        }
	}
}