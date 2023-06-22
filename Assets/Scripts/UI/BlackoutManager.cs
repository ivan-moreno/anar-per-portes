using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Blackout Manager")]
    public sealed class BlackoutManager : MonoBehaviour
    {
        public static BlackoutManager Singleton { get; private set; }
        [SerializeField] private Animator blackoutAnimator;
        [SerializeField] private GameObject squareBars;

        public void EnableSquareBars()
        {
            squareBars.SetActive(true);
        }

        public void DisableSquareBars()
        {
            squareBars.SetActive(false);
        }

        public void PlayInstantly()
        {
            blackoutAnimator.Play("InstantBlackout", 0, 0f);
        }

        public void PlayDoorOpen()
        {
            blackoutAnimator.Play("DoorOpen", 0, 0f);
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
