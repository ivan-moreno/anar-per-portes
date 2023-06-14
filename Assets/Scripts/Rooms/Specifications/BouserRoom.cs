using AnarPerPortes.Enemies;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Bouser Room")]
    public class BouserRoom : Room
    {
        public Transform BouserWaypointGroup => bouserWaypointGroup;
        public Transform BouserSpawnPoint => bouserSpawnPoint;
        public bool PlayerIsInsideRoom { get; private set; } = false;

        [Header("Components")]
        [SerializeField] private Transform bouserWaypointGroup;
        [SerializeField] private Transform bouserSpawnPoint;
        [SerializeField] private Animator bouserRoomDoorsAnimator;

        [Header("Stats")]
        [SerializeField] private float spawnBouserDistance = 9f;
        [SerializeField] private float spawnBouserHardDistance = 38f;
        [SerializeField] private float closeEntranceDoorDistance = 32f;

        private bool isBouserAwake = false;

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
            if (LatestRoom() != this)
                return;

            var distance = Vector3.Distance(bouserRoomDoorsAnimator.transform.position, PlayerPosition());

            if (IsHardmodeEnabled() && !PlayerIsInsideRoom && distance <= spawnBouserHardDistance)
            {
                PlayerIsInsideRoom = true;
                RoomManager.Singleton.Rooms[^2].CloseDoor();
                WakeUpBouser();
                return;
            }

            if (!PlayerIsInsideRoom && distance <= closeEntranceDoorDistance)
            {
                PlayerIsInsideRoom = true;
                RoomManager.Singleton.Rooms[^2].CloseDoor();
            }

            if (isBouserAwake)
                return;

            if (distance <= spawnBouserDistance)
                WakeUpBouser();
        }
    }
}
