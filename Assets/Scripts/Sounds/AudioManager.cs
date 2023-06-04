using UnityEngine;
using UnityEngine.Audio;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Audio Manager")]
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Singleton { get; private set; }
        [SerializeField] private AudioMixer defaultAudioMixer;

        public void MuteDefaultAudioMixer()
        {
            defaultAudioMixer.SetFloat("MasterVolume", 0f);
        }

        public void UnmuteDefaultAudioMixer()
        {
            defaultAudioMixer.SetFloat("Volume", 1f);
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}
