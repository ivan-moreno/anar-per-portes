using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrialMember : MonoBehaviour, IInteractable
    {
        public UnityEvent OnInteracted { get; } = new();
        [SerializeField] private string tooltipMessage;
        [SerializeField] private string knownTooltipMessage;

        public void Focus()
        {
            InteractionManager.Singleton.ShowTooltip(transform.GetChild(0), KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), tooltipMessage);
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(transform.GetChild(0));
        }

        public void Interact()
        {
            OnInteracted?.Invoke();
        }
    }
}
