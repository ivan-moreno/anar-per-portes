using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [RequireComponent(typeof(BoxCollider))]
    [AddComponentMenu("Anar per Portes/Room Door")]
    public class RoomDoor : MonoBehaviour
    {
        public UnityEvent OnDoorOpened { get; } = new();
        [SerializeField] private BoxCollider closedCollider;
        private bool isOpened = false;
        private Animator animator;
        private AudioSource audioSource;

        public void Open()
        {
            if (isOpened)
                return;

            isOpened = true;
            OnDoorOpened?.Invoke();
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

        public void Deactivate()
        {
            isOpened = true;
            closedCollider.enabled = true;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            var rngPitch = Random.Range(-0.05f, 0.05f);
            audioSource.pitch += rngPitch;
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
