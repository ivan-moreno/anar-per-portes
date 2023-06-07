using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Skell Hear Manager")]
    [RequireComponent(typeof(AudioSource))]
    public sealed class SkellHearManager : MonoBehaviour
    {
        public static SkellHearManager Singleton { get; private set; }

        [Header("Components")]
        [SerializeField] private Image soundWaveL;
        [SerializeField] private Image soundWaveR;

        [Header("Stats")]
        [SerializeField][Min(1f)] private float maxNoise = 30f;

        [Header("Sound")]
        [SerializeField] private SoundResource warningSound;

        private AudioSource audioSource;
        private float noiseLevel = 0f;
        private float timeSinceLastNoise = 0f;
        private bool isHearing = false;

        public void AddNoise(float amount)
        {
            if (!isHearing || SkellEnemy.EnemyIsActive)
                return;

            noiseLevel += amount;
            noiseLevel = Mathf.Clamp(noiseLevel, 0f, maxNoise);
            timeSinceLastNoise = 0f;

            if (noiseLevel >= maxNoise)
            {
                EnemyManager.Singleton.GenerateEnemy(EnemyManager.Singleton.SkellEnemyPrefab);
                isHearing = false;
                noiseLevel = 0f;
            }
        }

        public void StartHearing()
        {
            if (isHearing || SkellEnemy.EnemyIsActive)
                return;

            isHearing = true;
            audioSource.PlayOneShot(warningSound);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            timeSinceLastNoise += Time.deltaTime;
            noiseLevel -= Time.deltaTime * (timeSinceLastNoise);
            noiseLevel = Mathf.Clamp(noiseLevel, 0f, maxNoise);

            var alpha = noiseLevel / (maxNoise * 2f) / Mathf.Clamp01(timeSinceLastNoise);

            soundWaveL.color
                = soundWaveR.color
                = new(1f, 0.4f, 0.8f, alpha);

            soundWaveL.rectTransform.localScale
                = soundWaveR.rectTransform.localScale
                = Vector3.Lerp(soundWaveL.rectTransform.localScale, Vector3.one * Mathf.Clamp(alpha, 0.85f, 1f), Time.deltaTime * 4f);

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.K))
                StartHearing();
#endif
        }
    }
}
