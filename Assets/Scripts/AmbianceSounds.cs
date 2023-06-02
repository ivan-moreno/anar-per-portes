using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Ambiance Sounds")]
    public class AmbianceSounds : MonoBehaviour
    {
        [SerializeField] private AudioClip[] sounds;
        [SerializeField] private string[] soundSubtitles;
        private AudioSource audioSource;
        private float timeUntilNextSound;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            timeUntilNextSound = Random.Range(20f, 90f);
        }

        private void Update()
        {
            timeUntilNextSound -= Time.deltaTime;

            if (timeUntilNextSound <= 0f)
            {
                var rngXPos = Random.Range(-1f, 1f);
                var rngZPos = Random.Range(-1f, 1f);
                var rngPosOffset = new Vector3(rngXPos, 0f, rngZPos).normalized * 16f;
                transform.position = PlayerController.Singleton.transform.position + new Vector3(rngPosOffset.x, 0f, rngPosOffset.z);

                var rngAudioIndex = Random.Range(0, sounds.Length);
                var rngAudio = sounds[rngAudioIndex];
                audioSource.PlayOneShot(rngAudio);

                if (soundSubtitles.Length > rngAudioIndex && !string.IsNullOrWhiteSpace(soundSubtitles[rngAudioIndex]))
                {
                    SubtitleManager.Singleton.PushSubtitle(soundSubtitles[rngAudioIndex], SubtitleCategory.SoundEffect, SubtitleSource.Common);
                }

                timeUntilNextSound = Random.Range(20f, 90f);
            }
        }
    }
}
