using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Volume Manager")]
    [RequireComponent(typeof(Volume))]
    public sealed class VolumeManager : MonoBehaviour
    {
        public static VolumeManager Singleton { get; private set; }
        private Volume volume;
        private Bloom bloom;
        private Vignette vignette;
        private ColorAdjustments colorAdjustments;
        private const float defaultVignetteIntensity = 0.25f;
        private const float hidingVignetteIntensity = 0.5f;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            volume = GetComponent<Volume>();
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out colorAdjustments);
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            bloom.active = GameSettingsManager.Singleton.CurrentSettings.EnablePostProcessing;
            vignette.active = GameSettingsManager.Singleton.CurrentSettings.EnablePostProcessing;
            colorAdjustments.contrast.value = GameSettingsManager.Singleton.CurrentSettings.Contrast;
            colorAdjustments.saturation.value = GameSettingsManager.Singleton.CurrentSettings.Saturation;
        }

        private void Update()
        {
            var targetVignetteIntensity = PlayerController.Singleton.IsCamouflaged ? hidingVignetteIntensity : defaultVignetteIntensity;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * 4f);
        }
    }
}
