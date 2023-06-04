using System.Collections.Generic;
using System.Linq;
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
        public static PlayerController Singleton { get; private set; }
        public bool IsHidingAsStatue { get; set; } = false;
        public bool IsCaught { get; set; } = false;
        public Camera Camera { get; private set; }
        public Camera UiCamera { get; private set; }
        public Vector3 Velocity => velocity;
        private bool CanMove => blockMoveCharges <= 0;
        private bool CanLook => blockLookCharges <= 0f;
        [SerializeField] private Animator visionAnimator;
        [SerializeField] private Animator modelAnimator;
        private CharacterController characterController;
        private Vector3 velocity;
        private Vector3 motion;
        private float vLook;
        private float walkSpeed = 8f;
        private int blockMoveCharges = 0;
        private int blockLookCharges = 0;
        private IInteractable lastFocusedInteractable;
        private readonly List<InventoryItem> items = new();
        private bool hasItemEquipped = false;
        private Transform visionTarget;
        private Vector3 visionTargetOffset;
        private const float vLookMaxAngle = 70f;
        private const float interactRange = 2.5f;

        public void BlockMove()
        {
            blockMoveCharges++;
        }

        public void BlockLook()
        {
            blockLookCharges++;
        }

        public void UnblockMove()
        {
            blockMoveCharges--;

            if (blockMoveCharges < 0)
                blockMoveCharges = 0;
        }

        public void UnblockLook()
        {
            blockLookCharges--;

            if (blockLookCharges < 0)
                blockLookCharges = 0;
        }

        public void Teleport(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }

        public void PackItem(InventoryItem item)
        {
            items.Add(item);
            ItemManager.Singleton.GenerateSlotFor(item);
        }

        public void SetVisionTarget(Transform target, Vector3 offset)
        {
            visionTarget = target;
            visionTargetOffset = offset;
        }

        public void ClearVisionTarget()
        {
            visionTarget = null;
            visionTargetOffset = Vector3.zero;
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            Camera = visionAnimator.GetComponentInChildren<Camera>();
            UiCamera = Camera.transform.GetChild(0).GetComponent<Camera>();
            Cursor.lockState = CursorLockMode.Locked;

            OnSettingsChanged();
            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            Camera.fieldOfView = GameSettingsManager.Singleton.CurrentSettings.FieldOfView;
            var cameraData = Camera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = GameSettingsManager.Singleton.CurrentSettings.EnablePostProcessing;
            cameraData.antialiasing = GameSettingsManager.Singleton.CurrentSettings.EnableSmaa
                ? AntialiasingMode.SubpixelMorphologicalAntiAliasing
                : AntialiasingMode.None;
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
                Teleport(RoomManager.Singleton.LastLoadedRoom.transform.position);

#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.F1))
                Teleport(RoomManager.Singleton.LastLoadedRoom.NextRoomGenerationPoint.position);
#endif
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

            if (Input.GetKeyUp(KeybindManager.Singleton.CurrentKeybinds.Interact))
                interactable.Interact();
        }

        private void UpdateRotation()
        {
            var hLookInput = Input.GetAxisRaw("Mouse X");
            hLookInput *= GameSettingsManager.Singleton.CurrentSettings.HMouseSensitivity;
            hLookInput *= Time.unscaledDeltaTime;

            var vLookInput = -Input.GetAxisRaw("Mouse Y");
            vLookInput *= GameSettingsManager.Singleton.CurrentSettings.VMouseSensitivity;
            vLookInput *= Time.unscaledDeltaTime;

            if (!CanLook)
                hLookInput = vLookInput = 0f;

            if (visionTarget != null)
            {
                var dir = -Vector3.Normalize(transform.position - (visionTarget.position + visionTargetOffset));
                var lookAt = Quaternion.LookRotation(dir);
                Camera.transform.rotation = Quaternion.Slerp(Camera.transform.rotation, lookAt, Time.deltaTime * 4f);
                Camera.transform.localEulerAngles = new Vector3(Camera.transform.localEulerAngles.x, Camera.transform.localEulerAngles.y, 0f);
                return;
            }

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
        }

        private void UpdateVisionAnimator()
        {
            if (Time.timeScale <= 0f)
                return;

            // TODO: This might provoke unintended offsets when disabling during gameplay.
            visionAnimator.speed = GameSettingsManager.Singleton.CurrentSettings.EnableVisionMotion ? 1f : 0f;

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
    }
}