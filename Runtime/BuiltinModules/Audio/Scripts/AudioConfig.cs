using UnityEngine;
using System;
using UnityEngine.Audio;

namespace Serbull.GameAssets.Audio
{
    public class AudioConfig : ScriptableObject
    {
        [Serializable]
        public class MusicData
        {
            public string Id;
            public AudioClip[] Clips;
            [Range(0f, 1f)] public float Volume = 1f;
            public bool PlayOnStart = true;
        }

        [Serializable]
        public class SoundData
        {
            public string Id;
            public AudioClip[] Clips;
            [Range(0f, 1f)] 
            public float Volume = 1f;
            [Header("Pitch Effect")]
            public bool UsePitchEffect;
            [ShowIf(nameof(UsePitchEffect))]
            public float PitchStep = 0.2f;
            [ShowIf(nameof(UsePitchEffect))]
            public float PitchResetTime = 2f;
        }

        public AudioMixer AudioMixer;
        public AudioMixerGroup MusicMixerGroup;
        public AudioMixerGroup SoundMixerGroup;
        public MusicData[] Musics;
        public SoundData[] Sounds;
    }
}
