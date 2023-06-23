using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Inventory Item Slot")]
    public class InventoryItemSlot : MonoBehaviour
    {
        public InventoryItem AssignedItem { get; private set; }
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text slotNumberText;
        private Animator animator;

        public void Initialize(InventoryItem assignedItem)
        {
            animator = GetComponent<Animator>();
            AssignedItem = assignedItem;
            AssignedItem.OnEquipped.AddListener(Equipped);
            AssignedItem.OnUnequipped.AddListener(Unequipped);
            AssignedItem.OnConsumed.AddListener(Consumed);
            iconImage.sprite = AssignedItem.Icon;
            gameObject.SetActive(true);
            animator.Play("Draw", 0, 0f);
        }

        public void AssignNumber(int number)
        {
            slotNumberText.text = number.ToString();
        }

        private void Equipped()
        {
            animator.Play("Equip", 0, 0f);
        }

        private void Unequipped()
        {
            animator.Play("Unequip", 0, 0f);
        }

        private void Consumed()
        {
            StartCoroutine(nameof(ConsumedCoroutine));
        }

        IEnumerator ConsumedCoroutine()
        {
            AssignedItem.OnEquipped.RemoveListener(Equipped);
            AssignedItem.OnUnequipped.RemoveListener(Unequipped);
            AssignedItem.OnConsumed.RemoveListener(Consumed);
            AssignedItem = null;
            animator.Play("Undraw", 0, 0f);
            yield return new WaitForSeconds(0.5f);

            if (AssignedItem != null)
                yield break;

            gameObject.SetActive(false);
        }
    }
}
