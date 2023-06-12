using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    public static class AudioSourceExtensions
    {
        public static void Play(this AudioSource audioSource, SoundResource soundResource)
        {
            audioSource.Play();
            PushSubtitle(soundResource);
        }

        public static void PlayOneShot(this AudioSource audioSource, SoundResource soundResource)
        {
            audioSource.PlayOneShot(soundResource.AudioClip);
            PushSubtitle(soundResource);
        }
    }
}
