using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Interaction Manager")]
    public class InteractionManager : MonoBehaviour
    {
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
    }
}
