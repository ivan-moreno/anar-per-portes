using AnarPerPortes.Enemies;
using UnityEngine;

namespace AnarPerPortes
{
    public class BouserTail : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform tooltipPosition;
        private BouserEnemy bouser;

        public void Focus()
        {
            if (bouser.IsFriendly || bouser.IsDefeated)
                return;

            InteractionManager.Singleton.ShowTooltip(tooltipPosition, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), "Agarrar cola");
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);
        }

        public void Interact()
        {
            if (bouser.IsFriendly || bouser.IsDefeated)
                return;

            bouser.GrabTail();
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);
            gameObject.SetActive(false);
        }

        private void Start()
        {
            bouser = GetComponentInParent<BouserEnemy>();
        }
    }
}
