// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Microsoft.Xna.Framework;
// using Terraria.Localization;
// using Terraria.GameContent;
// using Terraria.Utilities;

// namespace ExpansionKele.Content.NPCs.TownNPC
// {
// 	[AutoloadHead]
// 	public class Cola : ModNPC
// 	{
// 		public const string ShopName = "Shop";

// 		public override void SetStaticDefaults() {
// 			Main.npcFrameCount[Type] = 25; // The total amount of frames the NPC has

// 			NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
// 			NPCID.Sets.AttackFrameCount[Type] = 4; // The amount of frames in the attacking animation.
// 			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the NPC that it tries to attack enemies.
// 			NPCID.Sets.AttackType[Type] = 0; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
// 			NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts.
// 			NPCID.Sets.AttackAverageChance[Type] = 30; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive.
// 			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
// 			NPCID.Sets.ShimmerTownTransform[Type] = false; // Allows for this NPC to have a different texture after touching the Shimmer liquid.

// 			// Influences how the NPC looks in the Bestiary
// 			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
// 				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
// 				Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
// 			};

// 			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
// 		}

// 		public override void SetDefaults() {
// 			NPC.townNPC = true; // Sets NPC to be a Town NPC
// 			NPC.friendly = true; // NPC Will not attack player
// 			NPC.width = 18;
// 			NPC.height = 40;
// 			NPC.aiStyle = NPCAIStyleID.Passive;
// 			NPC.damage = 10;
// 			NPC.defense = 15;
// 			NPC.lifeMax = 250;
// 			NPC.HitSound = SoundID.NPCHit1;
// 			NPC.DeathSound = SoundID.NPCDeath1;
// 			NPC.knockBackResist = 0.5f;

// 			AnimationType = NPCID.Guide;
// 		}

// 		public override void SetBestiary(Terraria.GameContent.Bestiary.BestiaryDatabase database, Terraria.GameContent.Bestiary.BestiaryEntry bestiaryEntry) {
// 			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
// 			bestiaryEntry.Info.AddRange(new Terraria.GameContent.Bestiary.IBestiaryInfoElement[] {
// 				// Sets the preferred biomes of this town NPC listed in the bestiary.
// 				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
// 				Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

// 				// Sets your NPC's flavor text in the bestiary. (use localization keys)
// 				new Terraria.GameContent.Bestiary.FlavorTextBestiaryInfoElement("A refreshing person who loves cola. Always brings happiness wherever he goes.")
// 			});
// 		}

// 		public override void HitEffect(NPC.HitInfo hit) {
// 			// TODO: Add particle effects when the NPC is hit
// 		}

// 		public override bool CanTownNPCSpawn(int numTownNPCs) {
// 			// TODO: Add conditions for the NPC to spawn
// 			// For now, allow spawning if another town NPC exists
// 			return numTownNPCs >= 1;
// 		}

// 		public override ITownNPCProfile TownNPCProfile() {
// 			// TODO: Add profile with textures for the NPC
// 			// For now, return a basic profile
// 			return new Profiles.DefaultNPCProfile("ExpansionKele/Content/NPCs/TownNPC/Cola", NPCHeadLoader.GetHeadSlot(HeadTexture));
// 		}

// 		public override void FindFrame(int frameHeight) {
// 			// TODO: Customize NPC animation frames if needed
// 		}

// 		public override string GetChat() {
// 			// TODO: Add more dialogue options
// 			WeightedRandom<string> chat = new WeightedRandom<string>();

// 			chat.Add("Have you tried the new cola flavor today?");
// 			chat.Add("Nothing beats a cold cola on a hot day!");
// 			chat.Add("I wonder where I can get more cola...");
			
// 			return chat;
// 		}

// 		public override void SetChatButtons(ref string button, ref string button2) {
// 			// TODO: Add functionality to shop buttons
// 			button = Language.GetTextValue("LegacyInterface.28"); // Shop
// 			button2 = "Cola!";
// 		}

// 		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
// 			if (firstButton) {
// 				// TODO: Add shop functionality
// 				shop = ShopName;
// 			}
// 			else {
// 				// TODO: Add secondary button functionality
// 				Main.npcChatText = "Glug glug~";
// 			}
// 		}

// 		public override void AddShops() {
// 			// TODO: Add items to the shop
// 			var npcShop = new NPCShop(Type, ShopName);
// 			npcShop.Register();
// 		}

// 		public override void ModifyNPCLoot(Terraria.ModLoader.NPCLoot npcLoot) {
// 			// TODO: Add loot drops
// 		}

// 		// Make this Town NPC teleport to the King and/or Queen statue when triggered.
// 		public override bool CanGoToStatue(bool toKingStatue) => true;

// 		public override void OnGoToStatue(bool toKingStatue) {
// 			// TODO: Add statue teleport effects
// 		}
// 	}
// }