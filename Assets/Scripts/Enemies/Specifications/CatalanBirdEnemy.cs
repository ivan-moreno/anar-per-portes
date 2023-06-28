using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using static AnarPerPortes.ShortUtils;
using static UnityEngine.Rendering.BoolParameter;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Catalan Bird Driver Enemy")]
    public class CatalanBirdEnemy : Enemy
    {
        public static bool IsCursed { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private bool isSupportingCast = false;
        [SerializeField][Min(0f)] private float maxSpeed = 14f;
        [SerializeField][Min(0f)] private float maxTurboSpeed = 22f;
        [SerializeField][Min(0f)] private float maxDriftSpeed = 6f;
        [SerializeField][Min(0f)] private float turboDistance = 20f;
        [SerializeField][Min(0f)] private float turboMaxAngle = 20f;
        [SerializeField][Min(0f)] private float acceleration = 2f;
        [SerializeField][Min(0f)] private float turnRate = 20f;
        [SerializeField][Min(0f)] private float driftTurnRate = 60f;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource turboSound;

        private float currentSpeed = 0f;
        private bool isTurbo = false;
        private bool isDrift = false;
        private bool isFrozen = false;
        private float turboCooldown;
        private static bool showedIntro = false;

        public override void Spawn()
        {
            if (!isSupportingCast)
                base.Spawn();

            CacheComponents();

            var targetPosition = LatestRoom().transform.position + LatestRoom().transform.forward * 56;
            var targetRotation = Quaternion.LookRotation(-LatestRoom().transform.forward);
            transform.SetPositionAndRotation(targetPosition, targetRotation);

            if (spawnSound != null)
                audioSource.PlayOneShot(spawnSound);

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(() =>
            {
                isFrozen = true;
                audioSource.Stop();
            });

            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
            RoomManager.Singleton.OnRoomGenerated.AddListener(x => Despawn());

            if (!IsHardmodeEnabled() && CurrentSettings().EnableEnemyTips)
            {
                if (!showedIntro && !isInIntro)
                {
                    showedIntro = true;
                    StartCoroutine(nameof(IntroCinematicCoroutine));
                }
            }
        }

        protected override void Despawn()
        {
            if (isSupportingCast)
            {
                base.Despawn();
                return;
            }

            if (IsHardmodeEnabled())
                PlayerCollectTix(50, "Has evadido a los coches de choque");
            else
                PlayerCollectTix(25, "Has evadido a Ocell Català");

            base.Despawn();
        }

        void Start()
        {
            if (isSupportingCast)
                Spawn();
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
            if (isInIntro || isFrozen || EnemyIsOperative<A90Enemy>())
                return;

            var distance = DistanceToPlayer(transform);
            var angleDiff = AngleDiff(transform, PlayerController.Singleton.transform);

            isDrift = angleDiff > turboMaxAngle;

            var wasTurbo = false;

            turboCooldown -= Time.deltaTime;

            isTurbo = turboCooldown <= 0f
                && distance < turboDistance
                && angleDiff < turboMaxAngle;

            if (!wasTurbo && isTurbo)
            {
                animator.Play("Turbo");
                audioSource.PlayOneShot(turboSound);
                turboCooldown = 5f;
            }

            var targetSpeed =
                isDrift ? maxDriftSpeed
                : isTurbo ? maxTurboSpeed
                : maxSpeed;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, isTurbo ? acceleration * 2f : acceleration * Time.deltaTime);

            var preMovePosition = transform.position;
            transform.Translate(currentSpeed * Time.deltaTime * Vector3.forward, Space.Self);
            var velocity = Time.timeScale <= 0f ? Vector3.zero : (transform.position - preMovePosition) / Time.deltaTime;

            var targetTurnRate = isDrift ? driftTurnRate : turnRate;
            targetTurnRate *= -TargetSide();
            var targetRotation = transform.eulerAngles.y + targetTurnRate * Time.deltaTime;

            if (distance > 8f && currentSpeed <= maxSpeed)
                transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

            var hVelocity = new Vector3(velocity.x, 0f, velocity.z).sqrMagnitude;

            if (maxTurboSpeed <= 0f)
                hVelocity = 0f;
            else
                hVelocity /= maxTurboSpeed * 8f;

            var animatorHVelocity = animator.GetFloat("HVelocity");

            // Smooth out the velocity changes.
            var smoothHVelocity = Mathf.Lerp(animatorHVelocity, hVelocity, Time.deltaTime * 16f);

            if (float.IsNaN(smoothHVelocity))
                smoothHVelocity = 0f;

            if (animator.enabled)
                animator.SetFloat("HVelocity", smoothHVelocity);
        }

        private float AngleDiff(Transform source, Transform target)
        {
            var directionToTarget = target.position - source.position;
            var angle = Vector3.Angle(source.forward, directionToTarget);
            return angle;
        }

        private float TargetSide()
        {
            var delta = (PlayerPosition() - transform.position).normalized;
            var cross = Vector3.Cross(delta, transform.forward);

            if (cross == Vector3.zero)
                return 0f;
            else if (cross.y > 0)
                return 1f;
            else
                return -1f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || isFrozen)
                return;

            if (IsRoblomanDisguise)
            {
                RevealRoblomanDisguise();
                Despawn();
                return;
            }

            if (TryConsumePlayerImmunityItem())
            {
                Despawn();
                return;
            }

            isCatching = true;
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            audioSource.Stop();

            if (IsHardmodeEnabled())
                CatchManager.Singleton.CatchPlayer("COCHES CHOCONES ENDING", "Era un domingo en la tarde");
            else
                CatchManager.Singleton.CatchPlayer("OCELL CATALÀ ENDING", "CASSO EN L'OLLA, NEN!");
        }
    }
}
