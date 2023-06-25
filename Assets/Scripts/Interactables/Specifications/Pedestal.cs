using AnarPerPortes.Enemies;
using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

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
        [SerializeField] private Transform graphic;

        private Outline graphicOutline;
        private AudioSource audioSource;
        private new Collider collider;
        private bool isOccupied = false;
        private float timeOccupied = 0f;
        private const float minOccupiedDuration = 0.5f;
        private bool isTransitioning = false;

        public void Focus()
        {
            if (isOccupied)
                return;

            InteractionManager.Singleton.ShowTooltip(tooltipPosition, KeybindManager.Singleton.CurrentKeybinds.Interact.ToString(), "Camuflarse");

            if (graphicOutline != null)
                graphicOutline.enabled = true;
        }

        public void Unfocus()
        {
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);

            if (graphicOutline != null)
                graphicOutline.enabled = false;
        }

        public void Interact()
        {
            OccupyPlayer();
        }

        public void OccupyPlayer()
        {
            StartCoroutine(nameof(OccupyPlayerCoroutine));
        }

        public void ReleasePlayer()
        {
            StartCoroutine(nameof(ReleasePlayerCoroutine));
        }

        IEnumerator OccupyPlayerCoroutine()
        {
            if (isOccupied || isTransitioning)
                yield break;

            isTransitioning = true;
            collider.enabled = false;
            PlayerController.Singleton.IsCamouflaged = true;
            PlayerController.Singleton.CurrentPedestal = this;
            PlayerController.Singleton.BlockMove();
            isOccupied = true;
            audioSource.PlayOneShot(hideSound);
            InteractionManager.Singleton.HideTooltipIfValidOwner(tooltipPosition);

            if (graphicOutline != null)
                graphicOutline.enabled = false;

            float timer = 0f;
            Vector3 originalPlayerPos = PlayerPosition();

            while (timer < 1f)
            {
                timer += Time.deltaTime * 4f;
                var targetPos = Vector3.Lerp(originalPlayerPos, occupyPlayerPosition.position, timer);
                PlayerController.Singleton.Teleport(targetPos);
                yield return null;
            }

            isTransitioning = false;
        }

        IEnumerator ReleasePlayerCoroutine()
        {
            if (!isOccupied || isTransitioning)
                yield break;

            isTransitioning = true;
            audioSource.PlayOneShot(revealSound);

            float timer = 0f;
            Vector3 targetPlayerPos =
                occupyPlayerPosition.position
                - new Vector3(0f, occupyPlayerPosition.localPosition.y, 0f)
                + PlayerController.Singleton.transform.forward * 2f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * 4f;
                var targetPos = Vector3.Lerp(occupyPlayerPosition.position, targetPlayerPos, timer);
                PlayerController.Singleton.Teleport(targetPos);
                yield return null;
            }

            PlayerController.Singleton.IsCamouflaged = false;
            PlayerController.Singleton.CurrentPedestal = null;
            PlayerController.Singleton.UnblockMove();
            isOccupied = false;
            collider.enabled = true;
            isTransitioning = false;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            collider = GetComponent<Collider>();

            if (graphic != null)
                graphic.TryGetComponent(out graphicOutline);
        }

        private void Update()
        {
            if (isOccupied)
            {
                timeOccupied += Time.deltaTime;

                if (!isTransitioning)
                    PlayerController.Singleton.Teleport(occupyPlayerPosition.position);
            }
            else
            {
                timeOccupied = 0f;
                return;
            }

            if (Input.GetKeyUp(KeybindManager.Singleton.CurrentKeybinds.Interact) && timeOccupied >= minOccupiedDuration)
                ReleasePlayer();
        }

        private void OnDisable()
        {
            if (graphicOutline != null)
                graphicOutline.enabled = false;
        }
    }
}
