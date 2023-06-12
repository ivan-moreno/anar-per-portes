using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Sheepy Enemy")]
    public class SheepyEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private float checkMotionTime = 1.2f;
        [SerializeField] private float checkMotionTimeHard = 0.6f;
        [SerializeField] private float despawnTime = 2.2f;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource safeSound;
        [SerializeField] private SoundResource jumpscareSound;

        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private bool isCatching = false;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            var lastRoom = RoomManager.Singleton.LatestRoom.transform;
            var targetPos = lastRoom.position + (lastRoom.forward * 4f);
            transform.position = targetPos;
            transform.LookAt(PlayerPosition());
            audioSource.Play(warningSound);
            SkellHearManager.Singleton.AddNoise(8f);
            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
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
            timeSinceSpawn += Time.deltaTime;

            var targetCheckMotionTime = IsHardmodeEnabled() ? checkMotionTimeHard : checkMotionTime;

            if (!checkedMotion && timeSinceSpawn >= targetCheckMotionTime)
                CheckForMotion();

            if (!isCatching && timeSinceSpawn >= despawnTime)
                Despawn();
        }

        private void CheckForMotion()
        {
            if (checkedMotion)
                return;

            checkedMotion = true;

            var movingX = Mathf.Abs(PlayerController.Singleton.Velocity.x) > 0f;
            var movingZ = Mathf.Abs(PlayerController.Singleton.Velocity.z) > 0f;

            if (movingX || movingZ)
                CatchPlayer();
            else
            {
                animator.Play("Retreat");
                PushSubtitle(safeSound);
            }
        }

        private void CatchPlayer()
        {
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            if (IsRoblomanDisguise)
            {
                RevealRoblomanDisguise();
                Despawn();
                yield break;
            }

            if (TryConsumePlayerImmunityItem())
            {
                Despawn();
                yield break;
            }

            if (isCatching)
                yield break;

            isCatching = true;
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);
            animator.Play("Jumpscare");
            audioSource.PlayOneShot(jumpscareSound);

            var timer = 0f;
            var originalPos = transform.position;
            var targetPos = PlayerPosition() + PlayerController.Singleton.transform.forward * 2f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * 1.5f;
                transform.LookAt(PlayerPosition());
                transform.position = Vector3.Lerp(originalPos, targetPos, timer);
                yield return null;
            }

            CatchManager.Singleton.CatchPlayer("SHEEPY ENDING", "¡¡Deja paso a las ovejaaas!!");
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
