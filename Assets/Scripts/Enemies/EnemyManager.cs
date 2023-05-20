using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemy Manager")]
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private Transform enemiesGroup;
        [SerializeField] private GameObject pedroEnemyPrefab;
        private int roomsWithoutEnemySpawn = 0;

        private void Start()
        {
            Game.RoomManager.OnRoomGenerated.AddListener(ProcessEnemyPossibilities);
        }

        private void GenerateEnemy(GameObject enemyPrefab)
        {
            Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, enemiesGroup);
        }

        private void ProcessEnemyPossibilities(Room generatedRoom)
        {
            roomsWithoutEnemySpawn++;

            if (generatedRoom.HasHidingSpots && Game.RoomManager.Rooms.Count >= 5)
            {
                var rng = Random.Range(0, 100) - roomsWithoutEnemySpawn;

                if (!PedroEnemy.EnemyIsActive && rng < 10)
                {
                    GenerateEnemy(pedroEnemyPrefab);
                    roomsWithoutEnemySpawn = 0;
                }
            }
        }
    }
}
