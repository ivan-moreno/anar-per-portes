using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Inventory Item")]
    public class InventoryItem : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnEquipped;
        [HideInInspector] public UnityEvent OnUnequipped;
        public Sprite Icon => icon;
        public bool IsEquipped { get; private set; } = false;
        [SerializeField] private AudioClip equipAudio;
        [SerializeField] private AudioClip unequipAudio;
        [SerializeField] private GameObject graphic;
        [SerializeField] private Sprite icon;
        private AudioSource audioSource;

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

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}
