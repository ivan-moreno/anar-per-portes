using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AnarPerPortes
{
    /// <summary>
    /// Logic class that reads Player input and acts accordingly.
    /// </summary>
    [AddComponentMenu("Anar per Portes/Player Controller")]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }
        public bool CanMove { get; set; } = true;
        public bool CanLook { get; set; } = true;
        public bool IsHidingAsStatue { get; set; } = false;
        public bool IsCaught { get; set; } = false;
        public Camera Camera { get; private set; }
        public Vector3 Velocity => velocity;
        private CharacterController characterController;
        [SerializeField] private Animator visionAnimator;
        [SerializeField] private Animator modelAnimator;
        private Vector3 velocity;
        private Vector3 motion;
        private float vLook;
        private float walkSpeed = 8f;
        private IInteractable lastFocusedInteractable;
        private readonly List<InventoryItem> items = new();
        private bool hasItemEquipped = false;
        private Vignette vignette;
        private const float vLookMaxAngle = 70f;
        private const float interactRange = 2.5f;
        private const float regularVignetteIntensity = 0.25f;
        private const float hidingVignetteIntensity = 0.5f;

        public void Teleport(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }

        public void PackItem(InventoryItem item)
        {
            items.Add(item);
            Game.ItemManager.GenerateSlotFor(item);
        }

        public void GetCaught(string title, string message)
        {
            IsCaught = true;
            CanMove = false;
            CanLook = false;
            Game.CaughtManager.PlayerCaught(title, message);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            Camera = visionAnimator.GetComponentInChildren<Camera>();
            Game.GlobalVolume.profile.TryGet(out vignette);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            UpdateInteraction();
            UpdateRotation();
            UpdateMotion();
            UpdateItems();

            if (IsCaught)
                return;

            var preMovePosition = transform.position;
            characterController.Move(motion + (4f * Time.deltaTime * Physics.gravity));
            velocity = (transform.position - preMovePosition) / Time.deltaTime;

            if (transform.position.y < -32f)
                Teleport(Game.RoomManager.LastLoadedRoom.transform.position);
        }

        private void UpdateInteraction()
        {
            if (IsCaught)
                return;

            var foundHit = Physics.Raycast(
                origin: transform.position + Camera.transform.localPosition,
                direction: Camera.transform.forward,
                hitInfo: out var hitInfo,
                maxDistance: interactRange,
                layerMask: LayerMask.GetMask("Default"));

            if (!foundHit)
            {
                if (lastFocusedInteractable != null)
                {
                    lastFocusedInteractable.Unfocus();
                    lastFocusedInteractable = null;
                }

                return;
            }

            var isInteractable = hitInfo.transform.TryGetComponent(out IInteractable interactable);

            if (!isInteractable)
            {
                if (lastFocusedInteractable != null)
                {
                    lastFocusedInteractable.Unfocus();
                    lastFocusedInteractable = null;
                }

                return;
            }

            if (lastFocusedInteractable != interactable)
            {
                lastFocusedInteractable = interactable;
                lastFocusedInteractable.Focus();
            }

            if (Input.GetKeyUp(Game.Settings.InteractKey))
                interactable.Interact();
        }

        private void UpdateRotation()
        {
            var hLookInput = Input.GetAxisRaw("Mouse X");
            hLookInput *= Game.Settings.HMouseSensitivity;
            hLookInput *= Time.unscaledDeltaTime;

            var vLookInput = -Input.GetAxisRaw("Mouse Y");
            vLookInput *= Game.Settings.VMouseSensitivity;
            vLookInput *= Time.unscaledDeltaTime;

            if (!CanLook)
                hLookInput = vLookInput = 0f;

            // Limit vertical look angle to avoid flipping the camera.
            vLook = Mathf.Clamp(vLook + vLookInput, -vLookMaxAngle, vLookMaxAngle);

            transform.Rotate(0f, hLookInput, 0f);
            Camera.transform.localEulerAngles = new(vLook, 0f, 0f);
        }

        private void UpdateMotion()
        {
            var moveXInput = Input.GetAxisRaw("Horizontal");
            var moveZInput = Input.GetAxisRaw("Vertical");

            if (!CanMove)
                moveXInput = moveZInput = 0f;

            motion = new(moveXInput, 0f, moveZInput);

            // Avoids faster speed when moving in diagonal.
            motion.Normalize();

            // Localizes motion to wherever the Player is looking.
            motion = transform.rotation * motion;

            // Applies the Walk Speed stat.
            motion *= walkSpeed;

            // Streamlines movement speed to be the same on any framerate.
            motion *= Time.deltaTime;
        }

        private void UpdateItems()
        {
            if (IsCaught)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1) && items.Count > 0)
            {
                items.FindAll(x => x != items[0]).ForEach(x => x.Unequip());
                items[0].ToggleEquipped();

                hasItemEquipped = items.Any(x => x.IsEquipped);
            }

            var itemLayerWeight = modelAnimator.GetLayerWeight(1);
            var targetItemLayerWeight = hasItemEquipped ? 1f : 0f;
            var smoothItemLayerWeight = Mathf.Lerp(itemLayerWeight, targetItemLayerWeight, Time.deltaTime * 8f);
            modelAnimator.SetLayerWeight(1, smoothItemLayerWeight);
        }

        private void LateUpdate()
        {
            UpdateVisionAnimator();
            UpdateVolume();
        }

        private void UpdateVisionAnimator()
        {
            // TODO: This might provoke unintended offsets when disabling during gameplay.
            visionAnimator.enabled = Game.Settings.EnableVisionMotion;

            // Normalize horizontal velocity between values 0 and 1.
            var hVelocity = new Vector3(velocity.x, 0f, velocity.z).sqrMagnitude;
            hVelocity /= walkSpeed * 8f;

            var animatorHVelocity = visionAnimator.GetFloat("HVelocity");

            // Smooth out the velocity changes.
            var smoothHVelocity = Mathf.Lerp(animatorHVelocity, hVelocity, Time.deltaTime * 8f);

            if (visionAnimator.enabled)
                visionAnimator.SetFloat("HVelocity", smoothHVelocity);

            modelAnimator.SetFloat("HVelocity", smoothHVelocity);
        }

        private void UpdateVolume()
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, IsHidingAsStatue ? hidingVignetteIntensity : regularVignetteIntensity, Time.deltaTime * 4f);
        }
    }
}
