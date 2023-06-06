using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Davilote Enemy")]
    public class DaviloteEnemy : Enemy
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
        private Transform model;
        private bool isCatching = false;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            model = transform.GetChild(0);
            EnemyIsActive = true;
            transform.rotation = PlayerController.Singleton.transform.rotation;
            audioSource.PlayOneShot(warningSound);
            SubtitleManager.Singleton.PushSubtitle("[DAVILOTE] Mira detrás de tí.", SubtitleCategory.Dialog, SubtitleSource.Hostile);
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
                var targetPos = PlayerController.Singleton.transform.position - transform.forward;
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 4f);
                return;
            }
        }

        private void LateUpdate()
        {
            transform.position = PlayerController.Singleton.transform.position - (transform.forward * 2f);
        }

        private void FixedUpdate()
        {
            var playerAngle = PlayerController.Singleton.transform.eulerAngles.y;
            var enemyAngle = transform.eulerAngles.y;
            var angleDiff = Mathf.DeltaAngle(playerAngle, enemyAngle);

            var vCameraAngle = PlayerController.Singleton.Camera.transform.eulerAngles.x;

            if (vCameraAngle is > 60f and < 80f or > 280f and < 300f)
                return;

            if (Mathf.Abs(angleDiff) > 120f)
                CatchPlayer();
        }

        private void CatchPlayer()
        {
            if (isCatching)
                return;

            isCatching = true;
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(Davilote grita)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            EnemyIsActive = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(1f);
            CatchManager.Singleton.CatchPlayer("DAVILOTE ENDING", "No eres un payaso, eres el circo entero.");
            audioSource.Play();
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
