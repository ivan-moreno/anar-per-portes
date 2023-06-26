using AnarPerPortes.Enemies;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Rooms
{
    [AddComponentMenu("Anar per Portes/Rooms/Bouser Room")]
    public class BouserRoom : Room
    {
        public Transform BouserWaypointGroup => bouserWaypointGroup;
        public Transform BouserSpawnPoint => bouserSpawnPoint;
        public bool PlayerIsInsideRoom { get; private set; } = false;
        public bool BouserJrIsAwake { get; private set; } = false;

        [Header("Components")]
        [SerializeField] private Transform bouserWaypointGroup;
        [SerializeField] private Transform bouserSpawnPoint;
        [SerializeField] private Animator bouserRoomDoorsAnimator;
        [SerializeField] private GameObject bouserJrObject;

        [Header("Stats")]
        [SerializeField] private float spawnBouserDistance = 9f;
        [SerializeField] private float spawnBouserHardDistance = 38f;
        [SerializeField] private float spawnAssPancakesDistance = 18f;
        [SerializeField] private float closeEntranceDoorDistance = 32f;

        private bool isBouserAwake = false;
        private bool shouldWakeBouserJr = false;
        private bool shouldSpawnAssPancakes = false;
        private bool spawnedAssPancakes = false;

        public override void Initialize()
        {
            base.Initialize();
            shouldWakeBouserJr = Random.Range(0f, 100f) < 100f; //20f
            shouldSpawnAssPancakes = !shouldWakeBouserJr && Random.Range(0f, 100f) < 2f;
        }

        public void WakeUpBouser()
        {
            if (isBouserAwake)
                return;

            isBouserAwake = true;
            OpenBouserDoorAsDecorative();
            GetEnemyInstance<BouserEnemy>().WakeUp();
        }

        public void OpenBouserDoorAsDefeated()
        {
            bouserRoomDoorsAnimator.Play("Open");
            bouserRoomDoorsAnimator.GetComponent<BoxCollider>().enabled = false;
        }

        private void OpenBouserDoorAsDecorative()
        {
            bouserRoomDoorsAnimator.Play("OpenBouser");
        }

        private void FixedUpdate()
        {
            if (LatestRoom() != this || EnemyIsOperative<AssPancakesEnemy>())
                return;

            var distance = Vector3.Distance(bouserRoomDoorsAnimator.transform.position, PlayerPosition());

            //TODO: if ass pancakes will spawn logic
            if (shouldSpawnAssPancakes
                && !spawnedAssPancakes
                && !isBouserAwake
                && !IsHardmodeEnabled()
                && distance < spawnAssPancakesDistance)
            {
                spawnedAssPancakes = true;
                EnemyManager.Singleton.SpawnEnemy(EnemyManager.Singleton.AssPancakesEnemyPrefab);
                return;
            }
            else if (shouldWakeBouserJr
                && !BouserJrIsAwake
                && !isBouserAwake
                && !IsHardmodeEnabled()
                && distance < spawnAssPancakesDistance
                && (!EnemyIsOperative<PedroEnemy>() ||
                GetEnemyInstance<PedroEnemy>().IsOnBreak))
            {
                WakeUpBouserJr();
                return;
            }

            if (IsHardmodeEnabled() && !PlayerIsInsideRoom && distance <= spawnBouserHardDistance)
            {
                PlayerIsInsideRoom = true;
                RoomManager.Singleton.Rooms[^2].CloseDoor();
                RoomManager.Singleton.Rooms[^2].DeactivateDoor();
                WakeUpBouser();
                return;
            }

            if (!PlayerIsInsideRoom && distance <= closeEntranceDoorDistance)
            {
                PlayerIsInsideRoom = true;
                RoomManager.Singleton.Rooms[^2].CloseDoor();
                RoomManager.Singleton.Rooms[^2].DeactivateDoor();
            }

            if (isBouserAwake
                || BouserJrIsAwake
                || spawnedAssPancakes)
                return;

            if (distance <= spawnBouserDistance)
                WakeUpBouser();
        }

        private void WakeUpBouserJr()
        {
            bouserJrObject.SetActive(true);
            BouserJrIsAwake = true;
            OpenBouserDoorAsDecorative();
        }
    }
}
