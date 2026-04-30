using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;

namespace ExpansionKele.Content.Audio
{
    public static class ExpansionKeleSounds
    {
        public static SoundStyle SniperSound = new SoundStyle("ExpansionKele/Content/Audio/SniperSound")
        {
            Volume = 0.3f,
            PitchVariance = 0.2f,
            MaxInstances = 3,
        };

        public static SoundStyle FadeReloadSound = new SoundStyle("ExpansionKele/Content/Audio/FadeReloadAudio")
        {
            Volume = 0.4f,
            PitchVariance = 0.1f,
            MaxInstances = 2,
        };

        public static SoundStyle IronCurtainExplosionSound = new SoundStyle("ExpansionKele/Content/Audio/IronCurtainExplosion")
        {
            Volume = 0.8f,
            PitchVariance = 0.1f,
            MaxInstances = 3,
        };

        public static SoundStyle SwingSound = new SoundStyle("ExpansionKele/Content/Audio/SwingSound")
        {
            Volume = 0.5f,
            PitchVariance = 0.15f,
            MaxInstances = 4,
        };
        private static Dictionary<string, SoundStyle> SoundStyles = new Dictionary<string, SoundStyle>();


        public static void PlaySound(string name, float pitch = 1, Vector2? pos = null, int maxIns = 6, float volume = 1)
        {
            string path = "ExpansionKele/Content/Audio/";
            if (!Main.dedServ)
            {
                if (!SoundStyles.ContainsKey(path + name))
                {
                    SoundStyles[path + name] = new SoundStyle(path + name);
                }
                SoundStyle sound = SoundStyles[path + name];
                sound.Pitch = pitch - 1;
                sound.Volume = volume;
                sound.MaxInstances = maxIns;
                sound.LimitsArePerVariant = true;
                SoundEngine.PlaySound(in sound, pos);
            }
        }

        public static void PlaySound(SoundStyle soundStyle, Vector2? pos = null,bool modifyProperties = false,float pitch = 1,  int maxIns = 5, float volume = 1)
        {
            if (!Main.dedServ)
            {
                if (modifyProperties)
                {
                    soundStyle = soundStyle with 
                    { 
                        Pitch = pitch - 1f, 
                        Volume = volume, 
                        MaxInstances = maxIns,
                        LimitsArePerVariant = true
                    };
                }
                SoundEngine.PlaySound(in soundStyle, pos);
            }
        }
    }
}
