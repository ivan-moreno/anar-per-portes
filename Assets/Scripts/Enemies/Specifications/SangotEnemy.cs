using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Sangot Enemy")]
    public class SangotEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;
        public static UnityEvent<SangotEnemy> OnSpawned { get; } = new();

        [Header("Stats")]
        [SerializeField] private float spawnDistance = 32f;
        [SerializeField] private float spawnDistanceHard = 24f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private float despawnTime = 2f;

        [Header("Sound")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource endingMusic;

        private bool isCatching = false;
        private Vector3 originalPlayerPosition;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(new(0.73f, 0.37f, 1f, 1f), 0.5f);

            originalPlayerPosition = PlayerPosition();
            PlayerController.Singleton.Teleport(EnemyManager.Singleton.SangotRealm.position);

            var spawnPosition = PlayerPosition();

            if (IsHardmodeEnabled())
            {
                var rngX = Random.Range(-1, 2);
                var rngZ = Random.Range(-1, 2);

                if (rngZ == 0)
                    rngX = Random.Range(0, 2) == 0 ? -1 : 1;

                spawnPosition.x += spawnDistanceHard * rngX;
                spawnPosition.z += spawnDistanceHard * rngZ;
            }
            else
            {
                spawnPosition += PlayerController.Singleton.transform.forward * spawnDistance;

                var rng = Random.Range(0, 3);

                if (rng == -1)
                    spawnPosition -= PlayerController.Singleton.transform.right * 8f;
                else if (rng == 1)
                    spawnPosition += PlayerController.Singleton.transform.right * 8f;
            }

            transform.position = spawnPosition;
            audioSource.Play(spawnSound);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
            OnSpawned?.Invoke(this);
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void Update()
        {
            despawnTime -= Time.deltaTime;

            if (despawnTime <= 0f && !isCatching)
            {
                WrapUp();
                return;
            }

            transform.LookAt(PlayerPosition());
            var distanceToPlayer = Vector3.Distance(transform.position, PlayerPosition());

            if (distanceToPlayer <= catchRange)
                CatchPlayer();

            if (isCatching)
                return;

            var nextPosition = Vector3.MoveTowards(transform.position, PlayerPosition(), runSpeed * Time.deltaTime);

            if (!SheepyEnemy.IsOperative && !A90Enemy.IsOperative)
                transform.position = nextPosition;
        }

        private void CatchPlayer()
        {
            if (IsRoblomanDisguise)
            {
                RevealRoblomanDisguise().OnPlayerPackedReward.AddListener(() =>
                {
                    WrapUp();
                    Despawn();
                });

                gameObject.SetActive(false);
                return;
            }

            if (isCatching)
                return;

            if (PlayerController.Singleton.EquippedItemIs("Roblobolita"))
            {
                PlayerController.Singleton.ConsumeEquippedItem();
                BlurOverlayManager.Singleton.SetBlur(Color.white);
                BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 2f);
                WrapUp();
                return;
            }

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            yield return new WaitForSeconds(1.45f);
            audioSource.spatialBlend = 0f;
            audioSource.PlayOneShot(endingMusic);
            CatchManager.Singleton.CatchPlayer("SANGOT ENDING", "youtube.com/@sangot");
        }

        private void WrapUp()
        {
            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 0.5f);
            PlayerController.Singleton.Teleport(originalPlayerPosition);
            Despawn();
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            IsOperative = false;

            if (IsRoblomanDisguise)
                RoblomanEnemy.IsOperative = false;

            Destroy(gameObject);
        }
    }
}
