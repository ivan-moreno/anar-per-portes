using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Skell Enemy")]
    public class SkellEnemy : Enemy
    {
        public static bool EnemyIsActive { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private float runSpeed = 16f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private float catchRange = 2f;

        [Header("Sound")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource jumpscareSound;

        private void Start()
        {
            EnemyIsActive = true;
            CacheComponents();
        }
    }
}
