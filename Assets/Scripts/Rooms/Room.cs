using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Room")]
    public class Room : MonoBehaviour
    {
        [HideInInspector] public UnityEvent OnDoorOpened;
        public bool HasHidingSpots => hasHidingSpots;
        public Transform NextRoomGenerationPoint => nextRoomGenerationPoint;
        [SerializeField] private bool hasHidingSpots = false;
        [SerializeField] private Transform nextRoomGenerationPoint;
        [SerializeField] private RoomDoor door;

        public void CloseDoor()
        {
            door.Close();
        }

        private void Start()
        {
            door.OnDoorOpened.AddListener(() => OnDoorOpened?.Invoke());
        }
    }
}
