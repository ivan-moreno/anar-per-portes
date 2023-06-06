using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Skell Enemy")]
    public class SkellEnemy : Enemy
    {
        public static bool EnemyIsActive { get; set; } = false;
        [SerializeField] private float runSpeed = 16f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource warningSound;

    }
}
