using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    public class HardmodeSwitch : Interactable
    {
        [SerializeField] private SoundResource thunderSound;

        protected override void Start()
        {
            RoomManager.Singleton.OnRoomGenerated.AddListener(x => Destroy(gameObject));
            base.Start();
        }

        public override void Interact()
        {
            HardmodeManager.Singleton.EnableHardmode();
            PushSubtitle("Modo difícil activado. Buena suerte.", 6f, Team.Hostile);
            PlayerSound(thunderSound);
            base.Interact();
            Destroy(gameObject);
        }
    }
}
