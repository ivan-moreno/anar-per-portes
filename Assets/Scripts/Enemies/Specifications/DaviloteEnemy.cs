using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Davilote Enemy")]
    public class DaviloteEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private float catchAngle = 120f;

        [Header("Audio")]
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource[] warningSounds;
        [SerializeField] private SoundResource[] meetSheepySounds;

        private bool isCatching = false;

        public float MeetSheepy()
        {
            audioSource.Stop();
            var sound = meetSheepySounds.RandomItem();
            audioSource.PlayOneShot(sound);
            return sound.AudioClip.length + 0.1f;
        }

        private void Start()
        {
            IsOperative = true;
            CacheComponents();
            transform.rotation = PlayerController.Singleton.transform.rotation;

            if (!SheepyEnemy.IsOperative)
                audioSource.PlayOneShot(warningSounds.RandomItem());

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            RoomManager.Singleton.OnRoomGenerated.AddListener((_) => Despawn());
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
            if (isCatching)
            {
                var targetPos = PlayerPosition() - transform.forward;
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 4f);
                return;
            }
        }

        private void LateUpdate()
        {
            transform.position = PlayerPosition() - (transform.forward * 4f);
        }

        private void FixedUpdate()
        {
            var playerAngle = PlayerController.Singleton.transform.eulerAngles.y;
            var enemyAngle = transform.eulerAngles.y;
            var angleDiff = Mathf.DeltaAngle(playerAngle, enemyAngle);

            var vCameraAngle = PlayerController.Singleton.Camera.transform.eulerAngles.x;

            if (vCameraAngle is (> 60f and < 80f) or (> 280f and < 300f))
                return;

            if (Mathf.Abs(angleDiff) > catchAngle)
                CatchPlayer();
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
            yield return new WaitForSeconds(0.84f);

            CatchManager.Singleton.CatchPlayer("DAVILOTE ENDING", "No eres un payaso, eres el circo entero.");
            audioSource.Play();
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
