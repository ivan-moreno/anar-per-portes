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

        public virtual void Focus()
        {
            InteractionManager.Singleton.ShowTooltip(tooltipLocation, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), tooltipMessage);
        }

        public virtual void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipLocation);
        }

        public virtual void Interact()
        {
            OnInteracted?.Invoke();
        }
    }
}
