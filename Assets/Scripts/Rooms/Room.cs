using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Room")]
    public class Room : MonoBehaviour
    {
        public Room NextRoom { get; set; }
        public Transform NextRoomGenerationPoint { get; private set; }
        public Transform WaypointGroup { get; private set; }
        public Transform SkellLocationsGroup { get; private set; }
        public Transform PedroBreakPoint { get; private set; }
        public UnityEvent OnDoorOpened { get; } = new();
        public UnityEvent OnUnloading { get; } = new();
        public bool HasHidingSpots => hasHidingSpots;

        [SerializeField] protected bool hasHidingSpots = false;
        [SerializeField] protected RoomDoor door;

        public void OpenDoor()
        {
            door.Open();
        }

        public void CloseDoor()
        {
            door.Close();
        }

        public void Unload()
        {
            OnUnloading?.Invoke();
            Destroy(gameObject);
        }

        public virtual void Initialize()
        {
            NextRoomGenerationPoint = transform.Find("Logic").Find("NextRoomPoint");
            WaypointGroup = transform.Find("Logic").Find("Waypoints");
            SkellLocationsGroup = transform.Find("Logic").Find("SkellLocations");
            PedroBreakPoint = transform.Find("Logic").Find("PedroBreakPoint");
            door.OnDoorOpened.AddListener(DoorOpened);
        }

        private void DoorOpened()
        {
            OnDoorOpened?.Invoke();
            SkellHearManager.Singleton.AddNoise(8f);

            if (TryGetComponent(out AudioSource audioSource))
                audioSource.Stop();
        }
    }
}
