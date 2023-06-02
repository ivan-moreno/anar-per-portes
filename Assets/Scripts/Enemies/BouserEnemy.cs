using UnityEngine;

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
        private Vector3 targetLocation;

        private void Start()
        {
            var bouserRoom = RoomManager.Singleton.LastLoadedRoom as BouserRoom;
            transform.SetPositionAndRotation(bouserRoom.BouserSpawnPoint.position, bouserRoom.BouserSpawnPoint.rotation);
        }

        private void Update()
        {

        }
    }
}
