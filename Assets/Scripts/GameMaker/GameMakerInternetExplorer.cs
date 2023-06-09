using AnarPerPortes.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Game Maker/Game Maker Internet Explorer")]
    public class GameMakerInternetExplorer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject window;
        [SerializeField] private GameObject ongoingDownloadBody;
        [SerializeField] private GameObject cancelledDownloadBody;
        [SerializeField] private Button internetExplorerButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button cancelDownloadButton;
        [SerializeField] private Slider downloadSlider;
        [SerializeField] private Image wifiImage;

        [Header("References")]
        [SerializeField] private Sprite[] wifiSprites;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float minSpawnTime = 30f;
        [SerializeField][Min(0f)] private float maxSpawnTime = 60f;
        [SerializeField][Min(0.01f)] private float downloadDuration = 10f;

        private GameMakerEnemy gameMaker;
        private bool isFunctioning = false;
        private float spawnTime;
        private float downloadTime;

        private void Start()
        {
            gameMaker = GetComponentInParent<GameMakerEnemy>();
            spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            cancelDownloadButton.onClick.AddListener(Despawn);
            internetExplorerButton.onClick.AddListener(OpenWindow);
            closeButton.onClick.AddListener(CloseWindow);
        }

        private void Update()
        {
            if (isFunctioning)
                UpdateFunctioning();
            else
                UpdateNonFunctioning();
        }

        private void UpdateNonFunctioning()
        {
            spawnTime -= Time.deltaTime;

            if (spawnTime <= 0f)
                Spawn();
        }

        private void UpdateFunctioning()
        {
            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            downloadTime += Time.deltaTime;

            var downloadProgress = Mathf.Clamp01(downloadTime / downloadDuration);
            downloadSlider.value = downloadSlider.maxValue * downloadProgress;

            var targetSprite = Mathf.CeilToInt(downloadProgress * wifiSprites.Length - 1);
            wifiImage.sprite = wifiSprites[targetSprite];

            if (downloadTime >= downloadDuration)
                gameMaker.CatchPlayer();
        }

        private void Spawn()
        {
            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            if (isFunctioning)
                return;

            isFunctioning = true;
            ongoingDownloadBody.SetActive(true);
            cancelledDownloadBody.SetActive(false);
        }

        private void Despawn()
        {
            if (!isFunctioning)
                return;

            isFunctioning = false;
            downloadTime = 0f;
            spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            cancelledDownloadBody.SetActive(true);
            ongoingDownloadBody.SetActive(false);
            wifiImage.sprite = wifiSprites[0];
        }

        void OpenWindow()
        {
            if (window.activeSelf)
                return;

            window.SetActive(true);
        }

        void CloseWindow()
        {
            if (!window.activeSelf)
                return;

            window.SetActive(false);
        }
    }
}
