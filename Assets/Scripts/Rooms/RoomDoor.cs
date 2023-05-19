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
        [SerializeField] private BoxCollider closedCollider;
        private bool isOpened = false;

        public void Close()
        {
            closedCollider.enabled = true;
            GetComponent<Animator>().Play("Close");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isOpened)
                return;

            if (!other.CompareTag("Player"))
                return;

            isOpened = true;
            DoorOpened?.Invoke();
            GetComponent<Animator>().Play("Open");
        }
    }
}
