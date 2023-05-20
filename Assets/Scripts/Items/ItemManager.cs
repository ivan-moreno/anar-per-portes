using UnityEngine;

namespace AnarPerPortes
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform slotsGroup;

        public void GenerateSlotFor(InventoryItem item)
        {
            var instance = Instantiate(slotPrefab, slotsGroup);

            var hasValidSlot = instance.TryGetComponent(out InventoryItemSlot slot);

            if (!hasValidSlot)
            {
                Debug.LogError("Slot Prefab has no InventoryItemSlot script attached to it.");
                return;
            }

            slot.Initialize(item);
        }
    }
}
