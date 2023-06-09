using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Entities/Pom Pom Entity")]
    public class PomPomEntity : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField][Min(0f)] private float walkSpeed = 1f;

        private Animator animator;
        private Transform model;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool waiting = false;
        private int waypointsTraversed = 0;
        private int roomsTraversed = 0;

        private void Start()
        {
            transform.SetParent(null);
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;

            transform.position = RoomManager.Singleton.Rooms[0].transform.position;
            targetLocation = RoomManager.Singleton.Rooms[0].WaypointGroup.GetChild(0).position;
            animator.SetBool("IsWalking", true);
            RoomManager.Singleton.OnRoomGenerated.AddListener(RoomGenerated);
        }

        private void RoomGenerated(Room room)
        {
            if (roomsTraversed >= RoomManager.Singleton.Rooms.Count)
                roomsTraversed = RoomManager.Singleton.Rooms.Count - 1;

            var waypointGroup = RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup;

            targetLocation = waypointGroup.GetChild(waypointsTraversed).position;
            waiting = false;
        }

        private void Update()
        {
            var nextPosition = Vector3.MoveTowards(transform.position, targetLocation, walkSpeed * Time.deltaTime);
            transform.position = nextPosition;

            if (waiting)
            {

            }
            else if (waypointsTraversed == RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup.childCount - 1)
                reachedTarget = Vector3.Distance(transform.position, targetLocation) <= 5f;
            else
                reachedTarget = Vector3.Distance(transform.position, targetLocation) <= walkSpeed * Time.deltaTime;

            if (reachedTarget && !waiting)
            {
                if (waypointsTraversed >= RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup.childCount - 1)
                {
                    roomsTraversed++;
                    waypointsTraversed = 0;
                }
                else
                    waypointsTraversed++;

                if (roomsTraversed < RoomManager.Singleton.Rooms.Count)
                    targetLocation = RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup.GetChild(waypointsTraversed).position;
                else
                {
                    waiting = true;
                    targetLocation = transform.position;
                }
            }

            animator.SetBool("IsWalking", !waiting);

            if (walkSpeed <= 0f)
                return;

            var direction = Vector3.Normalize(targetLocation - transform.position);

            if (direction.magnitude < Mathf.Epsilon)
                return;

            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
        }
    }
}
