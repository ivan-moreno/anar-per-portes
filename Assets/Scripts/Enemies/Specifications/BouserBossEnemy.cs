using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Bouser Boss Enemy")]
    public class BouserBossEnemy : Enemy
    {
        public static UnityEvent<BouserBossEnemy> OnSpawn { get; } = new();

        [SerializeField] private AudioSource voiceSource;

        [Header("Stats")]
        [SerializeField] private float timeBetweenActivites = 3f;
        [SerializeField] private float startWaitTime = 5f;

        [Header("Audio")]
        [SerializeField] private SoundResource startVoice;
        [SerializeField] private SoundResource[] punchAttackVoices;
        [SerializeField] private AudioClip startMusic;
        [SerializeField] private AudioClip battleMusic;

        private bool isWaiting = true;
        private bool isDoingActivity = false;
        private float nextActivityCooldown;

        public override void Spawn()
        {
            base.Spawn();
            animator = GetComponent<Animator>();
            model = animator.transform;
            audioSource = voiceSource;

            PauseManager.Singleton.OnPauseChanged.AddListener(OnPauseChanged);
            OnSpawn?.Invoke(this);

            StartCoroutine(nameof(SpawnCoroutine));
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void Start()
        {
            Spawn();
        }

        private void Update()
        {
            if (isWaiting)
                return;

            if (!isDoingActivity)
                nextActivityCooldown -= Time.deltaTime;

            if (nextActivityCooldown <= 0f)
            {
                //random activity
                StartCoroutine(nameof(PunchAttackCoroutine));
                nextActivityCooldown = timeBetweenActivites;
            }
        }

        private IEnumerator SpawnCoroutine()
        {
            nextActivityCooldown = startWaitTime;
            PlayerSound(startMusic);
            yield return new WaitForSeconds(2f);

            audioSource.PlayOneShot(startVoice);
            yield return new WaitForSeconds(startMusic.length - 2f);
            isWaiting = false;
            AudioManager.Singleton.SetVolume(1f);
            AudioManager.Singleton.PlayMusic(battleMusic);
        }

        private IEnumerator PunchAttackCoroutine()
        {
            if (isWaiting || isDoingActivity)
                yield break;

            isDoingActivity = true;
            animator.Play("PunchStart");
            audioSource.PlayOneShot(punchAttackVoices.RandomItem());
            yield return new WaitForSeconds(2f);

            animator.Play("PunchExecute");
            yield return new WaitForSeconds(4f);

            isDoingActivity = false;
        }
    }
}
