using AnarPerPortes.Enemies;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Game Maker/Game Maker Media Player")]
    [RequireComponent(typeof(AudioSource))]
    public class GameMakerMediaPlayer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject window;
        [SerializeField] private Button mediaPlayerButton;
        [SerializeField] private Button closeButton;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float timeToOpen = 30f;
        [SerializeField][Min(0f)] private float openDuration = 6f;

        [Header("Audio")]
        [SerializeField] private SoundResource errorSound;
        [SerializeField] private SoundResource appearSound;

        private GameMakerEnemy gameMaker;
        private AudioSource audioSource;
        private bool isOpen = false;
        private float curTimeToOpen;
        private float timeSinceOpen;

        private void Start()
        {
            gameMaker = GetComponentInParent<GameMakerEnemy>();
            audioSource = GetComponent<AudioSource>();
            mediaPlayerButton.onClick.AddListener(RestartApparition);
            closeButton.onClick.AddListener(Close);
            PauseManager.Singleton.OnPauseChanged.AddListener(OnPauseChanged);
            PlayerController.Singleton.OnBeginCatchSequence.AddListener(audioSource.Stop);
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void Update()
        {
            if (isOpen)
                UpdateOpen();
            else
                UpdateNotOpen();
        }

        private void UpdateOpen()
        {
            timeSinceOpen += Time.deltaTime;

            closeButton.enabled = timeSinceOpen >= openDuration;
        }

        private void UpdateNotOpen()
        {
            curTimeToOpen += Time.deltaTime;

            if (curTimeToOpen >= timeToOpen)
                Open();
        }

        private void Open()
        {
            if (isOpen)
                return;

            isOpen = true;
            window.SetActive(true);
            audioSource.Play(appearSound);
        }

        private void Close()
        {
            if (!isOpen || timeSinceOpen < openDuration)
                return;

            isOpen = false;
            timeSinceOpen = 0f;
            curTimeToOpen = 0f;
            window.SetActive(false);
            audioSource.Stop();
        }

        private void RestartApparition()
        {
            if (isOpen)
                return;

            curTimeToOpen = 0f;
            audioSource.PlayOneShot(errorSound.AudioClip);
        }
    }
}
