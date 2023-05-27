using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Missing Managers")]
        private void GenerateMissingManagers()
        {
            GenerateMissingManager<AccessibilityFontManager>();
            GenerateMissingManager<CatchManager>();
            GenerateMissingManager<EnemyManager>();
            GenerateMissingManager<EnemyTipManager>();
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
