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
        private float timeSinceSpawn = 0f;
        private bool isCatching = false;
        private const float timeToDespawn = 3f;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            model = transform.GetChild(0);
            EnemyIsActive = true;
            var lastLoadedRoomTransform = RoomManager.Singleton.LastLoadedRoom.transform;
            var targetPos = lastLoadedRoomTransform.position;
            targetPos -= lastLoadedRoomTransform.forward * 8f;
            var targetRot = lastLoadedRoomTransform.rotation;
            transform.SetPositionAndRotation(targetPos, targetRot);
            audioSource.PlayOneShot(warningSound);
            SubtitleManager.Singleton.PushSubtitle("[DAVILOTE] Mira detrás de tí.", SubtitleCategory.Dialog, SubtitleSource.Hostile);
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

            timeSinceSpawn += Time.deltaTime;

            if (timeSinceSpawn >= timeToDespawn)
                Despawn();
        }

        private void FixedUpdate()
        {
            var playerAngle = PlayerController.Singleton.transform.eulerAngles.y;
            var enemyAngle = transform.eulerAngles.y;
            var angleDiff = Mathf.DeltaAngle(playerAngle, enemyAngle);

            if (Mathf.Abs(angleDiff) < 140f)
                return;

            var lineOfSightCheck =
                Physics.Linecast(
                    start: transform.position + Vector3.up,
                    end: PlayerController.Singleton.transform.position + Vector3.up,
                    hitInfo: out var hit,
                    layerMask: LayerMask.GetMask("Default", "Player"),
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player");

            if (lineOfSightCheck)
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
