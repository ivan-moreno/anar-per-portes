using UnityEngine;
using UnityEngine.Audio;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Audio Manager")]
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Singleton { get; private set; }
        [SerializeField] private AudioMixer defaultAudioMixer;
        [SerializeField] private AudioMixer reverbAudioMixer;

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
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            AudioListener.volume = GameSettingsManager.Singleton.CurrentSettings.Volume;
        }
    }
}
