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

            Game.InteractionManager.ShowTooltip(tooltipPosition, Game.Settings.InteractKey.ToString(), "Camuflarse");
        }

        public void Unfocus()
        {
            Game.InteractionManager.HideTooltipIfValidOwner(tooltipPosition);
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

            if (Input.GetKeyUp(Game.Settings.InteractKey) && timeOccupied >= minOccupiedDuration)
                ReleasePlayer();
        }

        private void OccupyPlayer()
        {
            if (isOccupied)
                return;

            PlayerController.Instance.Teleport(occupyPlayerPosition.position);
            PlayerController.Instance.CanMove = false;
            PlayerController.Instance.IsHidingAsStatue = true;
            isOccupied = true;
            audioSource.PlayOneShot(hideSound);

            Game.InteractionManager.HideTooltipIfValidOwner(tooltipPosition);
        }

        private void ReleasePlayer()
        {
            if (!isOccupied)
                return;

            PlayerController.Instance.Teleport(releasePlayerPosition.position);
            PlayerController.Instance.CanMove = true;
            PlayerController.Instance.IsHidingAsStatue = false;
            isOccupied = false;
            audioSource.PlayOneShot(revealSound);
        }
    }
}
