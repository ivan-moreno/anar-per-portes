using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Tix Manager")]
    [RequireComponent(typeof(AudioSource))]
    public class TixManager : MonoBehaviour
    {
        public static TixManager Singleton { get; private set; }

        [SerializeField] private RectTransform tixPanel;
        [SerializeField] private TMP_Text tixAmountLabel;
        [SerializeField] private AudioClip collectTixSound;

        private AudioSource audioSource;
        private float targetTixAmount;
        private float smoothTixAmount;
        private float timeSinceTixChanged;
        private const float hideTixPanelDuration = 5f;
        private const float increaseSoundPitchDuration = 2.5f;

        public void SetNewTixAmount(int amount)
        {
            targetTixAmount = amount;
            
            tixAmountLabel.rectTransform.localScale = Vector3.one * 1.2f;

            if (timeSinceTixChanged <= increaseSoundPitchDuration)
            {
                audioSource.pitch += 0.1f;
                audioSource.pitch = Mathf.Clamp(audioSource.pitch, 1f, 1.2f);
            }
            else
                audioSource.pitch = 1f;

            timeSinceTixChanged = 0f;
            audioSource.PlayOneShot(collectTixSound);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            timeSinceTixChanged = hideTixPanelDuration;
        }

        private void Update()
        {
            timeSinceTixChanged += Time.deltaTime;

            var targetPanelPos = timeSinceTixChanged >= hideTixPanelDuration ? new Vector2(64f, 64f) : new Vector2(64f, -64f);
            tixPanel.anchoredPosition = Vector2.Lerp(tixPanel.anchoredPosition, targetPanelPos, 4f * Time.unscaledDeltaTime);

            smoothTixAmount = Mathf.Lerp(smoothTixAmount, targetTixAmount, 4f * Time.unscaledDeltaTime);
            tixAmountLabel.text = smoothTixAmount.ToString("0");
            tixAmountLabel.rectTransform.localScale = Vector3.Lerp(tixAmountLabel.rectTransform.localScale, Vector3.one, 4f * Time.unscaledDeltaTime);
        }
    }
}
