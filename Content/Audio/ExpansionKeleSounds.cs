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

        // ==== 开辟明日的剑刃（Dawn-Piercing Blade）====
        // 素材约 1 秒，一阶段 15 帧（0.25 秒）一刀 ⇒ 同一时刻最多 4 层。
        // MaxInstances 给到 4：每层刚好自然播完，不会走到 ReplaceOldest 的 Stop(true) 硬切。
        // 4 层同相位叠加约 +12dB，即 4 倍响度。

        /// <summary>本体挥砍（Item.UseSound）。</summary>
        public static SoundStyle EdgeSwing = new SoundStyle("ExpansionKele/Content/Audio/TomorrowsEdge/Swing")
        {
            Volume = 1f,
            PitchVariance = 0.15f,
            MaxInstances = 4,
        };

        /// <summary>本体弧光命中。</summary>
        public static SoundStyle EdgeBaseHit = new SoundStyle("ExpansionKele/Content/Audio/TomorrowsEdge/BaseHit")
        {
            Volume = 1f,
            PitchVariance = 0.2f,
            // 弧光一次挥动可能同时切到一片敌人，每个目标各播一次；
            // 受击音已被 TomorrowsEdgeGlobalNPC 静音，这里是唯一反馈，给足余量。
            MaxInstances = 10,
        };

        /// <summary>一阶段贴怪斩击。</summary>
        public static SoundStyle EdgeStageOneSlash = new SoundStyle("ExpansionKele/Content/Audio/TomorrowsEdge/StageOneSlashOE")
        {
            Volume = 1f,
            PitchVariance = 0.15f,
            // 单体时 0.25 秒一刀只堆到 4 层，但打 4 个目标会一帧起 4 个实例，
            // 4 层的上限会在下一刀立刻触发 Stop 硬切，所以按「目标数 × 2 刀」给到 8。
            // 12 是为了容纳「一阶段 4 目标 + 归宿斩击 6 目标」同时发声。
            MaxInstances = 12,
        };

        /// <summary>进入形态：本体→一阶段、一阶段→二阶段 共用。</summary>
        public static SoundStyle EdgeTurnStage = new SoundStyle("ExpansionKele/Content/Audio/TomorrowsEdge/turnstage")
        {
            Volume = 1f,
            PitchVariance = 0.05f,
            MaxInstances = 2,
        };

        /// <summary>替身结束退回本体。</summary>
        public static SoundStyle EdgeOutStage = new SoundStyle("ExpansionKele/Content/Audio/TomorrowsEdge/OutStage")
        {
            Volume = 1f,
            PitchVariance = 0.1f,
            MaxInstances = 2,
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
