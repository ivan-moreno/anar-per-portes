using System;
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
        private Animator animator;
        private AudioSource audioSource;

        public void Open()
        {
            if (isOpened)
                return;

            isOpened = true;
            DoorOpened?.Invoke();
            animator.Play("Open");
            audioSource.Play();
        }

        public void Close()
        {
            if (!isOpened)
                return;

            isOpened = false;
            closedCollider.enabled = true;
            animator.Play("Close");
            audioSource.Play();
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isOpened || !other.CompareTag("Player"))
                return;

            Open();
        }
    }
}
