using System;
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
        [SerializeField] private Slider fieldOfViewSlider;
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
            fieldOfViewSlider.onValueChanged.AddListener(ChangeFieldOfViewSetting);
            subtitlesButton.onClick.AddListener(ToggleSubtitlesSetting);
            largeSubtitlesButton.onClick.AddListener(ToggleLargeSubtitlesSetting);
            lightModeButton.onClick.AddListener(ToggleLightModeSetting);
            visionMotionButton.onClick.AddListener(ToggleVisionMotionSetting);
            dyslexicFriendlyFontButton.onClick.AddListener(ToggleDyslexicFriendlyFontSetting);
            flamboyantGraphicsButton.onClick.AddListener(ToggleFlamboyantGraphicsSetting);
        }

        private void ChangeFieldOfViewSetting(float value)
        {
            CurrentSettings.FieldOfView = value;
            fieldOfViewSlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Campo de visión <color=#88EEEE>({CurrentSettings.FieldOfView:0})</color>";
        }

        //TODO: Convert into a dropdown
        private void ToggleSubtitlesSetting()
        {
            var areSubtitlesEnabled = CurrentSettings.SubtitlesSetting is SubtitlesSetting.DialogAndSoundEffects;

            CurrentSettings.SubtitlesSetting = areSubtitlesEnabled
                ? SubtitlesSetting.Disabled
                : SubtitlesSetting.DialogAndSoundEffects;

            subtitlesButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Subtítulos", !areSubtitlesEnabled);
        }

        private void ToggleLargeSubtitlesSetting()
        {
            CurrentSettings.EnableLargeSubtitles = !CurrentSettings.EnableLargeSubtitles;
            largeSubtitlesButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Subtítulos grandes", CurrentSettings.EnableLargeSubtitles);
        }

        private void ToggleLightModeSetting()
        {
            CurrentSettings.EnableLightMode = !CurrentSettings.EnableLightMode;
            lightModeButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Iluminar salas", CurrentSettings.EnableLightMode);
        }

        private void ToggleVisionMotionSetting()
        {
            CurrentSettings.EnableVisionMotion = !CurrentSettings.EnableVisionMotion;
            visionMotionButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Meneo de la cámara", CurrentSettings.EnableVisionMotion);
        }

        private void ToggleDyslexicFriendlyFontSetting()
        {
            CurrentSettings.EnableDyslexicFriendlyFont = !CurrentSettings.EnableDyslexicFriendlyFont;
            dyslexicFriendlyFontButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Fuente apta para disléxicos", CurrentSettings.EnableDyslexicFriendlyFont);
        }

        private void ToggleFlamboyantGraphicsSetting()
        {
            CurrentSettings.EnableFlamboyantGraphics = !CurrentSettings.EnableFlamboyantGraphics;
            flamboyantGraphicsButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Gráficos importantes llamativos", CurrentSettings.EnableFlamboyantGraphics);
        }

        private string GetSettingText(string text, bool isOn)
        {
            return text + (isOn ? " <color=#88CCFF>(SÍ)</color>" : " <color=#FF4444>(NO)</color>");
        }

        //TODO: Save and load settings on disk file.
        private void LoadSerializedSettings()
        {
            CurrentSettings = new();
        }
    }
}
