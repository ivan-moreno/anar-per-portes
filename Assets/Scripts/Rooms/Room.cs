using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes.Rooms
{
    [AddComponentMenu("Anar per Portes/Rooms/Room")]
    public class Room : MonoBehaviour
    {
        public RoomSetEgg RoomSet { get; set; }
        public Room NextRoom { get; set; }
        public Transform NextRoomSpawnPoint { get; private set; }
        public Transform WaypointGroup { get; private set; }
        public Transform SkellLocationsGroup { get; private set; }
        public Transform PedroBreakPoint { get; private set; }
        public UnityEvent OnDoorOpened { get; } = new();
        public UnityEvent OnUnloading { get; } = new();
        public bool HasPedestals => hasPedestals;
        public bool IsSmallSize => isSmallSize;
        public bool IsMediumSize => isMediumSize;
        public bool IsLargeSize => isLargeSize;

        [Header("Components")]
        [SerializeField] protected RoomDoor door;

        [Header("Flags")]
        [SerializeField] protected bool isSmallSize = false;
        [SerializeField] protected bool isMediumSize = false;
        [SerializeField] protected bool isLargeSize = false;
        [SerializeField] protected bool isSafe = false;
        [SerializeField] protected bool isNormalmodeExclusive = false;
        [SerializeField] protected bool isHardmodeExclusive = false;
        [SerializeField] protected bool hasPedestals = false;
        [SerializeField] protected bool hasHallucinations = false;

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
            NextRoomSpawnPoint = transform.Find("Logic").Find("NextRoomPoint");
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
