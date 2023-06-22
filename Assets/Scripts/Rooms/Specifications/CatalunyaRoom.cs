using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Rooms
{
    [AddComponentMenu("Anar per Portes/Rooms/Catalunya Room")]
    public class CatalunyaRoom : Room
    {
        [SerializeField] private GameObject[] supportingCast;
        [SerializeField] private AudioClip catalanBirdMusic;
        [SerializeField] private AudioClip hardmodeCochesChoconesMusic;

        private void Start()
        {
            if (IsHardmodeEnabled())
            {
                foreach (var enemy in supportingCast)
                    enemy.SetActive(true);

                AudioManager.Singleton.PlayMusic(hardmodeCochesChoconesMusic);
            }
            else
                AudioManager.Singleton.PlayMusic(catalanBirdMusic);
        }

        protected override void DoorOpened()
        {
            AudioManager.Singleton.StopMusic();
            base.DoorOpened();
        }
    }
}
