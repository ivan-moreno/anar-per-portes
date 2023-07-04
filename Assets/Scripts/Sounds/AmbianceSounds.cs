using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Ambiance Sounds")]
    public class AmbianceSounds : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField][Min(0f)] private float minTimeToPlay = 20f;
        [SerializeField][Min(0f)] private float maxTimeToPlay = 90f;

        [Header("Audio")]
        [SerializeField] private SoundResource[] sounds;

        private AudioSource audioSource;
        private float timeUntilNextSound;
        private Vector3 offset;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            timeUntilNextSound = Random.Range(minTimeToPlay, maxTimeToPlay);
        }

        private void Update()
        {
            timeUntilNextSound -= Time.deltaTime;
            transform.position = PlayerPosition() + offset;

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.O))
                PlaySoundOnRandomPosition();
#endif

            if (timeUntilNextSound <= 0f)
                PlaySoundOnRandomPosition();
        }

        private void PlaySoundOnRandomPosition()
        {
            timeUntilNextSound = Random.Range(minTimeToPlay, maxTimeToPlay);

            var rngXPos = Random.Range(-1f, 1f);
            var rngZPos = Random.Range(-1f, 1f);

            offset = new Vector3(rngXPos, 0f, rngZPos).normalized * 16f;
            audioSource.PlayOneShot(sounds.RandomItem());

            SkellHearManager.Singleton.AddNoise(4f);
        }
    }
}
