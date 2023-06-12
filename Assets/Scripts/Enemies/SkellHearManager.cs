using UnityEngine;
using UnityEngine.UI;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Skell Hear Manager")]
    [RequireComponent(typeof(AudioSource))]
    public sealed class SkellHearManager : MonoBehaviour
    {
        public static SkellHearManager Singleton { get; private set; }
        public bool IsHearing { get; private set; } = false;
        public bool IsHunting { get; private set; } = false;

        [Header("Components")]
        [SerializeField] private Image soundWaveL;
        [SerializeField] private Image soundWaveR;

        [Header("Stats")]
        [SerializeField][Min(1f)] private float maxNoise = 20f;
        [SerializeField][Min(0f)] private float noiseDecayRate = 3f;
        [SerializeField][Min(1)] private int doorsUntilDespawn = 8;
        [SerializeField][Min(1)] private int doorsUntilHuntDespawn = 20;
        [SerializeField][Min(0f)] private float farthestFogDistance = 32f;
        [SerializeField][Min(0f)] private float hardmodeNoiseMultiplier = 0.8f;

        [Header("Sound")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource huntMusic;

        private AudioSource audioSource;
        private float noiseLevel = 0f;
        private float timeSinceLastNoise = 0f;
        private int openedDoors = 0;

        public void AddNoise(float amount)
        {
            if (!IsHearing)
                return;

            if (IsHardmodeEnabled())
                amount *= hardmodeNoiseMultiplier;

            noiseLevel += amount;
            noiseLevel = Mathf.Clamp(noiseLevel, 0f, maxNoise + 1f);
            timeSinceLastNoise = 0f;

            if (noiseLevel < maxNoise)
                return;

            if (IsHardmodeEnabled())
                CatchPlayerHardmode();
            else
                StartHunting();
        }

        public void StartHearing()
        {
            if (!IsHardmodeEnabled() && (IsHearing || IsHunting))
                return;

            IsHearing = true;
            openedDoors = 0;
            audioSource.PlayOneShot(warningSound);
        }

        public void StartHunting()
        {
            if (IsHunting)
                return;

            WrapUp();

            IsHunting = true;

            if (!IsHardmodeEnabled())
                audioSource.Play(huntMusic);

            RenderSettings.fog = true;
        }

        public void FinishHunting()
        {
            if (IsHardmodeEnabled())
                return;

            IsHunting = false;
            audioSource.Stop();
            openedDoors = 0;
            RenderSettings.fog = false;
        }

        public void PauseHuntMusic()
        {
            if (IsHardmodeEnabled())
                return;

            if (!audioSource.isPlaying)
                return;

            audioSource.Pause();
        }

        public void UnpauseHuntMusic()
        {
            if (IsHardmodeEnabled())
                return;

            if (audioSource.isPlaying)
                return;

            audioSource.UnPause();
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

            if (IsHunting)
            {
                if (openedDoors >= doorsUntilHuntDespawn)
                    FinishHunting();

                return;
            }

            if (openedDoors >= doorsUntilDespawn)
                WrapUp();
        }

        private void WrapUp()
        {
            IsHearing = false;
            noiseLevel = 0f;
            openedDoors = 0;
        }

        private void CatchPlayerHardmode()
        {
            var targetPos = PlayerPosition() + (PlayerController.Singleton.transform.forward * (RenderSettings.fogStartDistance + 3f));
            var instance = Instantiate(EnemyManager.Singleton.SkellEnemyPrefab, targetPos, Quaternion.identity);
            instance.GetComponent<SkellEnemy>().SpawnedBecauseOfFog = true;
            WrapUp();
            enabled = false;
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
            else if (Input.GetKeyDown(KeyCode.H))
                StartHunting();
#endif

            if (!IsHardmodeEnabled())
                return;

            var targetFogDistance = RenderSettings.fogStartDistance + farthestFogDistance * (1f - (noiseLevel / maxNoise));

            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, targetFogDistance, Time.deltaTime * 4f);
        }
    }
}
