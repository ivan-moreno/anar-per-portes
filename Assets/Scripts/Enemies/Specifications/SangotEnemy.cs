using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Sangot Enemy")]
    public class SangotEnemy : Enemy
    {
        public static UnityEvent<SangotEnemy> OnSpawned { get; } = new();

        [Header("Stats")]
        [SerializeField][Min(0f)] private float spawnDistance = 32f;
        [SerializeField][Min(0f)] private float spawnDistanceHard = 24f;
        [SerializeField][Min(0f)] private float runSpeed = 10f;
        [SerializeField][Min(0f)] private float catchRange = 2f;
        [SerializeField][Min(0f)] private float despawnTime = 2f;

        [Header("Sound")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource endingMusic;

        private Vector3 originalPlayerPosition;
        private Pedestal originalPedestal;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(new(0.73f, 0.37f, 1f, 1f), 0.5f);

            if (PlayerController.Singleton.IsCamouflaged)
            {
                originalPedestal = PlayerController.Singleton.CurrentPedestal;
                originalPedestal.ReleasePlayer();
            }

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

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            BouserBossEnemy.OnSpawn.AddListener((_) => WrapUp());
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            OnSpawned?.Invoke(this);
        }

        protected override void Despawn()
        {
            if (isCatching)
                return;

            PlayerCollectTix(10, "Has evadido a Sangot");
            base.Despawn();
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
            if (isInIntro)
                return;

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

            if (!EnemyIsOperative<SheepyEnemy>() && !EnemyIsOperative<A90Enemy>())
                transform.position = nextPosition;
        }

        private void CatchPlayer()
        {
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            if (IsRoblomanDisguise)
            {
                RevealRoblomanDisguise().OnPlayerPackedReward.AddListener(WrapUp);
                gameObject.SetActive(false);
                yield break;
            }

            if (TryConsumePlayerImmunityItem())
            {
                WrapUp();
                yield break;
            }

            if (isCatching)
                yield break;

            isCatching = true;
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);
            animator.Play("Jumpscare");
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            yield return new WaitForSeconds(1.45f);

            audioSource.spatialBlend = 0f;
            audioSource.PlayOneShot(endingMusic);
            CatchManager.Singleton.CatchPlayer("SANGOT ENDING", "youtube.com/@sangot", characterRenderSprite, broskyTip);
        }

        private void WrapUp()
        {
            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 0.5f);
            PlayerController.Singleton.Teleport(originalPlayerPosition);

            if (originalPedestal != null)
                originalPedestal.OccupyPlayer();

            Despawn();
        }
    }
}
