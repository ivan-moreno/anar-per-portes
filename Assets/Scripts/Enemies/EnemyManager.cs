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

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.F1))
                GenerateEnemy(pedroEnemyPrefab);
#endif
        }

        private void GenerateEnemy(GameObject enemyPrefab)
        {
            var hasEnemyScript = enemyPrefab.TryGetComponent(out IEnemy enemy);

            if (hasEnemyScript)
            {
                if (!enemy.EnemyTipWasDisplayed && Game.Settings.EnemyTipSetting is EnemyTipSetting.ShowOnFirstEncounterAndWhenCaught)
                {
                    Game.EnemyTipManager.DisplayTip(enemy.TipTitle, enemy.TipMessage, enemy.TipRender, () => JustInstantiateEnemy(enemyPrefab));
                    enemy.EnemyTipWasDisplayed = true;
                }
                else
                    JustInstantiateEnemy(enemyPrefab);
            }
        }

        private void JustInstantiateEnemy(GameObject enemyPrefab)
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
