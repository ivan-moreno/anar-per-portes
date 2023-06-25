using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Danylopez Enemy")]
    [RequireComponent(typeof(AudioSource))]
    public class DanylopezEnemy : Enemy
    {
        public static DanylopezEnemy Singleton { get; private set; }
        public static bool HasAppearedInThisSession { get; private set; } = false;

        [Header("Components")]
        [SerializeField] private GameObject overlay;
        [SerializeField] private GameObject renderTexture;
        [SerializeField] private GameObject renderTextureCamera;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnDaviloteSound;
        [SerializeField] private SoundResource spawnPedroSound;
        [SerializeField] private SoundResource spawnSangotSound;
        [SerializeField] private SoundResource spawnSheepySound;
        [SerializeField] private SoundResource spawnSkellSound;
        [SerializeField] private SoundResource meetBouserSound;
        [SerializeField] private Image danylopezCamImage;
        [SerializeField] private Sprite normalReaction;
        [SerializeField] private Sprite anxiousReaction;
        [SerializeField] private Sprite angryReaction;
        [SerializeField] private Sprite sadReaction;
        [SerializeField] private Sprite suprisedReaction;
        [SerializeField] private Sprite scaredReaction;

        Animator overlayAnimator;

        public override void Spawn()
        {
            if (EnemyIsOperative<DanylopezEnemy>())
                return;

            EnemyManager.Singleton.MarkAsOperative(this);
            HasAppearedInThisSession = true;
            overlay.SetActive(true);
            //renderTexture.SetActive(true);
            //renderTextureCamera.SetActive(true);

            overlayAnimator.Play("Draw");

            if (LatestRoomNumber() == 50)
                StartCoroutine(nameof(MeetBouserCoroutine));
            else
                StartCoroutine(nameof(SpawnEnemyCoroutine));
        }

        protected override void Despawn()
        {
            if (!EnemyIsOperative<DanylopezEnemy>())
                return;

            EnemyManager.Singleton.UnmarkAsOperative(this);
            StopCoroutine(nameof(SpawnEnemyCoroutine));
            overlay.SetActive(false);
        }

        IEnumerator MeetBouserCoroutine()
        {
            audioSource.PlayOneShot(meetBouserSound);
            yield return new WaitForSeconds(meetBouserSound.AudioClip.length + 1f);

            overlayAnimator.Play("Undraw");
            yield return new WaitForSeconds(0.6f);

            Despawn();
        }

        private void Start()
        {
            overlayAnimator = overlay.GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            HasAppearedInThisSession = false;
            PauseManager.Singleton.OnPauseChanged.AddListener(OnPauseChanged);
            BouserBossEnemy.OnSpawn.AddListener((_) => Despawn());
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private IEnumerator SpawnEnemyCoroutine()
        {
            if (LatestRoomNumber() == 50)
                yield break;

            var enemyPairPool = new List<(GameObject prefab, SoundResource spawnSound)>();

            if (!EnemyIsOperative<DaviloteEnemy>())
                enemyPairPool.Add((EnemyManager.Singleton.DaviloteEnemyPrefab, spawnDaviloteSound));

            if (!EnemyIsOperative<PedroEnemy>())
                enemyPairPool.Add((EnemyManager.Singleton.PedroEnemyPrefab, spawnPedroSound));

            if (!EnemyIsOperative<SangotEnemy>())
                enemyPairPool.Add((EnemyManager.Singleton.SangotEnemyPrefab, spawnSangotSound));

            if (!EnemyIsOperative<SheepyEnemy>())
                enemyPairPool.Add((EnemyManager.Singleton.SheepyEnemyPrefab, spawnSheepySound));

            if (!SkellHearManager.Singleton.IsHearing && !EnemyIsOperative<SkellEnemy>())
                enemyPairPool.Add((EnemyManager.Singleton.SkellEnemyPrefab, spawnSkellSound));

            var (prefab, spawnSound) = enemyPairPool.RandomItem();

            danylopezCamImage.sprite = new Sprite[]
            {
                normalReaction,
                anxiousReaction,
                angryReaction,
                sadReaction,
                suprisedReaction,
                scaredReaction
            }.RandomItem();

            audioSource.PlayOneShot(spawnSound);
            yield return new WaitForSeconds(spawnSound.AudioClip.length + 1f);

            EnemyManager.Singleton.SpawnEnemy(prefab);
            overlayAnimator.Play("Undraw");
            yield return new WaitForSeconds(0.6f);

            Despawn();
        }
    }
}
