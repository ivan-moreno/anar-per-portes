using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Accessibility Font Manager")]
    public class AccessibilityFontManager : MonoBehaviour
    {
        public TMP_FontAsset DyslexicFriendlyFont => dyslexicFriendlyFont;
        [SerializeField] private TMP_FontAsset dyslexicFriendlyFont;
    }
}
