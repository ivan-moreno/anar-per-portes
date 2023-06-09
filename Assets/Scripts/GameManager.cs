using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Singleton { get; private set; }

        public void RestartLevel()
        {
            StartCoroutine(nameof(RestartLevelEnumerator));
        }

        public void QuitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            OnSettingsChanged();
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            if (!PauseManager.Singleton.IsPaused)
                Time.timeScale = GameSettingsManager.Singleton.CurrentSettings.EnableSpeedrunMode ? 2f : 1f;

            Screen.fullScreen = GameSettingsManager.Singleton.CurrentSettings.EnableFullscreen;
            QualitySettings.vSyncCount = GameSettingsManager.Singleton.CurrentSettings.EnableVSync ? 1 : 0;
        }

        private IEnumerator RestartLevelEnumerator()
        {
            FadeScreenManager.Singleton.Display();
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = GameSettingsManager.Singleton.CurrentSettings.EnableSpeedrunMode ? 2f : 1f;
            yield return SceneManager.LoadSceneAsync(0);
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Missing Managers")]
        private void GenerateMissingManagers()
        {
            GenerateMissingManager<AccessibilityFontManager>();
            GenerateMissingManager<AudioManager>();
            GenerateMissingManager<BlurOverlayManager>();
            GenerateMissingManager<CatchManager>();
            GenerateMissingManager<EnemyManager>();
            GenerateMissingManager<EnemyTipManager>();
            GenerateMissingManager<FadeScreenManager>();
            GenerateMissingManager<FlamboyantGraphicManager>();
            GenerateMissingManager<GameSettingsManager>();
            GenerateMissingManager<InteractionManager>();
            GenerateMissingManager<ItemManager>();
            GenerateMissingManager<KeybindManager>();
            GenerateMissingManager<LightModeManager>();
            GenerateMissingManager<PauseManager>();
            GenerateMissingManager<RoomManager>();
            GenerateMissingManager<SubtitleManager>();
            GenerateMissingManager<SkellHearManager>();
            GenerateMissingManager<VolumeManager>();
        }

        private void GenerateMissingManager<T>() where T : Component
        {
            var foundManager = GetComponentInChildren<T>() != null;

            if (foundManager)
                return;

            new GameObject(typeof(T).Name, typeof(T)).transform.SetParent(transform);
        }
#endif

    }
}
