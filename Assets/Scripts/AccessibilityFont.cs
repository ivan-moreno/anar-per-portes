using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Accessibility Font")]
    public class AccessibilityFont : MonoBehaviour
    {
        [SerializeField][Min(0)] private float dyslexicFontSize = 0;
        private TMP_FontAsset originalFont;
        private float originalFontSize;
        private TMP_Text label;

        private void Start()
        {
            label = GetComponent<TMP_Text>();
            originalFont = label.font;
            originalFontSize = label.fontSize;

            if (Game.Settings.EnableDyslexicFriendlyFont)
            {
                label.font = Game.AccessibilityFontManager.DyslexicFriendlyFont;

                if (dyslexicFontSize > 0)
                    label.fontSize = dyslexicFontSize;
            }
        }
    }
}
