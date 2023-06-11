using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    public class PickableItem : MonoBehaviour, IInteractable
    {
        public UnityEvent OnPacked { get; } = new();
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
            OnPacked?.Invoke();
            Destroy(gameObject);
        }
    }
}
