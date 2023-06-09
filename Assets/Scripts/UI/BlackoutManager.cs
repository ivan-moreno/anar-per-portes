using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Blackout Manager")]
    public sealed class BlackoutManager : MonoBehaviour
    {
        public static BlackoutManager Singleton { get; private set; }
        [SerializeField] private Animator blackoutAnimator;

        public void PlayDoorOpen()
        {
            blackoutAnimator.Play("DoorOpen");
        }

        public void Hide()
        {
            blackoutAnimator.Play("Hidden");
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}
