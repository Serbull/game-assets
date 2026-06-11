using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Serbull.GameAssets.Audio
{
    public class AudioManager : MonoBehaviour, IAudioService
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

        private readonly List<MusicData> _musicDatas = new();
        private readonly Dictionary<string, PitchParam> _soundPitches = new();

        private AudioConfig _audioConfig;
        private AudioSource _soundSource;

        public void Init(AudioConfig audioConfig)
        {
            _audioConfig = audioConfig;

            foreach (var music in audioConfig.Musics)
            {
                var source = new GameObject($"Music ({music.Id})").AddComponent<AudioSource>();
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

            _soundSource = new GameObject("Sounds").AddComponent<AudioSource>();
            _soundSource.transform.SetParent(transform);
            _soundSource.outputAudioMixerGroup = audioConfig.SoundMixerGroup;
            _soundSource.playOnAwake = false;
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

        public void PlayMusic(string musicName)
        {
            var data = _musicDatas.FirstOrDefault(i => i.ConfigData.Id == musicName);
            if (data == null)
            {
                Debug.LogWarning($"Not exist music with id: {musicName}.");
                return;
            }

            data.IsPlaying = true;
            PlayNextMusicClip(data);
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

        public void PlaySound(string soundName)
        {
            PlaySound(soundName, 0, 0);
        }

        public void PlaySound(string soundName, float overrideVolume, float overridePitch)
        {
            var sound = _audioConfig.Sounds.FirstOrDefault((sound) => sound.Id == soundName);

            if (sound == null)
            {
                Debug.LogWarning($"Not exist sound with id: {soundName}.");
                return;
            }

            var clip = sound.Clips[Random.Range(0, sound.Clips.Length)];

            if (sound.UsePitchEffect)
            {
                PlaySound(clip, sound.Volume, sound.Id, sound.PitchStep);
            }
            else
            {
                var volume = overrideVolume > 0 ? overrideVolume : sound.Volume;
                var pitch = overridePitch > 0 ? overridePitch : 1;
                PlaySound(clip, volume, pitch);
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

        private void PlaySound(AudioClip clip, float volume, float pitch)
        {
            _soundSource.pitch = pitch;
            _soundSource.PlayOneShot(clip, volume);
        }

        private void PlaySound(AudioClip clip, float volume, string pitchId, float pitchStep)
        {
            if (!_soundPitches.ContainsKey(pitchId))
            {
                _soundPitches.Add(pitchId, new PitchParam());
            }

            var pitchParam = _soundPitches[pitchId];

            if (Time.time - pitchParam.LastPlayTime > 2f)
                pitchParam.PlayTimes = 0;

            pitchParam.LastPlayTime = Time.time;
            pitchParam.PlayTimes++;

            var pitch = 1f - pitchStep + pitchParam.PlayTimes * pitchStep;
            _soundSource.pitch = pitch;
            _soundSource.PlayOneShot(clip, volume);
        }
    }
}
