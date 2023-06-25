using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Interactables/Interactable")]
    public class Interactable : MonoBehaviour, IInteractable
    {
        public UnityEvent OnInteracted { get; } = new();
        [SerializeField] protected Transform tooltipLocation;
        [SerializeField] private string tooltipMessage;
        [SerializeField] private Transform graphic;

        private Outline graphicOutline;

        public virtual void Focus()
        {
            InteractionManager.Singleton.ShowTooltip(tooltipLocation, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), tooltipMessage);

            if (graphicOutline != null)
                graphicOutline.enabled = true;
        }

        public virtual void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipLocation);

            if (graphicOutline != null)
                graphicOutline.enabled = false;
        }

        public virtual void Interact()
        {
            OnInteracted?.Invoke();
        }

        protected virtual void Start()
        {
            if (graphic != null)
                graphic.TryGetComponent(out graphicOutline);
        }

        private void OnDisable()
        {
            if (graphicOutline != null)
                graphicOutline.enabled = false;
        }
    }
}
