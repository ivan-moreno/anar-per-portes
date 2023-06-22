using static AnarPerPortes.ShortUtils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes.Enemies
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Enemies/A90 Enemy")]
    public sealed class A90Enemy : Enemy
    {
        public static A90Enemy Singleton { get; private set; }

        [Header("Components")]
        [SerializeField] private Image image;
        [SerializeField] private Animator jumpscareAnimator;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float checkMotionTime = 1.2f;
        [SerializeField][Min(0f)] private float despawnTime = 1.5f;
        [SerializeField][Min(0f)] private float despawnTimeHard = 0.9f;

        [Header("Hardmode Stats")]
        [SerializeField][Min(0f)] private float checkMotionTimeHard = 0.6f;
        [SerializeField][Min(0f)] private float hardmodeMinSpawnTime = 15f;
        [SerializeField][Min(0f)] private float hardmodeMaxSpawnTime = 30f;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource jumpscareSound;

        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private float hardmodeSpawnTime = 20f;

        public override void Spawn()
        {
            if (!PlayerController.Singleton.CanBeCaught)
            {
                Despawn();
                return;
            }

            base.Spawn();
            ChangeScreenLocation();
            AudioManager.Singleton.MuteAllAudioMixers();

            if (IsHardmodeEnabled())
                audioSource.time = 0.4f;

            audioSource.Play(warningSound);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void ChangeScreenLocation()
        {
            var rngX = Random.Range(-512f, 512f);
            var rngY = Random.Range(-350f, 350f);
            image.rectTransform.anchoredPosition = new(rngX, rngY);
            image.enabled = true;
            timeSinceSpawn = 0f;
        }

        protected override void Despawn()
        {
            if (isCatching)
                return;

            image.enabled = false;
            checkedMotion = false;
            timeSinceSpawn = 0f;
            jumpscareAnimator.gameObject.SetActive(false);
            AudioManager.Singleton.UnmuteAllAudioMixers();
            EnemyManager.Singleton.UnmarkAsOperative(this);
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
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
            if (IsHardmodeEnabled() && !EnemyIsOperative<A90Enemy>())
            {
                hardmodeSpawnTime -= Time.deltaTime;

                if (hardmodeSpawnTime <= 0f)
                {
                    hardmodeSpawnTime = Random.Range(hardmodeMinSpawnTime, hardmodeMaxSpawnTime);
                    Spawn();
                }
            }

            if (EnemyIsOperative<A90Enemy>())
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
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            if (isCatching)
                yield break;

            isCatching = true;
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.IsCaught = true;
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            jumpscareAnimator.Play("Jumpscare", 0, 0f);

            yield return new WaitForSeconds(5f);
            AudioManager.Singleton.UnmuteAllAudioMixers();
            GameManager.Singleton.RestartLevel();
        }
    }
}
