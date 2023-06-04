using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Enemy Manager")]
    public sealed class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Singleton { get; private set; }
        public GameObject BouserEnemyPrefab => bouserEnemyPrefab;
        [SerializeField] private Transform enemiesGroup;
        [SerializeField] private GameObject daviloteEnemyPrefab;
        [SerializeField] private GameObject bouserEnemyPrefab;
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
            if (Input.GetKeyUp(KeyCode.F2))
                GenerateEnemy(pedroEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F3))
                GenerateEnemy(daviloteEnemyPrefab);
#endif
        }

        public void GenerateEnemy(GameObject enemyPrefab)
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
            roomsWithoutEnemySpawn++;

            if (generatedRoom is BouserRoom)
            {
                roomsWithoutEnemySpawn = 0;
            }

            if (generatedRoom is IsleRoom)
            {
                GenerateEnemy(yusufEnemyPrefab);
                roomsWithoutEnemySpawn = 0;
            }

            if (generatedRoom is not BouserRoom
                && generatedRoom is not IsleRoom
                && generatedRoom.HasHidingSpots
                && RoomManager.Singleton.LastOpenedRoomNumber >= 5)
            {
                //TODO: Redesign this method of RNG
                var rng = Random.Range(0, 100) - roomsWithoutEnemySpawn;

                if (!PedroEnemy.EnemyIsActive && rng < 10)
                {
                    GenerateEnemy(pedroEnemyPrefab);
                    roomsWithoutEnemySpawn = 0;
                }
            }

            if (generatedRoom is not BouserRoom
                && generatedRoom is not IsleRoom
                && RoomManager.Singleton.LastOpenedRoomNumber >= 5)
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
