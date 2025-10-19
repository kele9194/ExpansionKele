using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Properties
{
    // Acts as a container for keybinds registered by this mod.
    // See your usage file for details on how to use these keybinds.
    public class ExpansionKeybindSystem : ModSystem
    {
        public static ModKeybind RandomBuffKeybind { get; private set; }
        public static ModKeybind LearningExampleKeybind { get; private set; }

        public static ModKeybind StarKeyBind { get; private set; }

        public override void Load() {
            // Registers a new keybind
            // We localize keybinds by adding a Mods.{ModName}.Keybind.{KeybindName} entry to our localization files. The actual text displayed to English users is in en-US.hjson
            // RandomBuffKeybind = KeybindLoader.RegisterKeybind(Mod, "ExpansionRandomBuff", "P");
            // LearningExampleKeybind = KeybindLoader.RegisterKeybind(Mod, "ExpansionLearningExample", "O");
            //StarKeyBind =   KeybindLoader.RegisterKeybind(Mod, "ExpansionStarBuff", "F");
        }

        // Please see your mod's Unload() method for a detailed explanation of the unloading process.
        public override void Unload() {
            // Not required if your AssemblyLoadContext is unloading properly, but nulling out static fields can help you figure out what's keeping it loaded.
            // RandomBuffKeybind = null;
            // LearningExampleKeybind = null;
            // StarKeyBind = null;
        }
    }
}