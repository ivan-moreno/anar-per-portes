using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    public class OshoskyEnemy : Enemy
    {
        [Header("Stats")]
        [SerializeField][Min(0f)] private float timeToPounce = 1.2f;
        [SerializeField][Min(0f)] private float pounceSpeed = 16f;
        [SerializeField][Min(0f)] private float catchDistance = 2f;

        [Header("Components")]
        [SerializeField] private ParticleSystem pounceParticles;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource pounceSound;
        [SerializeField] private SoundResource stuckSound;
        [SerializeField] private SoundResource catchSound;
        [SerializeField] private AudioClip catchMusic;

        private new Collider collider;
        private bool playerInLineOfSight = false;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            collider = GetComponent<Collider>();
            collider.enabled = false;
            transform.LookAt(PlayerPosition());
            transform.position = LatestRoom().transform.position + LatestRoom().transform.forward * 4f;

            PauseManager.Singleton.OnPauseChanged.AddListener(OnPauseChanged);
            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            LatestRoom().OnUnloading.AddListener(Despawn);

            StartCoroutine(nameof(PounceCoroutine));
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void FixedUpdate()
        {
            playerInLineOfSight = PlayerIsInLineOfSight(transform.position + Vector3.up);
        }

        private IEnumerator PounceCoroutine()
        {
            transform.LookAt(-LatestRoom().transform.forward);
            var targetPounceLocation = transform.position + transform.forward * (pounceSpeed * 0.5f);
            var timer = 0f;
            audioSource.PlayOneShot(warningSound);

            if (!IsHardmodeEnabled() && PlayerIsInLineOfSight(transform.position + Vector3.up))
            {
                targetPounceLocation = PlayerPosition();
                transform.LookAt(targetPounceLocation);
            }

            while (timer < timeToPounce)
            {
                if (IsHardmodeEnabled() && playerInLineOfSight)
                {
                    targetPounceLocation = PlayerPosition();
                    transform.LookAt(targetPounceLocation);
                }

                // TODO: Consider waiting for Sangot on hardmode
                if (!EnemyIsOperative<A90Enemy>() && !EnemyIsOperative<SheepyEnemy>())
                    timer += Time.deltaTime;

                yield return null;
            }

            animator.SetTrigger("Pounce");
            audioSource.PlayOneShot(pounceSound);
            pounceParticles.Play(true);

            while (Vector3.Distance(transform.position, targetPounceLocation) > pounceSpeed * Time.deltaTime)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPounceLocation, pounceSpeed * Time.deltaTime);

                if (DistanceToPlayer(transform) < catchDistance)
                {
                    yield return CatchPlayerCoroutine();
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }

            timer = 1f;
            pounceParticles.Stop(true);

            while (timer > 0.25f)
            {
                transform.Translate(Vector3.forward * pounceSpeed * timer * Time.deltaTime, Space.Self);
                timer -= Time.deltaTime * 2f;

                if (DistanceToPlayer(transform) < catchDistance)
                {
                    yield return CatchPlayerCoroutine();
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }

            PlayerController.Singleton.CollectTix(10, "Has evadido a Oshosky");
            collider.enabled = true;
            animator.SetTrigger("Stuck");
            audioSource.PlayOneShot(stuckSound);

            var foundFloor = Physics.Raycast(transform.position, Vector3.down, out var hit, 24f, LayerMask.GetMask("Default"));

            if (foundFloor)
                transform.position = hit.point;
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            if (isCatching)
                yield break;

            if (IsRoblomanDisguise)
            {
                RevealRoblomanDisguise();
                Despawn();
                yield break;
            }

            if (TryConsumePlayerImmunityItem())
            {
                PlayerSound(stuckSound);
                Despawn();
                yield break;
            }

            isCatching = true;
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);

            transform.LookAt(PlayerPosition());
            audioSource.Stop();
            audioSource.PlayOneShot(catchSound);
            pounceParticles.Stop(true);
            yield return new WaitForSeconds(1f);

            audioSource.Stop();
            audioSource.PlayOneShot(catchMusic);
            CatchManager.Singleton.CatchPlayer("OSHOSENDING", "ONE-TRICK OSHOSKY ROAD TO APOYO GRANDMASTER B)", characterRenderSprite, broskyTip);
        }
    }
}
