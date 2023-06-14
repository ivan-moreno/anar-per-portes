using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Light Mode Manager")]
    public sealed class LightModeManager : MonoBehaviour
    {
        public static LightModeManager Singleton { get; private set; }
        private readonly Color defaultAmbientLight = new(0.06f, 0.06f, 0.07f);
        private readonly Color lightModeAmbientLight = new(0.5f, 0.5f, 0.5f);

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            RenderSettings.ambientLight = GameSettingsManager.Singleton.CurrentSettings.EnableRoomBrighten
                ? lightModeAmbientLight
                : defaultAmbientLight;
        }
    }
}
