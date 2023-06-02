using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    public class BouserTail : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform tooltipPosition;

        public void Focus()
        {
            InteractionManager.Singleton.ShowTooltip(tooltipPosition, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), "Agarrar cola");
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);
        }

        public void Interact()
        {
            GetComponentInParent<BouserEnemy>().GrabTail();
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);
            gameObject.SetActive(false);
        }
    }
}
