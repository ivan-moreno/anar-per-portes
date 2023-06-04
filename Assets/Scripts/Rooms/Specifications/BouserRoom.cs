using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Rooms/Bouser Room")]
    public class BouserRoom : Room
    {
        public Transform BouserWaypointGroup => bouserWaypointGroup;
        public Transform BouserSpawnPoint => bouserSpawnPoint;
        [SerializeField] private Transform bouserWaypointGroup;
        [SerializeField] private Transform bouserSpawnPoint;
        [SerializeField] private Animator bouserRoomDoorsAnimator;
        private bool spawnedBouser = false;
        private const float spawnBouserDistance = 9f;

        public void SpawnBouser()
        {
            if (spawnedBouser)
                return;

            spawnedBouser = true;
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
            
            if (distance <= spawnBouserDistance)
            {
                SpawnBouser();
            }
        }
    }
}
