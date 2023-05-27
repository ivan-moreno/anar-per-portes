using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Pause Manager")]
    public sealed class PauseManager : MonoBehaviour
    {
        public static PauseManager Singleton { get; private set; }
        public UnityEvent<bool> OnPauseChanged { get; private set; } = new();
        public bool IsPaused { get; private set; } = false;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private Button resumeButton;
        private CanvasGroup pauseMenuCanvasGroup;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            pauseMenuCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
            resumeButton.onClick.AddListener(Resume);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeybindManager.Singleton.CurrentKeybinds.Pause))
                TogglePause();

            if (IsPaused)
                pauseMenuCanvasGroup.alpha = Mathf.MoveTowards(pauseMenuCanvasGroup.alpha, 1f, Time.unscaledDeltaTime * 3f);
        }

        private void TogglePause()
        {
            if (!IsPaused)
                Pause();
            else
                Resume();

            OnPauseChanged?.Invoke(IsPaused);
        }

        private void Pause()
        {
            if (IsPaused)
                return;

            IsPaused = true;
            pauseMenu.SetActive(true);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            Time.timeScale = 0.01f;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenu.SetActive(false);
            PlayerController.Singleton.UnblockMove();
            PlayerController.Singleton.UnblockLook();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
