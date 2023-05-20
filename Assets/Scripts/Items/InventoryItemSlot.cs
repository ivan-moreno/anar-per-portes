using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Inventory Item Slot")]
    public class InventoryItemSlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text slotNumberText;
        private Animator animator;

        public void Initialize(InventoryItem assignedItem)
        {
            animator = GetComponent<Animator>();
            assignedItem.OnEquipped.AddListener(Equipped);
            assignedItem.OnUnequipped.AddListener(Unequipped);
            iconImage.sprite = assignedItem.Icon;
        }

        private void Equipped()
        {
            animator.Play("Equip", 0, 0f);
        }

        private void Unequipped()
        {
            animator.Play("Unequip", 0, 0f);
        }
    }
}
