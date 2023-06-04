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
        private AudioSource audioSource;
        private float timeSinceSpawn = 0f;
        private const float timeToDespawn = 1.3f;

        public void Spawn()
        {
            EnemyIsActive = true;
            var rngX = Random.Range(-512f, 512f);
            var rngY = Random.Range(-350f, 350f);
            image.rectTransform.anchoredPosition = new(rngX, rngY);
            image.enabled = true;
            AudioManager.Singleton.MuteDefaultAudioMixer();
            audioSource.Play();
        }

        private void Despawn()
        {
            EnemyIsActive = false;
            timeSinceSpawn = 0f;
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

            if (timeSinceSpawn >= timeToDespawn)
                Despawn();
        }
    }
}
