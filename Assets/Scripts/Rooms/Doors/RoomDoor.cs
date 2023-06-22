using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Doors/Room Door")]
    [RequireComponent(typeof(BoxCollider))]
    public class RoomDoor : MonoBehaviour
    {
        public bool IsDeactivated { get; protected set; } = false;
        public UnityEvent OnDoorOpened { get; } = new();

        [SerializeField] protected BoxCollider closedCollider;
        [SerializeField] protected AudioClip breakThroughSound;

        protected Animator animator;
        protected AudioSource audioSource;
        protected bool isOpened = false;

        public void BreakThrough(string holeOwner)
        {
            if (isOpened || !IsDeactivated)
                return;

            transform.Find(holeOwner + "Hole").gameObject.SetActive(true);
            audioSource.PlayOneShot(breakThroughSound);
        }

        public virtual void Open()
        {
            if (isOpened || IsDeactivated)
                return;

            isOpened = true;
            closedCollider.enabled = false;
            OnDoorOpened?.Invoke();
            animator.Play("Open");
            audioSource.Play();
        }

        public virtual void Close()
        {
            if (!isOpened || IsDeactivated)
                return;

            isOpened = false;
            closedCollider.enabled = true;
            animator.Play("Close");
            audioSource.Play();
        }

        public virtual void Deactivate()
        {
            IsDeactivated = true;
        }

        private void Start()
        {
            TryGetComponent(out animator);
            TryGetComponent(out audioSource);

            if (audioSource == null)
                return;

            var rngPitch = Random.Range(-0.05f, 0.05f);
            audioSource.pitch += rngPitch;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isOpened || IsDeactivated || !other.CompareTag("Player"))
                return;

            Open();
        }
    }
}
