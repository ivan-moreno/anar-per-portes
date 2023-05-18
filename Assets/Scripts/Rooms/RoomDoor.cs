using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [RequireComponent(typeof(BoxCollider))]
    [AddComponentMenu("Anar per Portes/Room Door")]
    public class RoomDoor : MonoBehaviour
    {
        public event Action DoorOpened;
        private bool isOpened = false;

        private void OnTriggerEnter(Collider other)
        {
            if (isOpened)
                return;

            if (!other.CompareTag("Player"))
                return;

            isOpened = true;
            DoorOpened?.Invoke();
        }
    }
}
