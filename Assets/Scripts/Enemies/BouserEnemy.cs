namespace AnarPerPortes
{
    public class BouserEnemy : Enemy
    {
        public static bool EnemyIsActive { get; private set; } = false;

        public override bool EnemyTipWasDisplayed
        {
            get => enemyTipWasDisplayed;
            set => enemyTipWasDisplayed = value;
        }

        private static bool enemyTipWasDisplayed = false;

        private void Start()
        {

        }

        private void Update()
        {

        }
    }
}
