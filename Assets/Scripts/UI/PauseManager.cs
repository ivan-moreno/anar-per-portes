using AnarPerPortes.Enemies;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Pause Manager")]
    public sealed class PauseManager : MonoBehaviour
    {
        public static PauseManager Singleton { get; private set; }
        public bool CanPause { get; set; } = true;
        public UnityEvent<bool> OnPauseChanged { get; } = new();
        public bool IsPaused { get; private set; } = false;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private Button resumeButton;
        private Animator pauseAnimator;

        public void PauseGameLogic()
        {
            IsPaused = true;
            PlayerController.Singleton.BlockAll();
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
        }

        public void UnpauseGameLogic()
        {
            IsPaused = false;
            PlayerController.Singleton.UnblockAll();
            Time.timeScale = GameSettingsManager.Singleton.CurrentSettings.EnableSpeedrunMode ? 2f : 1f;
            OnPauseChanged?.Invoke(false);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            pauseAnimator = pauseMenu.GetComponent<Animator>();
            resumeButton.onClick.AddListener(Resume);
            pauseMenu.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeybindManager.Singleton.CurrentKeybinds.Pause))
                TogglePause();
        }

        private void TogglePause()
        {
            if (!CanPause)
                return;

            if (!IsPaused)
                Pause();
            else
                Resume();
        }

        private void Pause()
        {
            if (!CanPause)
                return;

            if (IsPaused || PlayerController.Singleton.IsCaught)
                return;

            IsPaused = true;
            pauseMenu.SetActive(true);
            pauseAnimator.Play("Draw", 0, 0f);
            StopCoroutine(nameof(ResumeEnumerator));
            PlayerController.Singleton.BlockAll();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            OnPauseChanged?.Invoke(true);
        }

        private void Resume()
        {
            if (!CanPause)
                return;

            if (!IsPaused)
                return;

            IsPaused = false;
            pauseAnimator.Play("Undraw", 0, 0f);
            StartCoroutine(nameof(ResumeEnumerator));
            PlayerController.Singleton.UnblockAll();
            Time.timeScale = GameSettingsManager.Singleton.CurrentSettings.EnableSpeedrunMode ? 2f : 1f;

            if (GameMakerEnemy.Singleton.DesktopEnabled)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;

            OnPauseChanged?.Invoke(false);
        }

        private IEnumerator ResumeEnumerator()
        {
            yield return new WaitForSeconds(0.2f);
            pauseMenu.SetActive(false);
        }
    }
}
