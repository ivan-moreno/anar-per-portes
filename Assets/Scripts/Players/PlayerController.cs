using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Player Controller")]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Singleton { get; private set; }

        public Pedestal CurrentPedestal { get; set; }
        public bool IsCamouflaged { get; set; } = false;
        public bool CanBeCaught { get; set; } = true;
        public bool IsInCatchSequence { get; set; } = false;
        public bool IsCaught { get; set; } = false;
        public float WalkSpeed { get; private set; } = 6f;
        public Camera Camera { get; private set; }
        public Camera UiCamera { get; private set; }
        public int TixAmount { get; private set; }
        public PlayerModel PlayerModel { get; private set; }
        public UnityEvent OnBeginCatchSequence { get; } = new();
        public Vector3 Velocity => velocity;

        [SerializeField] private Animator modelAnimator;

        private Animator visionAnimator;
        private AudioSource audioSource;
        private Transform model;
        private CharacterController characterController;
        private InventoryItem equippedItem;
        private Vector3 velocity;
        private Vector3 motion;
        private float vLook;
        private int blockMoveCharges = 0;
        private int blockLookCharges = 0;
        private int blockInteractCharges = 0;
        private IInteractable lastFocusedInteractable;
        private readonly List<InventoryItem> items = new();
        private bool hasItemEquipped = false;
        private Transform visionTarget;
        private Vector3 visionTargetOffset;
        private float effectSpeed = 1f;
        private float effectTimer;
        private bool CanMove => blockMoveCharges <= 0;
        private bool CanLook => blockLookCharges <= 0f;
        private bool CanInteract => blockInteractCharges <= 0f;

        private const float vLookMaxAngle = 70f;
        private const float interactRange = 2.5f;

        public void BeginCatchSequence()
        {
            if (!CanBeCaught)
                return;

            IsInCatchSequence = true;
            CanBeCaught = false;
            OnBeginCatchSequence?.Invoke();
        }

        public void BlockAll()
        {
            BlockMove();
            BlockLook();
            BlockInteract();
        }

        public void BlockMove()
        {
            blockMoveCharges++;
        }

        public void BlockLook()
        {
            blockLookCharges++;
        }

        public void BlockInteract()
        {
            blockInteractCharges++;
        }

        public void UnblockAll()
        {
            UnblockMove();
            UnblockLook();
            UnblockInteract();
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

        public void UnblockInteract()
        {
            blockInteractCharges--;

            if (blockInteractCharges < 0)
                blockInteractCharges = 0;
        }

        public void Teleport(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }

        public InventoryItem GetItem(string itemId)
        {
            var itemTransform = transform.Find("Items").Find(itemId);

            if (itemTransform == null)
                return null;

            return itemTransform.GetComponent<InventoryItem>();
        }

        public void PackItem(string itemId)
        {
            var itemTransform = transform.Find("Items").Find(itemId);

            if (itemTransform == null)
                return;

            if (itemTransform.TryGetComponent(out InventoryItem item))
                PackItem(item);
        }

        private void PackAllItems()
        {
            foreach (Transform t in transform.Find("Items"))
                PackItem(t.name);
        }

        public void PackItem(InventoryItem item)
        {
            if (items.Count >= 9)
                return;

            if (items.Contains(item))
                return;

            items.Add(item);
            ItemManager.Singleton.OccupyAvailableSlotWith(item);
            modelAnimator.Play("Grab", 0, 0f);
            modelAnimator.SetLayerWeight(1, 1f);
        }

        public void ConsumeItem(string itemId)
        {
            var itemTransform = transform.Find("Items").Find(itemId);

            if (itemTransform == null)
                return;

            if (itemTransform.TryGetComponent(out InventoryItem item))
                ConsumeItem(item);
        }

        public void ConsumeItem(InventoryItem item)
        {
            if (!items.Contains(item))
                return;

            if (item == equippedItem)
            {
                ConsumeEquippedItem();
                return;
            }

            item.Consume();
            items.Remove(item);
        }

        public void ConsumeEquippedItem()
        {
            equippedItem.Consume();
            items.Remove(equippedItem);
            equippedItem = null;
            hasItemEquipped = false;
        }

        public bool HasItem(string itemId)
        {
            return items.Find(x => x.name.Equals(itemId)) != null;
        }

        public bool EquippedItemIs(string itemId)
        {
            if (equippedItem == null)
                return false;

            return equippedItem.name.Equals(itemId);
        }

        public void SetVisionTarget(Transform target, Vector3 offset = default)
        {
            visionTarget = target;
            visionTargetOffset = offset;
        }

        public void ClearVisionTarget()
        {
            visionTarget = null;
            visionTargetOffset = Vector3.zero;
        }

        public void PlaySound(SoundResource sound)
        {
            audioSource.PlayOneShot(sound);
        }

        public void PlaySound(AudioClip sound)
        {
            audioSource.PlayOneShot(sound);
        }

        public void CollectTix(int amount)
        {
            TixAmount += amount;
            TixManager.Singleton.SetNewTixAmount(TixAmount);
        }

        public void CollectTix(int amount, string reason)
        {
            TixAmount += amount;
            TixManager.Singleton.SetNewTixAmount(TixAmount);
            TixManager.Singleton.GenerateTixTransaction(amount, reason);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            model = modelAnimator.transform;
            PlayerModel = model.GetComponent<PlayerModel>();
            visionAnimator = transform.Find("Vision").GetComponent<Animator>();
            Camera = visionAnimator.GetComponentInChildren<Camera>();
            UiCamera = Camera.transform.GetChild(0).GetComponent<Camera>();
            audioSource = GetComponent<AudioSource>();
            Cursor.lockState = CursorLockMode.Locked;

            GameSettingsManager.Singleton.OnCurrentSettingsChanged.AddListener(OnSettingsChanged);
        }

        private void OnSettingsChanged()
        {
            Camera.fieldOfView = GameSettingsManager.Singleton.CurrentSettings.FieldOfView;
            var cameraData = Camera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.antialiasing = GameSettingsManager.Singleton.CurrentSettings.EnableSmaa
                ? AntialiasingMode.SubpixelMorphologicalAntiAliasing
                : AntialiasingMode.None;
        }

        private void Update()
        {
            UpdateEffects();
            UpdateInteraction();
            UpdateRotation();
            UpdateMotion();
            UpdateItems();

            if (IsCaught || IsInCatchSequence || IsCamouflaged)
            {
                velocity = Vector3.zero;
                return;
            }

            var preMovePosition = transform.position;
            characterController.Move(motion + (4f * Time.deltaTime * Physics.gravity));

            velocity = Time.timeScale <= 0f ? Vector3.zero : (transform.position - preMovePosition) / Time.deltaTime;

            if (transform.position.y < -256f)
                Teleport(RoomManager.Singleton.LatestRoom.transform.position);

            if (Input.GetKeyUp(KeyCode.F1))
                Teleport(RoomManager.Singleton.LatestRoom.NextRoomSpawnPoint.position);
            else if (Input.GetKeyUp(KeyCode.P))
                PackAllItems();
        }

        public void ApplySpeedEffect(float speed, float duration)
        {
            effectSpeed = speed;
            effectTimer = duration;
        }

        void UpdateEffects()
        {
            if (effectTimer <= 0f)
            {
                effectSpeed = 1f;
                return;
            }
            else if (effectTimer <= 1f)
                effectSpeed = Mathf.Lerp(effectSpeed, 1f, 1f - effectTimer);

            effectTimer -= Time.deltaTime;
        }

        private void UpdateInteraction()
        {
            if (!CanInteract || IsCaught || IsCamouflaged)
                return;

            var foundHit = Physics.Raycast(
                origin: transform.position + Camera.transform.localPosition,
                direction: Camera.transform.forward,
                hitInfo: out var hitInfo,
                maxDistance: interactRange,
                layerMask: LayerMask.GetMask("Default", "IgnoreEnemyRaycast"));

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
                if (lastFocusedInteractable != null)
                    lastFocusedInteractable.Unfocus();

                lastFocusedInteractable = interactable;
                lastFocusedInteractable.Focus();
            }

            if (Input.GetKeyDown(KeybindManager.Singleton.CurrentKeybinds.Interact))
                interactable.Interact();
        }

        private void UpdateRotation()
        {
            var hLookInput = Input.GetAxisRaw("Mouse X");
            hLookInput *= GameSettingsManager.Singleton.CurrentSettings.MouseSensitivity;
            hLookInput *= Time.unscaledDeltaTime;

            var vLookInput = -Input.GetAxisRaw("Mouse Y");
            vLookInput *= GameSettingsManager.Singleton.CurrentSettings.MouseSensitivity;
            vLookInput *= Time.unscaledDeltaTime;

            if (!CanLook)
                hLookInput = vLookInput = 0f;

            if (visionTarget != null)
            {
                var dir = -Vector3.Normalize(transform.position - (visionTarget.position + visionTargetOffset));
                var lookAtX = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookAtX, Time.deltaTime * 4f);
                var lookAtY = Quaternion.LookRotation(new Vector3(dir.x, dir.y, dir.z));
                Camera.transform.rotation = Quaternion.Slerp(Camera.transform.rotation, lookAtY, Time.deltaTime * 4f);
                //transform.transform.localEulerAngles = new Vector3(transform.transform.localEulerAngles.x, transform.transform.localEulerAngles.z, 0f);
                //Camera.transform.localEulerAngles = new Vector3(Camera.transform.localEulerAngles.x, Camera.transform.localEulerAngles.y, 0f);
                vLook = Camera.transform.localEulerAngles.x;
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
            motion.Normalize();
            motion = transform.rotation * motion;
            motion *= WalkSpeed;
            motion *= effectSpeed;
            motion *= Time.deltaTime;
        }

        private void UpdateItems()
        {
            if (IsCaught || PauseManager.Singleton.IsPaused || EnemyTipManager.Singleton.IsDisplaying)
                return;

            void UpdateEquipmentForKey(int number, KeyCode keyCode)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    if (ItemManager.Singleton.TryEquip(number, out equippedItem))
                    {
                        equippedItem.ToggleEquipped();
                        items.FindAll(x => x != equippedItem).ForEach(x => x.Unequip());
                        hasItemEquipped = equippedItem.IsEquipped;
                    }
                }
            }

            UpdateEquipmentForKey(1, KeyCode.Alpha1);
            UpdateEquipmentForKey(2, KeyCode.Alpha2);
            UpdateEquipmentForKey(3, KeyCode.Alpha3);
            UpdateEquipmentForKey(4, KeyCode.Alpha4);
            UpdateEquipmentForKey(5, KeyCode.Alpha5);
            UpdateEquipmentForKey(6, KeyCode.Alpha6);
            UpdateEquipmentForKey(7, KeyCode.Alpha7);
            UpdateEquipmentForKey(8, KeyCode.Alpha8);
            UpdateEquipmentForKey(9, KeyCode.Alpha9);

            var itemLayerWeight = modelAnimator.GetLayerWeight(1);
            var targetItemLayerWeight = hasItemEquipped ? 1f : 0f;
            var smoothItemLayerWeight = Mathf.Lerp(itemLayerWeight, targetItemLayerWeight, Time.deltaTime * 8f);

            if (!modelAnimator.GetCurrentAnimatorStateInfo(1).IsName("Grab"))
                modelAnimator.SetLayerWeight(1, smoothItemLayerWeight);
        }

        private void LateUpdate()
        {
            UpdateVisionAnimator();
        }

        private void UpdateVisionAnimator()
        {
            if (Time.timeScale <= 0.01f)
                return;

            // TODO: This might provoke unintended offsets when disabling during gameplay.
            visionAnimator.speed = GameSettingsManager.Singleton.CurrentSettings.EnableVisionMotion ? 1f : 0f;

            // Normalize horizontal velocity between values 0 and 1.
            var hVelocity = new Vector3(velocity.x, 0f, velocity.z).sqrMagnitude;

            if (WalkSpeed <= 0f)
                hVelocity = 0f;
            else
                hVelocity /= WalkSpeed * 6f;

            var animatorHVelocity = visionAnimator.GetFloat("HVelocity");

            // Smooth out the velocity changes.
            var smoothHVelocity = Mathf.Lerp(animatorHVelocity, hVelocity, Time.deltaTime * 8f);

            if (float.IsNaN(smoothHVelocity))
                smoothHVelocity = 0f;

            if (visionAnimator.enabled)
            {
                visionAnimator.speed = effectSpeed;
                visionAnimator.SetFloat("HVelocity", smoothHVelocity);
            }

            modelAnimator.SetFloat("HVelocity", smoothHVelocity);
            modelAnimator.speed = effectSpeed;

            var localVelocity = transform.rotation * new Vector3(velocity.x, 0f, velocity.z);
            var targetModelRotation = new Vector3(0f, 0f, 0f);

            if (localVelocity.x < -Mathf.Epsilon)
                targetModelRotation.y = -8f;
            else if (localVelocity.x > Mathf.Epsilon)
                targetModelRotation.y = 8f;

            if (localVelocity.z > Mathf.Epsilon || localVelocity.z < -Mathf.Epsilon)
                targetModelRotation.y *= 0.5f;

            model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(targetModelRotation), Time.deltaTime * 6f);
        }
    }
}
