using UnityEngine;

namespace AnarPerPortes
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private Transform slotsGroup;
        [SerializeField] private GameObject slotPrefab;
        private CanvasGroup slotsCanvasGroup;
        private float timeSinceItemChange;
        private const float timeToUnfocus = 8f;

        public void GenerateSlotFor(InventoryItem item)
        {
            var instance = Instantiate(slotPrefab, slotsGroup);

            var hasValidSlot = instance.TryGetComponent(out InventoryItemSlot slot);

            if (!hasValidSlot)
            {
                Debug.LogError("Slot Prefab has no InventoryItemSlot script attached to it.");
                return;
            }

            item.OnEquipped.AddListener(ResetTimeSinceItemChange);
            item.OnUnequipped.AddListener(ResetTimeSinceItemChange);

            slot.Initialize(item);
        }

        private void Start()
        {
            slotsCanvasGroup = slotsGroup.GetComponent<CanvasGroup>();
        }

        void ResetTimeSinceItemChange()
        {
            timeSinceItemChange = 0f;
        }

        private void Update()
        {
            timeSinceItemChange += Time.deltaTime;

            var shouldFocus = timeSinceItemChange < timeToUnfocus;
            slotsCanvasGroup.alpha = Mathf.MoveTowards(
                current: slotsCanvasGroup.alpha,
                target: shouldFocus ? 1f : 0.3f,
                maxDelta: shouldFocus ? Time.unscaledDeltaTime * 4f : Time.unscaledDeltaTime * 0.5f);
        }
    }
}
