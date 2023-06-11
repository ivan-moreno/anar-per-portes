using UnityEngine;

namespace AnarPerPortes
{
    public abstract class Enemy : MonoBehaviour
    {
        public bool IsRoblomanDisguise { get; private set; } = false;
        public EnemyTip Tip => tip;

        [SerializeField] private EnemyTip tip;
        [SerializeField] private GameObject roblomanDisguiseObject;

        protected AudioSource audioSource;
        protected Animator animator;
        protected Transform model;

        public void MarkAsRoblomanDisguise()
        {
            IsRoblomanDisguise = true;
            roblomanDisguiseObject.SetActive(true);
        }

        protected void CacheComponents()
        {
            audioSource = GetComponent<AudioSource>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
        }

        protected virtual RoblomanEnemy RevealRoblomanDisguise()
        {
            return EnemyManager.Singleton.SpawnRoblomanAt(transform.position);
        }
    }
}
