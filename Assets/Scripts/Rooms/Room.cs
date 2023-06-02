using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Room")]
    public class Room : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnDoorOpened;
        public bool HasHidingSpots => hasHidingSpots;
        public Transform NextRoomGenerationPoint => nextRoomGenerationPoint;
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

        protected virtual void Start()
        {
            door.OnDoorOpened.AddListener(() => OnDoorOpened?.Invoke());
        }
    }
}
