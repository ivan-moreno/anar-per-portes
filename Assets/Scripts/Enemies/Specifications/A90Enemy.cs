using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Enemies/A90 Enemy")]
    public sealed class A90Enemy : Enemy
    {
        public static bool EnemyIsActive { get; private set; } = false;

        public override bool EnemyTipWasDisplayed
        {
            get => enemyTipWasDisplayed;
            set => enemyTipWasDisplayed = value;
        }

        private static bool enemyTipWasDisplayed = false;
        [SerializeField] private Image image;
        [SerializeField] private Animator jumpscareAnimator;
        [SerializeField] private AudioClip jumpscareSound;
        private AudioSource audioSource;
        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private bool isCatching = false;
        private const float checkMotionTime = 1.2f;
        private const float despawnTime = 1.5f;

        public void Spawn()
        {
            EnemyIsActive = true;
            var rngX = Random.Range(-512f, 512f);
            var rngY = Random.Range(-350f, 350f);
            image.rectTransform.anchoredPosition = new(rngX, rngY);
            image.enabled = true;
            AudioManager.Singleton.MuteDefaultAudioMixer();
            audioSource.Play();
            SubtitleManager.Singleton.PushSubtitle("(distorsiones)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            EnemyIsActive = false;
            image.enabled = false;
            checkedMotion = false;
            timeSinceSpawn = 0f;
            jumpscareAnimator.gameObject.SetActive(false);
            AudioManager.Singleton.UnmuteDefaultAudioMixer();
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (EnemyIsActive)
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
            AudioManager.Singleton.UnmuteDefaultAudioMixer();
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(grito distorsionado)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            EnemyIsActive = false;
            jumpscareAnimator.Play("Jumpscare", 0, 0f);
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(1f);
            StringBuilder stringBuilder = new();

            for (var i = 0; i < 24; i++)
                stringBuilder.Append("TE HAS MOVIDO ");

            CatchManager.Singleton.CatchPlayer(
                title: "<color=#222>A-90</color>",
                message: "<color=#222>" + stringBuilder.ToString() + "</color>",
                showUiOnScreenshot: true);
        }
    }
}
