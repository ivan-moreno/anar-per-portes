using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Flamboyant Graphic")]
    public class FlamboyantGraphic : MonoBehaviour
    {
        [SerializeField] private Material flamboyantMaterial;
        private Material defaultMaterial;

        private void Start()
        {
            if (Game.Settings.EnableFlamboyantSilhouettes)
                GetComponent<Renderer>().material = flamboyantMaterial;
        }
    }
}
