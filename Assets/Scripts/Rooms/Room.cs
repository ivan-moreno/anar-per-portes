using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Room")]
    public class Room : MonoBehaviour
    {
        public event Action DoorOpened;
        public Transform NextRoomGenerationPoint => nextRoomGenerationPoint;
        [SerializeField] private Transform nextRoomGenerationPoint;
        [SerializeField] private RoomDoor door;

        public void CloseDoor()
        {
            door.Close();
        }

        private void Start()
        {
            door.DoorOpened += DoorOpened;
        }

        private void OnDestroy()
        {
            door.DoorOpened -= DoorOpened;
        }
    }
}
