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
        private RectTransform rectTransform;

        public void Initialize(string message, float duration, Color color)
        {
            messageText.text = message;
            messageText.color = color;
            aliveTime = duration;
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            if (GameSettingsManager.Singleton.CurrentSettings.EnableLargeSubtitles)
                messageText.fontSize *= 1.5f;
        }

        private void Update()
        {
            if (PauseManager.Singleton.IsPaused)
                return;

            aliveTime -= Time.unscaledDeltaTime;

            // Smoothly display or hide text.
            canvasGroup.alpha = aliveTime > 0.5f
                ? Mathf.Clamp01(canvasGroup.alpha + (Time.unscaledDeltaTime * appearanceRate))
                : Mathf.Clamp01(canvasGroup.alpha - (Time.unscaledDeltaTime * appearanceRate));

            // Remove the subtitle message whenever its duration has transcurred.
            if (aliveTime <= 0f)
                Destroy(gameObject);
        }

        private void LateUpdate()
        {
            rectTransform.sizeDelta = messageText.rectTransform.sizeDelta + new Vector2(32f, 16f);
        }
    }
}
