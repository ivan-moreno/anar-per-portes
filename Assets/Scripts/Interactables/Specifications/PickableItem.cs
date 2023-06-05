using UnityEngine;

namespace AnarPerPortes
{
    public class PickableItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private string itemId;
        [SerializeField] private string tooltipItemName;
        [SerializeField] private Transform tooltipPosition;

        public void Focus()
        {
            InteractionManager.Singleton.ShowTooltip(tooltipPosition, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), "Recoger <b>" + tooltipItemName + "</b>");
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);
        }

        public void Interact()
        {
            PlayerController.Singleton.PackItem(itemId);
            Destroy(gameObject);
        }
    }
}
