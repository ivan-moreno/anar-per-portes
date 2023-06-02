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
        }

        private void Awake()
        {
            Singleton = this;
            Time.timeScale = 1f;
        }

        private IEnumerator RestartLevelEnumerator()
        {
            FadeScreenManager.Singleton.Display();
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(0);
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Missing Managers")]
        private void GenerateMissingManagers()
        {
            GenerateMissingManager<AccessibilityFontManager>();
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
