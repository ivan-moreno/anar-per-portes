using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Sheepy Enemy")]
    public class SheepyEnemy : Enemy
    {
        public static bool EnemyIsActive { get; private set; } = false;

        public override bool EnemyTipWasDisplayed
        {
            get => enemyTipWasDisplayed;
            set => enemyTipWasDisplayed = value;
        }

        private static bool enemyTipWasDisplayed = false;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip jumpscareSound;
        private AudioSource audioSource;
        private Animator animator;
        private Transform model;
        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private bool isCatching = false;
        private const float checkMotionTime = 1.2f;
        private const float despawnTime = 2.2f;

        private void Start()
        {
            var lastRoom = RoomManager.Singleton.LastLoadedRoom.transform;
            var targetPos = lastRoom.position + (lastRoom.forward * 4f);
            transform.position = targetPos;
            audioSource = GetComponent<AudioSource>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
            EnemyIsActive = true;
            audioSource.Play();
            SubtitleManager.Singleton.PushSubtitle("(STOP!)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
        }

        private void Update()
        {
            timeSinceSpawn += Time.deltaTime;

            if (!checkedMotion && timeSinceSpawn >= checkMotionTime)
                CheckForMotion();

            if (!isCatching && timeSinceSpawn >= despawnTime)
                Despawn();
        }

        private void LateUpdate()
        {
            transform.LookAt(PlayerController.Singleton.transform.position);
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
                SubtitleManager.Singleton.PushSubtitle("(Wait a minute!)", SubtitleCategory.SoundEffect, SubtitleSource.Common);
            }
        }

        private void CatchPlayer()
        {
            if (isCatching)
                return;

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(Sheepy grita)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            EnemyIsActive = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.8f);
            CatchManager.Singleton.CatchPlayer("SHEEPY ENDING", "¡¡Deja paso a las ovejaaas!!");
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            EnemyIsActive = false;
            Destroy(gameObject);
        }
    }
}
