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
        public UnityEvent OnApplySettings { get; private set; } = new();
        public UnityEvent OnCurrentSettingsChanged { get; private set; } = new(); //TODO: normalize UnityEvents to be either a field or a property

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

            GenerateSlider(audioGroup)
                .Initialize("Volumen", CurrentSettings.Volume, 0f, 100f)
                .WithTarget(target => CurrentSettings.Volume = Mathf.Clamp01(target / 100f));
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
