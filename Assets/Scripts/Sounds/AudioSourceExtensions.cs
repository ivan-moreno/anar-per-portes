using UnityEngine;

namespace AnarPerPortes
{
    public static class AudioSourceExtensions
    {
        public static void PlayOneShot(this AudioSource audioSource, SoundResource soundResource)
        {
            audioSource.PlayOneShot(soundResource.AudioClip);
            SubtitleManager.Singleton.PushSubtitle(soundResource.SubtitleText, soundResource.SubtitleTeam);
        }
    }
}
