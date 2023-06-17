using AnarPerPortes.Enemies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Game Maker/Game Maker Problem Detector")]
    [RequireComponent(typeof(AudioSource))]
    public class GameMakerProblemDetector : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject window;
        [SerializeField] private GameObject biosScreen;
        [SerializeField] private Transform biosOptionGroup;
        [SerializeField] private Button problemDetectorButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text foundProblemLabel;
        [SerializeField] private string[] problemTitles;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float minSpawnTime = 60f;
        [SerializeField][Min(0f)] private float maxSpawnTime = 120f;

        private GameMakerEnemy gameMaker;
        private AudioSource audioSource;
        private int currentDetectedProblem;
        private int selectedProblem;
        private float spawnTime;
        private float blinkTimer;

        private void Start()
        {
            gameMaker = GetComponentInParent<GameMakerEnemy>();
            audioSource = GetComponent<AudioSource>();
            spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            TargetRandomProblem();
            problemDetectorButton.onClick.AddListener(OpenWindow);
            closeButton.onClick.AddListener(CloseWindow);
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
            if (biosScreen.activeSelf)
            {
                UpdateBios();
                return;
            }

            spawnTime -= Time.deltaTime;

            if (spawnTime <= 0f)
                OpenBios();

            blinkTimer -= Time.deltaTime;

            if (blinkTimer <= 0f)
                ToggleProblemBlink();
        }

        private void UpdateBios()
        {
            if (Input.GetKeyDown(KeybindManager.Singleton.CurrentKeybinds.MoveForward))
            {
                if (selectedProblem != 0)
                    selectedProblem--;

                RefreshOptions();
            }
            else if (Input.GetKeyDown(KeybindManager.Singleton.CurrentKeybinds.MoveBackward))
            {
                if (selectedProblem != problemTitles.Length - 1)
                    selectedProblem++;

                RefreshOptions();
            }

            if (Input.GetKeyDown(KeyCode.Return))
                EnterOption();
        }

        private void RefreshOptions()
        {
            for (var i = 0; i < biosOptionGroup.childCount; i++)
            {
                if (i == selectedProblem)
                {
                    biosOptionGroup.GetChild(i).GetComponent<Image>().color = Color.white;
                    biosOptionGroup.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.black;
                }
                else
                {
                    biosOptionGroup.GetChild(i).GetComponent<Image>().color = Color.black;
                    biosOptionGroup.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.white;
                }
            }
        }

        private void ToggleProblemBlink()
        {
            blinkTimer = 0.3f;
            foundProblemLabel.gameObject.SetActive(!foundProblemLabel.gameObject.activeSelf);
        }

        private void OpenWindow()
        {
            if (window.activeSelf)
                return;

            blinkTimer = 0.5f;
            foundProblemLabel.gameObject.SetActive(true);
            window.SetActive(true);
        }

        private void CloseWindow()
        {
            if (!window.activeSelf)
                return;

            window.SetActive(false);
        }

        private void TargetRandomProblem()
        {
            currentDetectedProblem = Random.Range(0, problemTitles.Length);
            foundProblemLabel.text = problemTitles[currentDetectedProblem];
        }

        private void OpenBios()
        {
            if (biosScreen.activeSelf)
                return;

            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            biosScreen.SetActive(true);
            audioSource.Play();
        }

        private void EnterOption()
        {
            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            audioSource.Stop();

            if (selectedProblem != currentDetectedProblem)
            {
                gameMaker.CatchPlayer();
                return;
            }

            biosScreen.SetActive(false);
            selectedProblem = 0;
            spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            TargetRandomProblem();
        }
    }
}
