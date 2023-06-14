using UnityEngine;

namespace AnarPerPortes.Enemies
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
        protected bool isCatching = false;

        public virtual void Spawn()
        {
            EnemyManager.Singleton.MarkAsOperative(this);
        }

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

        protected virtual void Despawn()
        {
            if (isCatching)
                return;

            EnemyManager.Singleton.UnmarkAsOperative(this);

            if (IsRoblomanDisguise)
                EnemyManager.Singleton.UnmarkAsOperative<RoblomanEnemy>();

            Destroy(gameObject);
        }

        protected virtual RoblomanEnemy RevealRoblomanDisguise()
        {
            return EnemyManager.Singleton.SpawnRoblomanAt(transform.position);
        }
    }
}
