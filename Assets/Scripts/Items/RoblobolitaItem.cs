using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    public class RoblobolitaItem : InventoryItem
    {
        [SerializeField] private AudioClip clickAudio;

        private void Update()
        {
            if (!IsEquipped)
            {
                graphic.transform.localScale = Vector3.one;
                return;
            }

            graphic.transform.localScale = Vector3.Lerp(graphic.transform.localScale, Vector3.one, 4f * Time.deltaTime);

            if (Input.GetKeyDown(CurrentKeybinds().PrimaryAction))
                PlayerClick();
        }

        private void PlayerClick()
        {
            graphic.transform.localScale = new(1.25f, 0.5f, 1.25f);
            audioSource.pitch = Random.Range(0.9f, 1.2f);
            audioSource.PlayOneShot(clickAudio);
        }
    }
}
