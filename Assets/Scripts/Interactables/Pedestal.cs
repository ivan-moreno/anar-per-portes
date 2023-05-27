using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Pedestal")]
    public class Pedestal : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform occupyPlayerPosition;
        [SerializeField] private Transform releasePlayerPosition;
        [SerializeField] private Transform tooltipPosition;
        [SerializeField] private AudioClip hideSound;
        [SerializeField] private AudioClip revealSound;
        private AudioSource audioSource;
        private bool isOccupied = false;
        private float timeOccupied = 0f;
        private const float minOccupiedDuration = 0.5f;

        public void Focus()
        {
            if (isOccupied)
                return;

            InteractionManager.Singleton.ShowTooltip(tooltipPosition, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), "Camuflarse");
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);
        }

        public void Interact()
        {
            OccupyPlayer();
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (isOccupied)
                timeOccupied += Time.deltaTime;
            else
            {
                timeOccupied = 0f;
                return;
            }

            if (Input.GetKeyUp(KeybindManager.Singleton.CurrentKeybinds.Interact) && timeOccupied >= minOccupiedDuration)
                ReleasePlayer();
        }

        private void OccupyPlayer()
        {
            if (isOccupied)
                return;

            PlayerController.Singleton.Teleport(occupyPlayerPosition.position);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.IsHidingAsStatue = true;
            isOccupied = true;
            audioSource.PlayOneShot(hideSound);

            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);
        }

        private void ReleasePlayer()
        {
            if (!isOccupied)
                return;

            PlayerController.Singleton.Teleport(releasePlayerPosition.position);
            PlayerController.Singleton.UnblockMove();
            PlayerController.Singleton.IsHidingAsStatue = false;
            isOccupied = false;
            audioSource.PlayOneShot(revealSound);
        }
    }
}
