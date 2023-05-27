using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Interaction Manager")]
    public sealed class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Singleton { get; private set; }
        [SerializeField] private InteractionTooltip interactionTooltip;

        public void ShowTooltip(Transform target, string key, string message)
        {
            interactionTooltip.Show(target, key, message);
        }

        public void HideTooltip()
        {
            interactionTooltip.Hide();
        }

        public void HideTooltipIfValidOwner(Transform targetOwner)
        {
            interactionTooltip.HideIfValidOwner(targetOwner);
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}
