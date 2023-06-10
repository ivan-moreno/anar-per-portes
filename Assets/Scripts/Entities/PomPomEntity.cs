using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Entities/Pom Pom Entity")]
    public class PomPomEntity : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField][Min(0f)] private float walkSpeed = 1f;
        [SerializeField][Min(0f)] private float waitAtDoorDistance = 5f;

        private Animator animator;
        private Transform model;
        private Room currentRoom;
        private bool isWaiting = false;
        private bool reachedTarget = false;
        private int targetWaypointIndex = 0;

        private void Start()
        {
            transform.SetParent(null);
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;

            currentRoom = RoomManager.Singleton.Rooms[0];
            transform.position = currentRoom.transform.position;
            animator.SetBool("IsWalking", true);
            RoomManager.Singleton.OnRoomGenerated.AddListener(OnRoomGenerated);
            RoomManager.Singleton.OnRoomUnloading.AddListener(OnRoomUnloading);
        }

        private void OnRoomGenerated(Room room)
        {
            if (isWaiting)
            {
                isWaiting = false;
                currentRoom = room;
                targetWaypointIndex = 0;
                animator.SetBool("IsWalking", true);
            }
        }

        private void OnRoomUnloading(Room room)
        {
            if (room == currentRoom)
                TargetNextRoom();
        }

        private void TargetNextRoom()
        {
            currentRoom = currentRoom.NextRoom;
            targetWaypointIndex = 0;
        }

        private void LookAtDirection(Vector3 direction)
        {
            if (direction.magnitude < Mathf.Epsilon)
                return;

            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
        }

        private void Update()
        {
            if (isWaiting)
                return;

            var targetWaypoint = currentRoom.WaypointGroup.GetChild(targetWaypointIndex);
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, walkSpeed * Time.deltaTime);

            var isLastRoom = currentRoom == RoomManager.Singleton.LastLoadedRoom;
            var isLastWaypoint = targetWaypointIndex == currentRoom.WaypointGroup.childCount - 1;
            var distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

            reachedTarget = isLastRoom && isLastWaypoint
                ? distanceToWaypoint <= waitAtDoorDistance
                : distanceToWaypoint <= walkSpeed * Time.deltaTime;

            if (!reachedTarget)
            {
                var direction = Vector3.Normalize(targetWaypoint.position - transform.position);
                LookAtDirection(direction);
                return;
            }

            if (isLastWaypoint)
            {
                if (isLastRoom)
                {
                    isWaiting = true;
                    animator.SetBool("IsWalking", false);
                }
                else
                    TargetNextRoom();
            }
            else
                targetWaypointIndex++;
        }
    }
}
