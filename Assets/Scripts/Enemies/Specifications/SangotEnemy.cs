using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Sangot Enemy")]
    public class SangotEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;
        public static UnityEvent<SangotEnemy> OnSpawned { get; } = new();

        [Header("Stats")]
        [SerializeField] private float spawnDistance = 32f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private float despawnTime = 2f;

        [Header("Sound")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource endingMusic;

        private bool isCatching = false;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            var spawnPosition = PlayerController.Singleton.transform.position;
            spawnPosition += PlayerController.Singleton.transform.forward * spawnDistance;

            var rng = Random.Range(0, 3);

            if (rng == -1)
                spawnPosition -= PlayerController.Singleton.transform.right * 8f;
            else if (rng == 1)
                spawnPosition += PlayerController.Singleton.transform.right * 8f;

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
            transform.LookAt(PlayerController.Singleton.transform.position);
            var distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Singleton.transform.position);

            if (distanceToPlayer <= catchRange)
                CatchPlayer();

            if (isCatching)
                return;

            var nextPosition = Vector3.MoveTowards(transform.position, PlayerController.Singleton.transform.position, runSpeed * Time.deltaTime);

            if (!SheepyEnemy.IsOperative && !A90Enemy.IsOperative)
                transform.position = nextPosition;
        }

        private void CatchPlayer()
        {
            if (isCatching)
                return;

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.Stop();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            yield return new WaitForSeconds(1.5f);
            audioSource.PlayOneShot(jumpscareSound);
            audioSource.spatialBlend = 0f;
            audioSource.PlayOneShot(endingMusic);
            CatchManager.Singleton.CatchPlayer("SANGOT ENDING", "youtube.com/@sangot");
        }

        private void Despawn()
        {
            IsOperative = false;
            Destroy(gameObject);
        }
    }
}