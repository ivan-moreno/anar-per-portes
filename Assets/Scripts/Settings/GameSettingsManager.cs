using Newtonsoft.Json;
using System.Collections;
using System.IO;
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
        public UnityEvent OnApplySettings { get; } = new();
        public UnityEvent OnCurrentSettingsChanged { get; } = new();

        [Header("Components")]
        [SerializeField] private GameObject settingsScreen;
        [SerializeField] private Button saveSettingsButton;
        [SerializeField] private Button gameplayGroupButton;
        [SerializeField] private Button accessibilityGroupButton;
        [SerializeField] private Button controlsGroupButton;
        [SerializeField] private Button graphicsGroupButton;
        [SerializeField] private Button audioGroupButton;
        [SerializeField] private Transform gameplayGroup;
        [SerializeField] private Transform accessibilityGroup;
        [SerializeField] private Transform controlsGroup;
        [SerializeField] private Transform graphicsGroup;
        [SerializeField] private Transform audioGroup;

        [Header("Prefabs")]
        [SerializeField] private GameObject toggleWidgetPrefab;
        [SerializeField] private GameObject sliderWidgetPrefab;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            LoadSerializedSettings();
            GenerateWidgets();
            ChangeVisibleGroup(gameplayGroup);
            saveSettingsButton.onClick.AddListener(OnSaveButtonClicked);
            gameplayGroupButton.onClick.AddListener(() => ChangeVisibleGroup(gameplayGroup));
            accessibilityGroupButton.onClick.AddListener(() => ChangeVisibleGroup(accessibilityGroup));
            controlsGroupButton.onClick.AddListener(() => ChangeVisibleGroup(controlsGroup));
            graphicsGroupButton.onClick.AddListener(() => ChangeVisibleGroup(graphicsGroup));
            audioGroupButton.onClick.AddListener(() => ChangeVisibleGroup(audioGroup));
        }

        private void LoadSerializedSettings()
        {
            var path = Application.persistentDataPath + Path.DirectorySeparatorChar + "settings.json";

            if (File.Exists(path))
            {
                var settings = File.ReadAllText(path);
                CurrentSettings = JsonConvert.DeserializeObject<GameSettings>(settings);
            }
            else
                CurrentSettings = new();

            StartCoroutine(nameof(ApplyLoadedSettings));
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

        private void OnSaveButtonClicked()
        {
            OnApplySettings?.Invoke();
            OnCurrentSettingsChanged?.Invoke();
            SerializeSettings();
        }

        private void GenerateWidgets()
        {
            GenerateSlider(gameplayGroup)
                .Initialize("Campo de visión", CurrentSettings.FieldOfView, 60f, 100f)
                .WithWholeNumbers()
                .WithTarget(target => CurrentSettings.FieldOfView = target);

            GenerateToggle(gameplayGroup)
                .Initialize("Consejos sobre enemigos", CurrentSettings.EnableEnemyTips)
                .WithTarget(target => CurrentSettings.EnableEnemyTips = target);

            GenerateToggle(gameplayGroup)
                .Initialize("Alucinaciones", CurrentSettings.EnableHallucinations)
                .WithTarget(target => CurrentSettings.EnableHallucinations = target);

            GenerateToggle(gameplayGroup)
                .Initialize("Modo Speedrun", CurrentSettings.EnableSpeedrunMode)
                .WithTarget(target => CurrentSettings.EnableSpeedrunMode = target);

            GenerateSlider(controlsGroup)
                .Initialize("Sensibilidad del ratón", CurrentSettings.MouseSensitivity, 0f, 500f)
                .WithWholeNumbers()
                .WithTarget(target => CurrentSettings.MouseSensitivity = target);

            GenerateToggle(accessibilityGroup)
                .Initialize("Subtítulos", CurrentSettings.EnableSubtitles)
                .WithTarget(target => CurrentSettings.EnableSubtitles = target);

            GenerateToggle(accessibilityGroup)
                .Initialize("Subtítulos grandes", CurrentSettings.EnableLargeSubtitles)
                .WithTarget(target => CurrentSettings.EnableLargeSubtitles = target);

            GenerateToggle(accessibilityGroup)
                .Initialize("Fuente OpenDyslexic", CurrentSettings.EnableOpenDyslexicFont)
                .WithTarget(target => CurrentSettings.EnableOpenDyslexicFont = target);

            GenerateToggle(accessibilityGroup)
                .Initialize("Iluminar salas", CurrentSettings.EnableRoomBrighten)
                .WithTarget(target => CurrentSettings.EnableRoomBrighten = target);

            GenerateToggle(accessibilityGroup)
                .Initialize("Resaltar elementos importantes", CurrentSettings.EnableFlamboyantGraphics)
                .WithTarget(target => CurrentSettings.EnableFlamboyantGraphics = target);

            GenerateToggle(accessibilityGroup)
                .Initialize("Meneo de la cámara", CurrentSettings.EnableVisionMotion)
                .WithTarget(target => CurrentSettings.EnableVisionMotion = target);

            GenerateToggle(graphicsGroup)
                .Initialize("Pantalla completa", CurrentSettings.EnableFullscreen)
                .WithTarget(target => CurrentSettings.EnableFullscreen = target);

            GenerateToggle(graphicsGroup)
                .Initialize("Sincronización vertical", CurrentSettings.EnableVSync)
                .WithTarget(target => CurrentSettings.EnableVSync = target);

            GenerateSlider(graphicsGroup)
                .Initialize("Contraste", CurrentSettings.Contrast, -100f, 100f)
                .WithWholeNumbers()
                .WithTarget(target => CurrentSettings.Contrast = target);

            GenerateSlider(graphicsGroup)
                .Initialize("Saturación", CurrentSettings.Saturation, -100f, 100f)
                .WithWholeNumbers()
                .WithTarget(target => CurrentSettings.Saturation = target);

            GenerateToggle(graphicsGroup)
                .Initialize("Antialias (SMAA)", CurrentSettings.EnableSmaa)
                .WithTarget(target => CurrentSettings.EnableSmaa = target);

            GenerateToggle(graphicsGroup)
                .Initialize("Efectos de postprocesado", CurrentSettings.EnablePostProcessing)
                .WithTarget(target => CurrentSettings.EnablePostProcessing = target);

            GenerateSlider(audioGroup)
                .Initialize("Volumen", CurrentSettings.Volume * 100f, 0f, 100f)
                .WithValueFormat("{0}%")
                .WithTarget(target => CurrentSettings.Volume = Mathf.Clamp01(target / 100f));
        }

        private ToggleWidget GenerateToggle(Transform group)
        {
            return Instantiate(toggleWidgetPrefab, group).GetComponent<ToggleWidget>();
        }

        private SliderWidget GenerateSlider(Transform group)
        {
            return Instantiate(sliderWidgetPrefab, group).GetComponent<SliderWidget>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeybindManager.Singleton.CurrentKeybinds.Pause) && settingsScreen.activeSelf)
                settingsScreen.SetActive(false);
        }

        private void ChangeVisibleGroup(Transform targetGroup)
        {
            gameplayGroup.gameObject.SetActive(gameplayGroup == targetGroup);
            accessibilityGroup.gameObject.SetActive(accessibilityGroup == targetGroup);
            controlsGroup.gameObject.SetActive(controlsGroup == targetGroup);
            graphicsGroup.gameObject.SetActive(graphicsGroup == targetGroup);
            audioGroup.gameObject.SetActive(audioGroup == targetGroup);
        }
    }
}
