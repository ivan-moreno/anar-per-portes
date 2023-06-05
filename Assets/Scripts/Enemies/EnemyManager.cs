using TMPro;
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
        [SerializeField] private GameObject sheepyEnemyPrefab;
        [SerializeField] private GameObject yusufEnemyPrefab;
        [SerializeField] private A90Enemy a90Enemy;
        [SerializeField] private TMP_Text _debugEnemyLabel;
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
            else if (Input.GetKeyUp(KeyCode.F4))
                GenerateEnemy(sheepyEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F5))
                a90Enemy.Spawn();
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
            _debugEnemyLabel.text = string.Empty;

            roomsWithoutEnemySpawn++;

            if (!A90Enemy.EnemyIsActive
                && generatedRoom is not BouserRoom
                && generatedRoom is not IsleRoom
                && RoomManager.Singleton.LastOpenedRoomNumber >= 90
                && !PedroEnemy.EnemyIsActive
                && !SheepyEnemy.EnemyIsActive)
            {
                var rng = Random.Range(0, 100) + roomsWithoutEnemySpawn;

                if (rng > 90)
                {
                    a90Enemy.Spawn();
                    roomsWithoutEnemySpawn = 0;
                }
            }

            if (generatedRoom is BouserRoom)
            {
                _debugEnemyLabel.text += "\n100%\tBouser";
                roomsWithoutEnemySpawn = 0;
            }
            else
                _debugEnemyLabel.text += "\n0%\t\tBouser";

            if (generatedRoom is IsleRoom)
            {
                _debugEnemyLabel.text += "\n100%\tYusuf";
                GenerateEnemy(yusufEnemyPrefab);
                roomsWithoutEnemySpawn = 0;
            }
            else
                _debugEnemyLabel.text += "\n0%\t\tYusuf";

            if (generatedRoom is not BouserRoom
                && generatedRoom is not IsleRoom
                && generatedRoom.HasHidingSpots
                && RoomManager.Singleton.LastOpenedRoomNumber >= 5)
            {
                var rng = Random.Range(0, 100) + roomsWithoutEnemySpawn;

                _debugEnemyLabel.text += "\n" + (100 - 80 + roomsWithoutEnemySpawn) + "%\t\tPedro";

                if (!PedroEnemy.EnemyIsActive && rng >= 80)
                {
                    _debugEnemyLabel.text += "\t<color=#0f0>[IN]</color>";
                    GenerateEnemy(pedroEnemyPrefab);
                    roomsWithoutEnemySpawn = 0;
                }
            }
            else
                _debugEnemyLabel.text += "\n0%\t\tPedro";

            if (generatedRoom is not BouserRoom
                && generatedRoom is not IsleRoom
                && RoomManager.Singleton.LastOpenedRoomNumber >= 5)
            {
                var rng = Random.Range(0, 100) + roomsWithoutEnemySpawn * 3;

                _debugEnemyLabel.text += "\n" + (100 - 70 + roomsWithoutEnemySpawn * 3) + "%\t\tDavilote";

                if (!DaviloteEnemy.EnemyIsActive && rng >= 70)
                {
                    _debugEnemyLabel.text += "\t<color=#0f0>[IN]</color>";
                    GenerateEnemy(daviloteEnemyPrefab);
                    roomsWithoutEnemySpawn = 0;
                }
            }
            else
                _debugEnemyLabel.text += "\n0%\t\tDavilote";

            if (generatedRoom is not BouserRoom
                && generatedRoom is not IsleRoom
                && RoomManager.Singleton.LastOpenedRoomNumber >= 5)
            {
                var rng = Random.Range(0, 100) + roomsWithoutEnemySpawn * 3;

                _debugEnemyLabel.text += "\n" + (100 - 70 + roomsWithoutEnemySpawn * 3) + "%\t\tSheepy";

                if (!SheepyEnemy.EnemyIsActive && !DaviloteEnemy.EnemyIsActive && rng >= 70)
                {
                    _debugEnemyLabel.text += "\t<color=#0f0>[IN]</color>";
                    GenerateEnemy(sheepyEnemyPrefab);
                    roomsWithoutEnemySpawn = 0;
                }
            }
            else
                _debugEnemyLabel.text += "\n0%\t\tSheepy";
        }
    }
}
