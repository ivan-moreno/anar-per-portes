using UnityEngine;

namespace AnarPerPortes
{
    public class TriggerMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip musicClip;
        [SerializeField] private bool triggerOnlyOnce = true;
        [SerializeField] private bool smooth = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (musicClip == null)
            {
                if (smooth)
                    AudioManager.Singleton.SetTargetVolume(0f);
                else
                    AudioManager.Singleton.StopMusic();

                return;
            }

            if (AudioManager.Singleton.GetCurrentMusicClip() == musicClip)
                return;

            if (smooth)
            {
                AudioManager.Singleton.SetVolume(0f);
                AudioManager.Singleton.SetTargetVolume(1f);
                AudioManager.Singleton.PlayMusic(musicClip);
            }
            else
            {
                AudioManager.Singleton.SetVolume(1f);
                AudioManager.Singleton.PlayMusic(musicClip);
            }

            if (triggerOnlyOnce)
                enabled = false;
        }
    }
}
