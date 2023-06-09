using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Blur Overlay Manager")]
    public class BlurOverlayManager : MonoBehaviour
    {
        public static BlurOverlayManager Singleton { get; private set; }

        [SerializeField] private Image blurImage;
        private Color targetBlurColor;
        private float targetTransitionDuration;

        public void SetBlur(Color blurColor)
        {
            blurImage.color = blurColor;
        }

        public void SetBlurSmooth(Color blurColor, float transitionDuration)
        {
            StopCoroutine(nameof(SetBlurSmoothCoroutine));
            targetBlurColor = blurColor;
            targetTransitionDuration = transitionDuration;
            StartCoroutine(nameof(SetBlurSmoothCoroutine));
        }

        private IEnumerator SetBlurSmoothCoroutine()
        {
            var time = 0f;

            while (time < 1f)
            {
                time += Time.deltaTime / Mathf.Max(0.01f, targetTransitionDuration);
                blurImage.color = Color.Lerp(blurImage.color, targetBlurColor, time);
                yield return null;
            }

            blurImage.color = targetBlurColor;
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}
