using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Room")]
    public class Room : MonoBehaviour
    {
        public Transform WaypointGroup { get; private set; }
        public Transform NextRoomGenerationPoint { get; private set; }
        public Transform PedroBreakPoint { get; private set; }
        public bool HasHidingSpots => hasHidingSpots;
        [HideInInspector] public UnityEvent OnDoorOpened;
        [HideInInspector] public UnityEvent OnUnloading;
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

        protected virtual void Start()
        {
            NextRoomGenerationPoint = transform.Find("Logic").Find("NextRoomPoint");
            WaypointGroup = transform.Find("Logic").Find("Waypoints");
            PedroBreakPoint = transform.Find("Logic").Find("PedroBreakPoint");
            door.OnDoorOpened.AddListener(() => OnDoorOpened?.Invoke());
        }
    }
}
