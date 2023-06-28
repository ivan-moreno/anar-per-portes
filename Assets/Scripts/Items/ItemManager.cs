using System;
using System.Linq;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/ItemManager")]
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Singleton { get; private set; }
        [SerializeField] private Transform slotsGroup;
        [SerializeField] private GameObject slotPrefab;
        private CanvasGroup slotsCanvasGroup;
        private float timeSinceItemChange;
        private const float timeToUnfocus = 8f;
        private readonly InventoryItemSlot[] slots = new InventoryItemSlot[9];

        public void OccupyAvailableSlotWith(InventoryItem item)
        {
            foreach (var slot in slots)
            {
                if (slot.AssignedItem != null)
                    continue;

                item.OnEquipped.AddListener(ResetTimeSinceItemChange);
                item.OnUnequipped.AddListener(ResetTimeSinceItemChange);
                slot.Initialize(item);
                break;
            }
        }

        public void CauseAlertOnItem(InventoryItem item)
        {
            var slot = slots.Where(slot => slot.AssignedItem == item).FirstOrDefault();

            if (slot == null)
                return;

            slot.VisualAlert();
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            slotsCanvasGroup = slotsGroup.GetComponent<CanvasGroup>();

            for (int i = 0; i < slots.Length; i++)
            {
                var instance = Instantiate(slotPrefab, slotsGroup);
                var slot = instance.GetComponent<InventoryItemSlot>();
                slots[i] = slot;
                slot.AssignNumber(i + 1);
                slot.gameObject.SetActive(false);
            }
        }

        private void ResetTimeSinceItemChange()
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

        public bool TryEquip(int number, out InventoryItem equippedItem)
        {
            if (slots[number - 1].AssignedItem)
            {
                equippedItem = slots[number - 1].AssignedItem;
                return true;
            }
            else
            {
                equippedItem = null;
                return false;
            }
        }
    }
}
