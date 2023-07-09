using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrialMember : MonoBehaviour, IInteractable
    {
        public UnityEvent OnInteracted { get; } = new();
        [SerializeField] private string tooltipMessage;
        [SerializeField] private string knownTooltipMessage;
        [SerializeField] private SoundResource interactSound;

        private new Renderer renderer;

        public void Focus()
        {
            InteractionManager.Singleton.ShowTooltip(transform.GetChild(0), KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), tooltipMessage);
            renderer.material.SetColor("_EmissionColor", new Color(0.75f, 0.75f, 0.75f));
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(transform.GetChild(0));
            renderer.material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
        }

        public void Interact()
        {
            PlayerSound(interactSound);
            OnInteracted?.Invoke();
        }

        private void Start()
        {
            renderer = transform.GetChild(0).GetComponent<Renderer>();
        }
    }
}
