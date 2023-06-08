using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Accessibility Font")]
    public class AccessibilityFont : MonoBehaviour
    {
        [SerializeField][Min(0)] private float dyslexicFontSize = 0;
        private TMP_Text label;
        private TMP_FontAsset defaultFont;
        private float defaultFontSize;

        private void Start()
        {
            label = GetComponent<TMP_Text>();
            defaultFont = label.font;
            defaultFontSize = label.fontSize;
            OnSettingsChanged();
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            if (GameSettingsManager.Singleton.CurrentSettings.EnableOpenDyslexicFont)
            {
                label.font = AccessibilityFontManager.Singleton.DyslexicFriendlyFont;

                if (dyslexicFontSize > 0)
                    label.fontSize = dyslexicFontSize;
            }
            else
            {
                label.font = defaultFont;
                label.fontSize = defaultFontSize;
            }
        }
    }
}
