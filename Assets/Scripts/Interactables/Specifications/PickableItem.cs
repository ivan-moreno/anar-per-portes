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
        [SerializeField] private Transform graphic;

        private Outline graphicOutline;

        public void Focus()
        {
            InteractionManager.Singleton.ShowTooltip(tooltipPosition, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), "Recoger <b>" + tooltipItemName + "</b>");

            if (graphicOutline != null)
                graphicOutline.enabled = true;
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);

            if (graphicOutline != null)
                graphicOutline.enabled = false;
        }

        public void Interact()
        {
            PlayerController.Singleton.PackItem(itemId);
            OnPacked?.Invoke();
            Destroy(gameObject);
        }

        private void Start()
        {
            if (graphic != null)
                graphic.TryGetComponent(out graphicOutline);
        }
    }
}
