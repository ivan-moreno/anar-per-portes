using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Accessibility Font Manager")]
    public sealed class AccessibilityFontManager : MonoBehaviour
    {
        public static AccessibilityFontManager Singleton { get; private set; }
        public TMP_FontAsset DyslexicFriendlyFont => dyslexicFriendlyFont;
        [SerializeField] private TMP_FontAsset dyslexicFriendlyFont;

        private void Awake()
        {
            Singleton = this;
        }
    }
}
