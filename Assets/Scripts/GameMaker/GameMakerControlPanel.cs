using AnarPerPortes.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Game Maker/Game Maker Control Panel")]
    public class GameMakerControlPanel : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject window;
        [SerializeField] private Button controlPanelButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private Image darknessImage;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float brightnessDecayRate = 2f;
        [SerializeField][Range(0f, 1f)] private float maxDarknessAlpha = 0.8f;

        private GameMakerEnemy gameMaker;
        private float brightnessValue = 100f;

        private void Start()
        {
            gameMaker = GetComponentInParent<GameMakerEnemy>();
            brightnessSlider.onValueChanged.AddListener(OnBrightnessSliderValueChanged);
            controlPanelButton.onClick.AddListener(OpenWindow);
            closeButton.onClick.AddListener(CloseWindow);
        }

        private void Update()
        {
            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            brightnessValue -= brightnessDecayRate * Time.deltaTime;

            var brightnessProgress = Mathf.Clamp01(brightnessValue / brightnessSlider.maxValue);
            brightnessSlider.value = brightnessValue;

            var targetAlpha = Mathf.Clamp(1f - brightnessProgress, 0f, maxDarknessAlpha);
            darknessImage.color = new(0f, 0f, 0f, targetAlpha);
        }

        private void OnBrightnessSliderValueChanged(float value)
        {
            brightnessValue = value;
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
