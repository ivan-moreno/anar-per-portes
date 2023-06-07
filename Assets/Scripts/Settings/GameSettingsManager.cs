using Newtonsoft.Json;
using System.Collections;
using System.IO;
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
        [SerializeField] private GameObject settingsScreen;
        [SerializeField] private Button saveSettingsButton;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Slider fieldOfViewSlider;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button smaaButton;
        [SerializeField] private Button postProcessingButton;
        [SerializeField] private Button subtitlesButton;
        [SerializeField] private Button largeSubtitlesButton;
        [SerializeField] private Button lightModeButton;
        [SerializeField] private Button visionMotionButton;
        [SerializeField] private Button dyslexicFriendlyFontButton;
        [SerializeField] private Button flamboyantGraphicsButton;
        [SerializeField] private Button enemyTipsButton;
        [SerializeField] private Button speedRunModeButton;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            LoadSerializedSettings();
            saveSettingsButton.onClick.AddListener(SaveSettingsButtonClicked);
            mouseSensitivitySlider.onValueChanged.AddListener(ChangeMouseSensitivitySetting);
            fieldOfViewSlider.onValueChanged.AddListener(ChangeFieldOfViewSetting);
            volumeSlider.onValueChanged.AddListener(ChangeVolumeSetting);
            smaaButton.onClick.AddListener(ToggleSmaaSetting);
            postProcessingButton.onClick.AddListener(TogglePostProcessingSetting);
            subtitlesButton.onClick.AddListener(ToggleSubtitlesSetting);
            largeSubtitlesButton.onClick.AddListener(ToggleLargeSubtitlesSetting);
            lightModeButton.onClick.AddListener(ToggleLightModeSetting);
            visionMotionButton.onClick.AddListener(ToggleVisionMotionSetting);
            dyslexicFriendlyFontButton.onClick.AddListener(ToggleDyslexicFriendlyFontSetting);
            flamboyantGraphicsButton.onClick.AddListener(ToggleFlamboyantGraphicsSetting);
            enemyTipsButton.onClick.AddListener(ToggleEnemyTipsSetting);
            speedRunModeButton.onClick.AddListener(ToggleSpeedRunModeSetting);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeybindManager.Singleton.CurrentKeybinds.Pause) && settingsScreen.activeSelf)
                settingsScreen.SetActive(false);
        }

        private void SaveSettingsButtonClicked()
        {
            OnCurrentSettingsChanged?.Invoke();
            SerializeSettings();
        }

        private void ChangeMouseSensitivitySetting(float value)
        {
            CurrentSettings.HMouseSensitivity = CurrentSettings.VMouseSensitivity = value;
            mouseSensitivitySlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Sensibilidad del ratón <color=#88EEEE>({CurrentSettings.HMouseSensitivity / 100f:0.0})</color>";
        }

        private void ChangeFieldOfViewSetting(float value)
        {
            CurrentSettings.FieldOfView = value;
            fieldOfViewSlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Campo de visión <color=#88EEEE>({CurrentSettings.FieldOfView:0})</color>";
        }

        private void ChangeVolumeSetting(float value)
        {
            CurrentSettings.Volume = value;
            volumeSlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Volumen <color=#88EEEE>({CurrentSettings.Volume * 100f:0}%)</color>";
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

        private void ToggleSubtitlesSetting()
        {
            CurrentSettings.EnableSubtitles = !CurrentSettings.EnableSubtitles;
            subtitlesButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Subtítulos", CurrentSettings.EnableSubtitles);
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

        //TODO: Convert into a dropdown
        private void ToggleEnemyTipsSetting()
        {
            var areEnemyTipsEnabled = CurrentSettings.EnemyTipSetting is EnemyTipSetting.ShowOnFirstEncounterAndWhenCaught;

            CurrentSettings.EnemyTipSetting = areEnemyTipsEnabled
                ? EnemyTipSetting.Disabled
                : EnemyTipSetting.ShowOnFirstEncounterAndWhenCaught;

            enemyTipsButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Mostrar ayuda sobre nuevos enemigos", !areEnemyTipsEnabled);
        }

        private void ToggleSpeedRunModeSetting()
        {
            CurrentSettings.EnableSpeedrunMode = !CurrentSettings.EnableSpeedrunMode;
            speedRunModeButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Modo Speedrun", CurrentSettings.EnableSpeedrunMode);
        }

        private string GetSettingText(string text, bool isOn)
        {
            return text + (isOn ? " <color=#88CCFF>(SÍ)</color>" : " <color=#FF4444>(NO)</color>");
        }

        //TODO: Save and load settings on disk file.
        private void LoadSerializedSettings()
        {
            var path = Application.persistentDataPath + Path.DirectorySeparatorChar + "settings.json";

            if (File.Exists(path))
            {
                var settings = File.ReadAllText(path);
                
                CurrentSettings = JsonConvert.DeserializeObject<GameSettings>(settings);
            }
            else
            {
                CurrentSettings = new();
            }

            StartCoroutine(nameof(ApplyLoadedSettings));
            RefreshSettingLabels();
        }

        private IEnumerator ApplyLoadedSettings()
        {
            yield return new WaitForEndOfFrame();
            OnCurrentSettingsChanged?.Invoke();
        }

        private void SerializeSettings()
        {
            var json = JsonConvert.SerializeObject(CurrentSettings);
            var path = Application.persistentDataPath + Path.DirectorySeparatorChar + "settings.json";
            File.WriteAllText(path, json);
        }

        private void RefreshSettingLabels()
        {
            mouseSensitivitySlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Sensibilidad del ratón <color=#88EEEE>({CurrentSettings.HMouseSensitivity / 100f:0.0})</color>";
            fieldOfViewSlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Campo de visión <color=#88EEEE>({CurrentSettings.FieldOfView:0})</color>";
            volumeSlider.transform.parent.GetComponentInChildren<TMP_Text>().text = $"Volumen <color=#88EEEE>({CurrentSettings.Volume * 100f:0}%)</color>";
            flamboyantGraphicsButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Gráficos importantes llamativos", CurrentSettings.EnableFlamboyantGraphics);
            smaaButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Antialias (SMAA)", CurrentSettings.EnableSmaa);
            postProcessingButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Efectos de postprocesado", CurrentSettings.EnablePostProcessing);
            subtitlesButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Subtítulos", CurrentSettings.EnableSubtitles);
            largeSubtitlesButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Subtítulos grandes", CurrentSettings.EnableLargeSubtitles);
            lightModeButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Iluminar salas", CurrentSettings.EnableLightMode);
            visionMotionButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Meneo de la cámara", CurrentSettings.EnableVisionMotion);
            dyslexicFriendlyFontButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Fuente apta para disléxicos", CurrentSettings.EnableDyslexicFriendlyFont);
            flamboyantGraphicsButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Gráficos importantes llamativos", CurrentSettings.EnableFlamboyantGraphics);
            speedRunModeButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Modo Speedrun", CurrentSettings.EnableSpeedrunMode);
            var areEnemyTipsEnabled = CurrentSettings.EnemyTipSetting is EnemyTipSetting.ShowOnFirstEncounterAndWhenCaught;
            enemyTipsButton.GetComponentInChildren<TMP_Text>().text = GetSettingText("Mostrar ayuda sobre nuevos enemigos", areEnemyTipsEnabled);
        }
    }
}
