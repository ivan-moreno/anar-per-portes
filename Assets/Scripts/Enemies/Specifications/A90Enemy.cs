using static AnarPerPortes.ShortUtils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Enemies/A90 Enemy")]
    public sealed class A90Enemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Components")]
        [SerializeField] private Image image;
        [SerializeField] private Animator jumpscareAnimator;

        [Header("Stats")]
        [SerializeField] private float checkMotionTime = 1.2f;
        [SerializeField] private float checkMotionTimeHard = 0.6f;
        [SerializeField] private float despawnTime = 1.5f;
        [SerializeField] private float despawnTimeHard = 0.9f;
        [SerializeField] private float hardmodeMinSpawnTime = 15f;
        [SerializeField] private float hardmodeMaxSpawnTime = 30f;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource jumpscareSound;

        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private bool isCatching = false;
        private float hardmodeSpawnTime = 20f;

        public void Spawn()
        {
            if (PlayerController.Singleton.IsCaught)
                return;

            IsOperative = true;
            var rngX = Random.Range(-512f, 512f);
            var rngY = Random.Range(-350f, 350f);
            image.rectTransform.anchoredPosition = new(rngX, rngY);
            image.enabled = true;
            timeSinceSpawn = 0f;
            AudioManager.Singleton.MuteAllAudioMixers();

            if (IsHardmodeEnabled())
                audioSource.time = 0.4f;

            audioSource.Play(warningSound);
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            IsOperative = false;
            image.enabled = false;
            checkedMotion = false;
            timeSinceSpawn = 0f;
            jumpscareAnimator.gameObject.SetActive(false);
            AudioManager.Singleton.UnmuteAllAudioMixers();
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            if (IsHardmodeEnabled())
                hardmodeSpawnTime = Random.Range(hardmodeMinSpawnTime, hardmodeMaxSpawnTime);
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
            if (IsHardmodeEnabled() && !IsOperative)
            {
                hardmodeSpawnTime -= Time.deltaTime;

                if (hardmodeSpawnTime <= 0f)
                {
                    hardmodeSpawnTime = Random.Range(hardmodeMinSpawnTime, hardmodeMaxSpawnTime);
                    Spawn();
                }
            }

            if (IsOperative)
                timeSinceSpawn += Time.deltaTime;

            if (!checkedMotion && timeSinceSpawn >= 0.6f && !jumpscareAnimator.gameObject.activeSelf)
            {
                jumpscareAnimator.gameObject.SetActive(true);
                jumpscareAnimator.Play("Warning", 0, IsHardmodeEnabled() ? 0.8f : 0f);
            }

            var targetCheckMotionTime = HardmodeManager.Singleton.IsHardmodeEnabled
                ? checkMotionTimeHard
                : checkMotionTime;

            if (!checkedMotion && timeSinceSpawn >= targetCheckMotionTime)
                CheckForMotion();

            if (checkedMotion && timeSinceSpawn >= (IsHardmodeEnabled() ? despawnTimeHard : despawnTime))
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
        }

        private void CatchPlayer()
        {
            if (isCatching)
                return;

            isCatching = true;
            PlayerController.Singleton.IsCaught = true;
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            jumpscareAnimator.Play("Jumpscare", 0, 0f);
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(5f);
            AudioManager.Singleton.UnmuteAllAudioMixers();
            GameManager.Singleton.RestartLevel();
        }
    }
}
