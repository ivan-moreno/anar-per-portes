using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Skell Hear Manager")]
    [RequireComponent(typeof(AudioSource))]
    public sealed class SkellHearManager : MonoBehaviour
    {
        public static SkellHearManager Singleton { get; private set; }
        public bool IsHearing { get; private set; } = false;

        [Header("Components")]
        [SerializeField] private Image soundWaveL;
        [SerializeField] private Image soundWaveR;

        [Header("Stats")]
        [SerializeField][Min(1f)] private float maxNoise = 30f;
        [SerializeField][Min(0f)] private float noiseDecayRate = 5f;
        [SerializeField][Min(1)] private int doorsUntilDespawn = 8;

        [Header("Sound")]
        [SerializeField] private SoundResource warningSound;

        private AudioSource audioSource;
        private float noiseLevel = 0f;
        private float timeSinceLastNoise = 0f;
        private int openedDoors = 0;

        public void AddNoise(float amount)
        {
            if (!IsHearing || SkellEnemy.IsOperative)
                return;

            noiseLevel += amount;
            noiseLevel = Mathf.Clamp(noiseLevel, 0f, maxNoise);
            timeSinceLastNoise = 0f;

            if (noiseLevel >= maxNoise)
            {
                EnemyManager.Singleton.GenerateEnemy(EnemyManager.Singleton.SkellEnemyPrefab);
                WrapUp();
            }
        }

        public void StartHearing()
        {
            if (IsHearing || SkellEnemy.IsOperative)
                return;

            IsHearing = true;
            audioSource.PlayOneShot(warningSound);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            RoomManager.Singleton.OnRoomGenerated.AddListener(OnRoomGenerated);
            S7Enemy.OnSpawn.AddListener((_) => WrapUp());
        }

        private void OnRoomGenerated(Room room)
        {
            openedDoors++;

            if (openedDoors >= doorsUntilDespawn)
                WrapUp();
        }

        private void WrapUp()
        {
            IsHearing = false;
            noiseLevel = 0f;
            openedDoors = 0;
        }

        private void Update()
        {
            timeSinceLastNoise += Time.deltaTime;
            noiseLevel -= Time.deltaTime * (timeSinceLastNoise * noiseDecayRate);
            noiseLevel = Mathf.Clamp(noiseLevel, 0f, maxNoise);

            var alpha = noiseLevel / (maxNoise * 2f) / Mathf.Clamp(timeSinceLastNoise, Mathf.Epsilon, 1f);

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
