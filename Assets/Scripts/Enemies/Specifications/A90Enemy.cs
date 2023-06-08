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
        [SerializeField] private float despawnTime = 1.5f;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource jumpscareSound;

        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private bool isCatching = false;

        public void Spawn()
        {
            IsOperative = true;
            var rngX = Random.Range(-512f, 512f);
            var rngY = Random.Range(-350f, 350f);
            image.rectTransform.anchoredPosition = new(rngX, rngY);
            image.enabled = true;
            AudioManager.Singleton.MuteAllAudioMixers();
            audioSource.Play(warningSound);
            SubtitleManager.Singleton.PushSubtitle("(distorsiones)", Team.Hostile);
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
            if (IsOperative)
                timeSinceSpawn += Time.deltaTime;

            if (!checkedMotion && timeSinceSpawn >= 0.6f && !jumpscareAnimator.gameObject.activeSelf)
            {
                jumpscareAnimator.gameObject.SetActive(true);
                jumpscareAnimator.Play("Warning", 0, 0f);
            }

            if (!checkedMotion && timeSinceSpawn >= checkMotionTime)
                CheckForMotion();

            if (checkedMotion && timeSinceSpawn >= despawnTime)
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
            AudioManager.Singleton.UnmuteAllAudioMixers();
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            jumpscareAnimator.Play("Jumpscare", 0, 0f);
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(5f);
            GameManager.Singleton.RestartLevel();
        }
    }
}
