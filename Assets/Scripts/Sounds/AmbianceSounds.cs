using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Ambiance Sounds")]
    public class AmbianceSounds : MonoBehaviour
    {
        [SerializeField] private SoundResource[] sounds;
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

            if (timeUntilNextSound > 0f)
                return;

            var rngXPos = Random.Range(-1f, 1f);
            var rngZPos = Random.Range(-1f, 1f);
            var rngPosOffset = new Vector3(rngXPos, 0f, rngZPos).normalized * 16f;
            transform.position = PlayerPosition() + new Vector3(rngPosOffset.x, 0f, rngPosOffset.z);
            audioSource.PlayOneShot(sounds.RandomItem());
            SkellHearManager.Singleton.AddNoise(4f);
            timeUntilNextSound = Random.Range(20f, 90f);
        }
    }
}
