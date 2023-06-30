using AnarPerPortes.Enemies;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AnarPerPortes.ShortUtils;

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
        [SerializeField] private TMP_Text roomRecordLabel;
        [SerializeField] private TMP_Text playTimeLabel;
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

            var minutes = Mathf.Floor(Time.realtimeSinceStartup / 60f);
            var seconds = Mathf.Floor(Time.realtimeSinceStartup % 60f);

            playTimeLabel.text = minutes.ToString("0") + ":" + (seconds < 10 ? "0" : "") + seconds.ToString("0");

            var recordOfRooms = PlayerPrefs.HasKey("RecordOfRooms") ? PlayerPrefs.GetInt("RecordOfRooms") : 0;

            if (LatestRoomNumber() > recordOfRooms)
            {
                PlayerPrefs.SetInt("RecordOfRooms", LatestRoomNumber());
                recordOfRooms = LatestRoomNumber();
            }

            roomRecordLabel.text = recordOfRooms.ToString();
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

            Cursor.lockState = GameMakerEnemy.Singleton.DesktopEnabled ? CursorLockMode.None : CursorLockMode.Locked;

            OnPauseChanged?.Invoke(false);
        }

        private IEnumerator ResumeEnumerator()
        {
            yield return new WaitForSeconds(0.2f);
            pauseMenu.SetActive(false);
        }
    }
}
