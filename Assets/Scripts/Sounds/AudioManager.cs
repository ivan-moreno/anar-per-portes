using UnityEngine;
using UnityEngine.Audio;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Audio Manager")]
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Singleton { get; private set; }
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambianceSource;

        [Header("Audio Mixers")]
        [SerializeField] private AudioMixer defaultAudioMixer;
        [SerializeField] private AudioMixer reverbAudioMixer;

        private float targetVolume = 1f;
        private float targetVolumeRate = 0.5f;

        public void SetVolume(float volume)
        {
            musicSource.volume = volume;
        }

        public void SetTargetVolume(float volume)
        {
            targetVolume = volume;
        }

        public void SetTargetRate(float volumeRate)
        {
            targetVolumeRate = volumeRate;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.isPlaying)
            {
                if (musicSource.clip == clip)
                    return;

                musicSource.Stop();
            }

            musicSource.clip = clip;
            musicSource.Play();
        }

        public AudioClip GetCurrentMusicClip() => musicSource.clip;

        public void StopMusic()
        {
            if (!musicSource.isPlaying)
                return;
            
            musicSource.Stop();
        }

        public void PlayAmbiance(AudioClip clip)
        {
            if (ambianceSource.isPlaying)
            {
                if (ambianceSource.clip == clip)
                    return;

                ambianceSource.Stop();
            }

            ambianceSource.clip = clip;
            ambianceSource.Play();
        }

        public void StopAmbiance()
        {
            if (!ambianceSource.isPlaying)
                return;

            ambianceSource.Stop();
        }

        public void MuteAllAudioMixers()
        {
            defaultAudioMixer.SetFloat("MasterVolume", -80f);
            reverbAudioMixer.SetFloat("MasterVolume", -80f);
        }

        public void UnmuteAllAudioMixers()
        {
            defaultAudioMixer.SetFloat("MasterVolume", 0f);
            reverbAudioMixer.SetFloat("MasterVolume", 0f);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            UnmuteAllAudioMixers();
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void Update()
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetVolume, targetVolumeRate * Time.unscaledDeltaTime);
        }

        private void OnSettingsChanged()
        {
            AudioListener.volume = GameSettingsManager.Singleton.CurrentSettings.Volume;
        }
    }
}
