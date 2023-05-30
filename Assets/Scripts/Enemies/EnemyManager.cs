using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Enemy Manager")]
    public sealed class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Singleton { get; private set; }
        [SerializeField] private Transform enemiesGroup;
        [SerializeField] private GameObject daviloteEnemyPrefab;
        [SerializeField] private GameObject pedroEnemyPrefab;
        [SerializeField] private GameObject yusufEnemyPrefab;
        private int roomsWithoutEnemySpawn = 0;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            RoomManager.Singleton.OnRoomGenerated.AddListener(ProcessEnemyPossibilities);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.F1))
                GenerateEnemy(pedroEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F2))
                GenerateEnemy(daviloteEnemyPrefab);
#endif
        }

        private void GenerateEnemy(GameObject enemyPrefab)
        {
            var hasEnemyScript = enemyPrefab.TryGetComponent(out IEnemy enemy);

            if (hasEnemyScript)
            {
                if (!enemy.EnemyTipWasDisplayed && GameSettingsManager.Singleton.CurrentSettings.EnemyTipSetting is EnemyTipSetting.ShowOnFirstEncounterAndWhenCaught)
                {
                    EnemyTipManager.Singleton.DisplayTip(enemy.TipTitle, enemy.TipMessage, enemy.TipRender, () => JustInstantiateEnemy(enemyPrefab));
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
            if (RoomManager.Singleton.LastOpenedRoomNumber <= 1)
                return;

            roomsWithoutEnemySpawn++;

            if (generatedRoom is IsleRoom)
            {
                GenerateEnemy(yusufEnemyPrefab);
                roomsWithoutEnemySpawn = 0;
                return;
            }

            if (generatedRoom.HasHidingSpots && RoomManager.Singleton.LastOpenedRoomNumber >= 5)
            {
                //TODO: Redesign this method of RNG
                var rng = Random.Range(0, 100) - roomsWithoutEnemySpawn;

                if (!PedroEnemy.EnemyIsActive && rng < 10)
                {
                    GenerateEnemy(pedroEnemyPrefab);
                    roomsWithoutEnemySpawn = 0;
                }
            }

            if (generatedRoom is not IsleRoom)
            {
                var rng = Random.Range(0, 100) + roomsWithoutEnemySpawn * 3;

                if (!DaviloteEnemy.EnemyIsActive && rng >= 70)
                {
                    GenerateEnemy(daviloteEnemyPrefab);
                    roomsWithoutEnemySpawn = 0;
                }
            }
        }
    }
}
