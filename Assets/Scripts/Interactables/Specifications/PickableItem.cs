using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    public class PickableItem : MonoBehaviour, IInteractable
    {
        public UnityEvent OnPacked { get; } = new();

        [Header("Text")]
        [SerializeField] private string itemId;
        [SerializeField] private string tooltipItemName;
        [SerializeField] private string tooltipMessage;

        [Header("Components")]
        [SerializeField] private Transform tooltipPosition;
        [SerializeField] private Transform graphic;

        private Outline graphicOutline;
        private const string defaultTooltipMessage = "Recoger <b>{0}</b>";

        public void Focus()
        {
            var targetTooltipPoint = tooltipPosition == null ? transform : tooltipPosition;
            var targetTooltipMessage = string.IsNullOrWhiteSpace(tooltipMessage) ? defaultTooltipMessage : tooltipMessage;

            InteractionManager.Singleton.ShowTooltip(
                target: targetTooltipPoint,
                key: CurrentKeybinds().Interact,
                message: string.Format(targetTooltipMessage, tooltipItemName));

            if (graphicOutline != null)
                graphicOutline.enabled = true;
        }

        public void Unfocus()
        {
            var targetTooltipPoint = tooltipPosition == null ? transform : tooltipPosition;

            InteractionManager.Singleton.HideTooltipIfValidOwner(targetTooltipPoint);

            if (graphicOutline != null)
                graphicOutline.enabled = false;
        }

        public void Interact()
        {
            PlayerController.Singleton.PackItem(itemId);
            OnPacked?.Invoke();
            Unfocus();
            gameObject.SetActive(false);
        }

        private void Start()
        {
            if (graphic != null)
                graphic.TryGetComponent(out graphicOutline);
        }
    }
}
