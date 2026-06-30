using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Serbull.GameAssets.Audio;

namespace Serbull.GameAssets
{
    public class AudioService : MonoBehaviour
    {
        private class PitchParam
        {
            public float LastPlayTime;
            public int PlayTimes;
        }

        private class MusicData
        {
            public AudioConfig.MusicData ConfigData;
            public AudioSource AudioSource;
            public int CurrentClip;
            public bool IsPlaying;
        }

        private class SoundData
        {
            public AudioSource AudioSource;
            public string SoundId;
        }

        private readonly List<MusicData> _musicDatas = new();
        private readonly Dictionary<string, PitchParam> _soundPitches = new();
        private readonly List<SoundData> _soundSources = new();

        private AudioConfig _audioConfig;
        private Transform _soundsRoot;

        public void Init(AudioConfig audioConfig)
        {
            _audioConfig = audioConfig;

            foreach (var music in audioConfig.Musics)
            {
                var source = new GameObject($"Music ({music.Id})")
                    .AddComponent<AudioSource>();
                source.transform.SetParent(transform);
                source.outputAudioMixerGroup = audioConfig.MusicMixerGroup;
                source.playOnAwake = false;

                var data = new MusicData { ConfigData = music, AudioSource = source, CurrentClip = 0, IsPlaying = music.PlayOnStart };
                _musicDatas.Add(data);

                if (music.PlayOnStart)
                {
                    PlayNextMusicClip(data);
                }
            }

            _soundsRoot = new GameObject("Sounds").transform;
            _soundsRoot.SetParent(transform);
        }

        private void Update()
        {
            foreach (var music in _musicDatas)
            {
                if (music.IsPlaying && !music.AudioSource.isPlaying)
                {
                    PlayNextMusicClip(music);
                }
            }
        }

        private SoundData GetFreeSoundSource()
        {
            foreach (var soundData in _soundSources)
            {
                if (!soundData.AudioSource.isPlaying)
                {
                    return soundData;
                }
            }

            var newSource = new GameObject($"Sound ({_soundSources.Count})")
                .AddComponent<AudioSource>();
            newSource.transform.SetParent(_soundsRoot);
            newSource.outputAudioMixerGroup = _audioConfig.SoundMixerGroup;
            newSource.playOnAwake = false;

            var data = new SoundData { AudioSource = newSource };
            _soundSources.Add(data);
            return data;
        }

        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);

            var mixerVolume = volume > 0 ? volume * 40f - 40f : -80f;
            _audioConfig.AudioMixer.SetFloat("MusicVolume", mixerVolume);
        }

        public void SetSoundVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);

            var mixerVolume = volume > 0 ? volume * 40f - 40f : -80f;
            _audioConfig.AudioMixer.SetFloat("SoundVolume", mixerVolume);
        }

        public AudioSource PlayMusic(string musicName)
        {
            var data = _musicDatas.FirstOrDefault(i => i.ConfigData.Id == musicName);
            if (data == null)
            {
                Debug.LogWarning($"Not exist music with id: {musicName}.");
                return null;
            }

            data.IsPlaying = true;
            PlayNextMusicClip(data);
            return data.AudioSource;
        }

        public void StopMusic(string musicName)
        {
            var data = _musicDatas.FirstOrDefault(i => i.ConfigData.Id == musicName);
            if (data == null)
            {
                Debug.LogWarning($"Not exist music with id: {musicName}.");
                return;
            }

            data.IsPlaying = false;
            data.AudioSource.Stop();
        }

        public AudioSource PlaySound(string soundName)
        {
            return PlaySound(soundName, 0, 0);
        }

        public AudioSource PlaySound(string soundName, float ovveridePitch)
        {
            return PlaySound(soundName, 0, ovveridePitch);
        }

        public AudioSource PlaySound(string soundName, float overrideVolume, float overridePitch)
        {
            var sound = _audioConfig.Sounds.FirstOrDefault((sound) => sound.Id == soundName);

            if (sound == null)
            {
                Debug.LogWarning($"Not exist sound with id: {soundName}.");
                return null;
            }

            var clip = sound.Clips[Random.Range(0, sound.Clips.Length)];

            if (sound.UsePitchEffect)
            {
                return PlaySound(clip, sound.Volume, sound.Id, sound.PitchStep, sound.PitchResetTime);
            }
            else
            {
                var volume = overrideVolume > 0 ? overrideVolume : sound.Volume;
                var pitch = overridePitch > 0 ? overridePitch : 1;
                return PlaySound(clip, volume, pitch, sound.Id);
            }
        }

        public void StopSound(string soundName)
        {
            foreach (var soundData in _soundSources)
            {
                if (soundData.SoundId == soundName && soundData.AudioSource.isPlaying)
                {
                    soundData.AudioSource.Stop();
                }
            }
        }

        private void PlayNextMusicClip(MusicData musicData)
        {
            if (musicData.ConfigData.Clips == null || musicData.ConfigData.Clips.Length == 0)
            {
                Debug.LogError($"Music '{musicData.ConfigData.Id}' hasn`t clips.");
                return;
            }

            musicData.AudioSource.clip = musicData.ConfigData.Clips[musicData.CurrentClip];
            musicData.CurrentClip = (musicData.CurrentClip + 1) % musicData.ConfigData.Clips.Length;
            musicData.AudioSource.volume = musicData.ConfigData.Volume;
            musicData.AudioSource.Play();
        }

        private AudioSource PlaySound(AudioClip clip, float volume, float pitch, string soundId)
        {
            var soundData = GetFreeSoundSource();
            soundData.SoundId = soundId;
            soundData.AudioSource.pitch = pitch;
            soundData.AudioSource.clip = clip;
            soundData.AudioSource.volume = volume;
            soundData.AudioSource.Play();
            return soundData.AudioSource;
        }

        private AudioSource PlaySound(AudioClip clip, float volume, string pitchId, float pitchStep, float pitchResetTime)
        {
            if (!_soundPitches.ContainsKey(pitchId))
            {
                _soundPitches.Add(pitchId, new PitchParam());
            }

            var pitchParam = _soundPitches[pitchId];

            if (Time.time - pitchParam.LastPlayTime > pitchResetTime)
                pitchParam.PlayTimes = 0;

            pitchParam.LastPlayTime = Time.time;
            pitchParam.PlayTimes++;

            var pitch = 1f - pitchStep + pitchParam.PlayTimes * pitchStep;
            var soundData = GetFreeSoundSource();
            soundData.SoundId = pitchId;
            soundData.AudioSource.pitch = pitch;
            soundData.AudioSource.clip = clip;
            soundData.AudioSource.volume = volume;
            soundData.AudioSource.Play();
            return soundData.AudioSource;
        }
    }
}
