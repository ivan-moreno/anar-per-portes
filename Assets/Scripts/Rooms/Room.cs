using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Room")]
    public class Room : MonoBehaviour
    {
        public bool HasHidingSpots => hasHidingSpots;
        public Transform NextRoomGenerationPoint => nextRoomGenerationPoint;
        public Transform WaypointGroup { get; private set; }
        [HideInInspector] public UnityEvent OnDoorOpened;
        [HideInInspector] public UnityEvent OnUnloading;
        [SerializeField] protected bool hasHidingSpots = false;
        [SerializeField] protected Transform nextRoomGenerationPoint;
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
            WaypointGroup = transform.Find("Logic").Find("Waypoints");
            door.OnDoorOpened.AddListener(() => OnDoorOpened?.Invoke());
        }
    }
}
