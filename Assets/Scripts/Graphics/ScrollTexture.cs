using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Scroll Texture")]
    public class ScrollTexture : MonoBehaviour
    {
        [SerializeField] private int materialIndex = 0;
        [SerializeField] private float xSpeed = 0.1f;
        [SerializeField] private float ySpeed = 0.1f;

        private new Renderer renderer;
        private float xOffset;
        private float yOffset;

        private void Start()
        {
            renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            xOffset += xSpeed * Time.deltaTime;
            yOffset += ySpeed * Time.deltaTime;
            renderer.materials[materialIndex].SetTextureOffset("_BaseMap", new(xOffset, yOffset));
        }
    }
}
