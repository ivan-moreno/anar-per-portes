using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Doors/Room Door")]
    public class RoomDoor : MonoBehaviour
    {
        public bool IsDeactivated { get; protected set; } = false;
        public UnityEvent OnDoorOpened { get; } = new();

        [Header("Components")]
        [SerializeField] protected BoxCollider closedCollider;
        [SerializeField] protected Animator animator;
        [SerializeField] protected AudioSource audioSource;

        [Header("Audio")]
        [SerializeField] protected AudioClip openSound;
        [SerializeField] protected AudioClip closeSound;
        [SerializeField] protected AudioClip breakThroughSound;

        protected bool isOpened = false;

        public void BreakThrough(string holeOwner)
        {
            if (isOpened || !IsDeactivated)
                return;

            var hole = transform.Find(holeOwner + "Hole");

            if (hole != null)
                hole.gameObject.SetActive(true);

            audioSource.PlayOneShot(breakThroughSound);
        }

        public virtual void Open()
        {
            if (isOpened || IsDeactivated)
                return;

            isOpened = true;
            closedCollider.enabled = false;
            OnDoorOpened?.Invoke();
            animator.Play("Open", 0, 0f);
            audioSource.PlayOneShot(openSound);
        }

        public virtual void Close()
        {
            if (!isOpened || IsDeactivated)
                return;

            isOpened = false;
            closedCollider.enabled = true;
            animator.Play("Close", 0, 0f);
            audioSource.PlayOneShot(closeSound);
        }

        public virtual void Deactivate()
        {
            IsDeactivated = true;
        }

        private void Start()
        {
            if (audioSource == null)
                return;

            var rngPitch = Random.Range(-0.05f, 0.05f);
            audioSource.pitch += rngPitch;

            PauseManager.Singleton.OnPauseChanged.AddListener(OnPauseChanged);
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isOpened || IsDeactivated || !other.CompareTag("Player"))
                return;

            Open();
        }
    }
}
