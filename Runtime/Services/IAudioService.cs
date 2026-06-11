
namespace Serbull.GameAssets
{
    public interface IAudioService
    {
        void SetMusicVolume(float volume);
        void SetSoundVolume(float volume);

        void PlayMusic(string musicName);
        void StopMusic(string musicName);

        void PlaySound(string soundName);
        void PlaySound(string soundName, float ovveridePitch);
        void PlaySound(string soundName, float ovveridePitch, float overrideVolume);
    }
}
