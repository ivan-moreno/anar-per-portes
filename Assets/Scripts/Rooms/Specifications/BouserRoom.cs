using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Bouser Room")]
    public class BouserRoom : Room
    {
        public Transform BouserWaypointGroup => bouserWaypointGroup;
        public Transform BouserSpawnPoint => bouserSpawnPoint;

        private static int spawnedBouserAmount = 0;

        [Header("Components")]
        [SerializeField] private Transform bouserWaypointGroup;
        [SerializeField] private Transform bouserSpawnPoint;
        [SerializeField] private Animator bouserRoomDoorsAnimator;

        [Header("Stats")]
        [SerializeField] private float spawnBouserDistance = 9f;
        [SerializeField] private float spawnBouserHardDistance = 25f;

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
            if (spawnedBouser)
                return;

            var distance = Vector3.Distance(bouserRoomDoorsAnimator.transform.position, PlayerController.Singleton.transform.position);
            var targetDistance = spawnedBouserAmount >= 1 ? spawnBouserHardDistance : spawnBouserDistance;

            if (SkellEnemy.IsOperative)
                targetDistance = spawnBouserHardDistance;
            else if (PedroEnemy.IsOperative)
                targetDistance = spawnBouserDistance;

            if (distance <= targetDistance)
                SpawnBouser();
        }
    }
}
