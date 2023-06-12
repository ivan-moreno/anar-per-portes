using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Catalan Bird Driver Enemy")]
    public class CatalanBirdDriverEnemy : Enemy
    {
        public static bool IsOperative { get; private set; } = false;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
        }

        void Despawn()
        {
            //TODO: If is catching, don't despawn

            IsOperative = false;
            Destroy(gameObject);
        }
    }
}
