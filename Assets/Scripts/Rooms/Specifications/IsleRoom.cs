using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Isle Room")]
    public class IsleRoom : Room
    {
        public RoomDoor IncorrectDoor => incorrectDoor;
        [HideInInspector] public UnityEvent OnIncorrectDoorOpened;
        [SerializeField] protected RoomDoor incorrectDoor;

        protected override void Start()
        {
            door.OnDoorOpened.AddListener(() => incorrectDoor.Deactivate());
            incorrectDoor.OnDoorOpened.AddListener(() => OnIncorrectDoorOpened?.Invoke());

            base.Start();
        }
    }
}
