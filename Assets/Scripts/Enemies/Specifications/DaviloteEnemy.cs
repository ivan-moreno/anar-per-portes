using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Davilote Enemy")]
    public class DaviloteEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private float catchAngle = 120f;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource jumpscareSound;

        private bool isCatching = false;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();
            transform.rotation = PlayerController.Singleton.transform.rotation;
            audioSource.PlayOneShot(warningSound.AudioClip);
            SubtitleManager.Singleton.PushSubtitle(warningSound);
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
            transform.position = PlayerController.Singleton.transform.position - (transform.forward * 4f);
        }

        private void FixedUpdate()
        {
            var playerAngle = PlayerController.Singleton.transform.eulerAngles.y;
            var enemyAngle = transform.eulerAngles.y;
            var angleDiff = Mathf.DeltaAngle(playerAngle, enemyAngle);

            var vCameraAngle = PlayerController.Singleton.Camera.transform.eulerAngles.x;

            if (vCameraAngle is > 60f and < 80f or > 280f and < 300f)
                return;

            if (Mathf.Abs(angleDiff) > catchAngle)
                CatchPlayer();
        }

        private void CatchPlayer()
        {
            if (isCatching)
                return;

            isCatching = true;
            audioSource.PlayOneShot(jumpscareSound);
            animator.Play("Jumpscare");
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.84f);
            CatchManager.Singleton.CatchPlayer("DAVILOTE ENDING", "No eres un payaso, eres el circo entero.");
            audioSource.Play();
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            IsOperative = false;
            Destroy(gameObject);
        }
    }
}
