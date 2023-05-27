using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Game Settings Manager")]
    public sealed class GameSettingsManager : MonoBehaviour
    {
        public static GameSettingsManager Singleton { get; private set; }
        public GameSettings CurrentSettings { get; private set; }
        public UnityEvent OnCurrentSettingsChanged { get; private set; } = new();
        [SerializeField] private Button saveSettingsButton;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Slider fieldOfViewSlider;
        [SerializeField] private Button smaaButton;
        [SerializeField] private Button postProcessingButton;
        [SerializeField] private Button subtitlesButton;
        [SerializeField] private Button largeSubtitlesButton;
        [SerializeField] private Button lightModeButton;
        [SerializeField] private Button visionMotionButton;
        [SerializeField] private Button dyslexicFriendlyFontButton;
        [SerializeField] private Button flamboyantGraphicsButton;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            LoadSerializedSettings();
            saveSettingsButton.onClick.AddListener(() => OnCurrentSettingsChanged?.Invoke());
            mouseSensitivitySlider.onValueChanged.AddListener(ChangeMouseSensitivitySetting);
            fieldOfViewSlider.onValueChanged.AddListener(ChangeFieldOfViewSetting);
            smaaButton.onClick.AddListener(ToggleSmaaSetting);
            postProcessingButton.onClick.AddListener(TogglePostProcessingSetting);
            subtitlesButton.onClick.AddListener(ToggleSubtitlesSetting);
            largeSubtitlesButton.onClick.AddListener(ToggleLargeSubtitlesSetting);
            lightModeButton.onClick.AddListener(ToggleLightModeSetting);
            visionMotionButton.onClick.AddListener(ToggleVisionMotionSetting);
            dyslexicFriendlyFontButton.onClick.AddListener(ToggleDyslexicFriendlyFontSetting);
            flamboyantGraphicsButton.onClick.AddListener(ToggleFlamboyantGraphicsSetting);
        }

        private void ChangeMouseSensitivitySetting(float value)
        {
            CurrentSettings.HMouseSensitivity = CurrentSettings.VMouseSensitivity = value;
            mouseSensitivitySlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Sensibilidad del rat�n <color=#88EEEE>({CurrentSettings.HMouseSensitivity / 100f:0.0})</color>";
        }

        private void ChangeFieldOfViewSetting(float value)
        {
            CurrentSettings.FieldOfView = value;
            fieldOfViewSlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Campo de visi�n <color=#88EEEE>({CurrentSettings.FieldOfView:0})</color>";
        }

        private void ToggleSmaaSetting()
        {
            CurrentSettings.EnableSmaa = !CurrentSettings.EnableSmaa;
            smaaButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Antialias (SMAA)", CurrentSettings.EnableSmaa);
        }

        private void TogglePostProcessingSetting()
        {
            CurrentSettings.EnablePostProcessing = !CurrentSettings.EnablePostProcessing;
            postProcessingButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Efectos de postprocesado", CurrentSettings.EnablePostProcessing);
        }

        //TODO: Convert into a dropdown
        private void ToggleSubtitlesSetting()
        {
            var areSubtitlesEnabled = CurrentSettings.SubtitlesSetting is SubtitlesSetting.DialogAndSoundEffects;

            CurrentSettings.SubtitlesSetting = areSubtitlesEnabled
                ? SubtitlesSetting.Disabled
                : SubtitlesSetting.DialogAndSoundEffects;

            subtitlesButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Subt�tulos", !areSubtitlesEnabled);
        }

        private void ToggleLargeSubtitlesSetting()
        {
            CurrentSettings.EnableLargeSubtitles = !CurrentSettings.EnableLargeSubtitles;
            largeSubtitlesButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Subt�tulos grandes", CurrentSettings.EnableLargeSubtitles);
        }

        private void ToggleLightModeSetting()
        {
            CurrentSettings.EnableLightMode = !CurrentSettings.EnableLightMode;
            lightModeButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Iluminar salas", CurrentSettings.EnableLightMode);
        }

        private void ToggleVisionMotionSetting()
        {
            CurrentSettings.EnableVisionMotion = !CurrentSettings.EnableVisionMotion;
            visionMotionButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Meneo de la c�mara", CurrentSettings.EnableVisionMotion);
        }

        private void ToggleDyslexicFriendlyFontSetting()
        {
            CurrentSettings.EnableDyslexicFriendlyFont = !CurrentSettings.EnableDyslexicFriendlyFont;
            dyslexicFriendlyFontButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Fuente apta para disl�xicos", CurrentSettings.EnableDyslexicFriendlyFont);
        }

        private void ToggleFlamboyantGraphicsSetting()
        {
            CurrentSettings.EnableFlamboyantGraphics = !CurrentSettings.EnableFlamboyantGraphics;
            flamboyantGraphicsButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Gr�ficos importantes llamativos", CurrentSettings.EnableFlamboyantGraphics);
        }

        private string GetSettingText(string text, bool isOn)
        {
            return text + (isOn ? " <color=#88CCFF>(S�)</color>" : " <color=#FF4444>(NO)</color>");
        }

        //TODO: Save and load settings on disk file.
        private void LoadSerializedSettings()
        {
            CurrentSettings = new();
        }
    }
}
