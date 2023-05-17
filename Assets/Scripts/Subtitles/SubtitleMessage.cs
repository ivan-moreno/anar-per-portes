using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Subtitle Message")]
    [RequireComponent(typeof(CanvasGroup))]
    public class SubtitleMessage : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        private CanvasGroup canvasGroup;
        private float aliveTime;
        private const float appearanceRate = 6f;

        public void Initialize(string message, float duration, Color color)
        {
            messageText.text = message;
            messageText.color = color;
            aliveTime = duration;
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            if (Game.Settings.LargeSubtitles)
                messageText.fontSize *= 1.5f;
        }

        private void Update()
        {
            aliveTime -= Time.deltaTime;

            // Apply a appear/disappear effect.
            canvasGroup.alpha = aliveTime > 0.5f
                ? Mathf.Clamp01(canvasGroup.alpha + (Time.unscaledDeltaTime * appearanceRate))
                : Mathf.Clamp01(canvasGroup.alpha - (Time.unscaledDeltaTime * appearanceRate));

            // Remove the subtitle message whenever its duration has transcurred.
            if (aliveTime <= 0f)
                Destroy(gameObject);
        }
    }
}
