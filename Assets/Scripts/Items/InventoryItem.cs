using AnarPerPortes.Enemies;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Inventory Item")]
    public class InventoryItem : MonoBehaviour
    {
        public UnityEvent OnEquipped { get; } = new();
        public UnityEvent OnUnequipped { get; } = new();
        public UnityEvent OnConsumed { get; } = new();
        public bool IsEquipped { get; private set; } = false;
        public Sprite Icon => icon;

        [SerializeField] protected AudioClip equipAudio;
        [SerializeField] protected AudioClip unequipAudio;
        [SerializeField] protected GameObject graphic;
        [SerializeField] private Sprite icon;

        protected AudioSource audioSource;

        public void ToggleEquipped()
        {
            if (IsEquipped)
                Unequip();
            else
                Equip();
        }

        public void Equip()
        {
            if (IsEquipped)
                return;

            IsEquipped = true;
            graphic.SetActive(true);
            OnEquipped?.Invoke();

            if (equipAudio != null)
                audioSource.PlayOneShot(equipAudio);
        }

        public void Unequip()
        {
            if (!IsEquipped)
                return;

            IsEquipped = false;
            OnUnequipped?.Invoke();
            graphic.SetActive(false);

            if (unequipAudio != null)
                audioSource.PlayOneShot(unequipAudio);
        }

        public void Consume()
        {
            Unequip();
            OnConsumed?.Invoke();
        }

        protected virtual void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}
