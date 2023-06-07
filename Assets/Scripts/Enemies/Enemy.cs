using UnityEngine;

namespace AnarPerPortes
{
    public abstract class Enemy : MonoBehaviour
    {
        public EnemyTip Tip => tip;

        [SerializeField] private EnemyTip tip;

        protected AudioSource audioSource;
        protected Animator animator;
        protected Transform model;

        protected void CacheComponents()
        {
            audioSource = GetComponent<AudioSource>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
        }
    }
}
