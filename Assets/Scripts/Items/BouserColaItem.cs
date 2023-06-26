using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    public class BouserColaItem : InventoryItem
    {
        [Header("Stats")]
        [SerializeField] private float speedMultiplier = 1.25f;
        [SerializeField] private float effectDuration = 16f;

        [Header("Audio")]
        [SerializeField] private AudioClip clickAudio;

        private void Update()
        {
            if (!IsEquipped || GameIsPaused())
                return;

            if (Input.GetKeyDown(CurrentKeybinds().PrimaryAction))
                PlayerClick();
        }

        private void PlayerClick()
        {
            audioSource.PlayOneShot(clickAudio);
            PlayerController.Singleton.ApplySpeedEffect(speedMultiplier, effectDuration);
            PlayerController.Singleton.ConsumeEquippedItem();
        }
    }
}
