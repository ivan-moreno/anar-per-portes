using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Flamboyant Graphic")]
    public class FlamboyantGraphic : MonoBehaviour
    {
        [SerializeField] private Material flamboyantMaterial;
        private Material defaultMaterial;
        private new Renderer renderer;

        private void Start()
        {
            renderer = GetComponent<Renderer>();
            defaultMaterial = renderer.material;
            OnSettingsChanged();
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            renderer.material = GameSettingsManager.Singleton.CurrentSettings.EnableFlamboyantGraphics
                ? flamboyantMaterial
                : defaultMaterial;
        }
    }
}
