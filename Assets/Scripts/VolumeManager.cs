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
        private Vignette vignette;
        private const float defaultVignetteIntensity = 0.25f;
        private const float hidingVignetteIntensity = 0.5f;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            volume = GetComponent<Volume>();
            volume.profile.TryGet(out vignette);
        }

        private void Update()
        {
            var targetVignetteIntensity = PlayerController.Singleton.IsHidingAsStatue ? hidingVignetteIntensity : defaultVignetteIntensity;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * 4f);
        }
    }
}
