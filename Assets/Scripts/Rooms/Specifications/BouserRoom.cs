using static AnarPerPortes.ShortUtils;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Bouser Room")]
    public class BouserRoom : Room
    {
        public Transform BouserWaypointGroup => bouserWaypointGroup;
        public Transform BouserSpawnPoint => bouserSpawnPoint;
        public bool PlayerIsInsideRoom { get; private set; } = false;

        private static int spawnedBouserAmount = 0;

        [Header("Components")]
        [SerializeField] private Transform bouserWaypointGroup;
        [SerializeField] private Transform bouserSpawnPoint;
        [SerializeField] private Animator bouserRoomDoorsAnimator;

        [Header("Stats")]
        [SerializeField] private float spawnBouserDistance = 9f;
        [SerializeField] private float spawnBouserHardDistance = 38f;
        [SerializeField] private float closeEntranceDoorDistance = 32f;

        private bool spawnedBouser = false;

        public void SpawnBouser()
        {
            if (spawnedBouser)
                return;

            spawnedBouser = true;
            spawnedBouserAmount++;
            PlayBouserDoorAnimation();
            EnemyManager.Singleton.GenerateEnemy(EnemyManager.Singleton.BouserEnemyPrefab);
        }

        public void OpenBouserDoor()
        {
            bouserRoomDoorsAnimator.Play("Open");
            bouserRoomDoorsAnimator.GetComponent<BoxCollider>().enabled = false;
        }

        private void PlayBouserDoorAnimation()
        {
            bouserRoomDoorsAnimator.Play("OpenBouser");
        }

        private void FixedUpdate()
        {
            if (RoomManager.Singleton.LastLoadedRoom != this)
                return;

            var distance = Vector3.Distance(bouserRoomDoorsAnimator.transform.position, PlayerController.Singleton.transform.position);

            if (IsHardmodeEnabled() && !PlayerIsInsideRoom && distance <= spawnBouserHardDistance)
            {
                PlayerIsInsideRoom = true;
                RoomManager.Singleton.Rooms[^2].CloseDoor();
                SpawnBouser();
                spawnedBouser = true;
                return;
            }

            if (!PlayerIsInsideRoom && distance <= closeEntranceDoorDistance)
            {
                PlayerIsInsideRoom = true;
                RoomManager.Singleton.Rooms[^2].CloseDoor();
            }

            if (spawnedBouser)
                return;

            var targetDistance = IsHardmodeEnabled() ? spawnBouserHardDistance : spawnBouserDistance;

            if (distance <= targetDistance)
                SpawnBouser();
        }
    }
}
