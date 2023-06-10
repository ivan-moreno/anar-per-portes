using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Enemy Manager")]
    public sealed class EnemyManager : MonoBehaviour
    {
        private sealed class EnemyPossibility
        {
            public GameObject EnemyPrefab { get; set; }
            public Func<Room, bool> SpawnRequirements { get; set; }
            public Func<EnemyPossibility, bool> RngRequirement { get; set; }
            public int RoomsWithoutSpawn { get; set; } = 0;
            public bool WillSpawn { get; set; } = false;

            public bool HasSpawnRequirements(Room generatedRoom)
            {
                return SpawnRequirements is not null && SpawnRequirements.Invoke(generatedRoom);
            }

            public bool HasRngRequirement()
            {
                return RngRequirement is not null && RngRequirement.Invoke(this);
            }

            public void DoSpawn()
            {
                if (EnemyPrefab != null)
                    EnemyManager.Singleton.GenerateEnemy(EnemyPrefab);

                RoomsWithoutSpawn = 0;
                WillSpawn = false;
            }
        }

        public static EnemyManager Singleton { get; private set; }

        //FIXME: Spawn Bouser and make him wait (inactive) instead of instantiating when his big door is opened.
        public Transform SangotRealm => sangotRealm;
        public GameObject BouserEnemyPrefab => bouserEnemyPrefab;
        public GameObject SkellEnemyPrefab => skellEnemyPrefab;
        public GameObject S7EnemyPrefab => s7EnemyPrefab;

        [SerializeField] private Transform enemiesGroup;
        [SerializeField] private Transform sangotRealm;
        [SerializeField] private GameObject daviloteEnemyPrefab;
        [SerializeField] private GameObject bouserEnemyPrefab;
        [SerializeField] private GameObject pedroEnemyPrefab;
        [SerializeField] private GameObject sangotEnemyPrefab;
        [SerializeField] private GameObject sheepyEnemyPrefab;
        [SerializeField] private GameObject skellEnemyPrefab;
        [SerializeField] private GameObject yusufEnemyPrefab;
        [SerializeField] private GameObject s7EnemyPrefab;
        [SerializeField] private A90Enemy a90Enemy;

        private int roomsWithoutAnyEnemySpawn = 0;
        private static readonly HashSet<string> displayedEnemyTipNames = new();

        private EnemyPossibility davilotePossibility;
        private EnemyPossibility pedroPossibility;
        private EnemyPossibility sangotPossibility;
        private EnemyPossibility sheepyPossibility;
        private EnemyPossibility skellPossibility;
        private EnemyPossibility a90Possibility;
        private readonly List<EnemyPossibility> allEnemyPossibilities = new();

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            // TODO: Find a more automatic way of solving this?
            BouserEnemy.IsOperative = false;
            DaviloteEnemy.IsOperative = false;
            PedroEnemy.IsOperative = false;
            SangotEnemy.IsOperative = false;
            SheepyEnemy.IsOperative = false;
            SkellEnemy.IsOperative = false;
            YusufEnemy.IsOperative = false;
            A90Enemy.IsOperative = false;
            S7Enemy.IsOperative = false;

            RoomManager.Singleton.OnRoomGenerated.AddListener(ProcessEnemyPossibilities);

            pedroPossibility = new()
            {
                EnemyPrefab = pedroEnemyPrefab,
                SpawnRequirements =
                    (room) => !PedroEnemy.IsOperative
                    && !SkellHearManager.Singleton.IsHearing
                    && !SkellEnemy.IsOperative
                    && room is not IsleRoom
                    && room.HasHidingSpots
                    && RoomManager.Singleton.LastOpenedRoomNumber >= 10,
                RngRequirement = (possibility) =>
                {
                    var rng = UnityEngine.Random.Range(0, 100);
                    rng += possibility.RoomsWithoutSpawn;
                    rng += roomsWithoutAnyEnemySpawn;

                    if (possibility.RoomsWithoutSpawn <= 4)
                        rng = 0;

                    return rng >= 70;
                }
            };

            allEnemyPossibilities.Add(pedroPossibility);

            davilotePossibility = new()
            {
                EnemyPrefab = daviloteEnemyPrefab,
                SpawnRequirements =
                    (room) => !DaviloteEnemy.IsOperative
                    && room is not BouserRoom
                    && room is not IsleRoom
                    && RoomManager.Singleton.LastOpenedRoomNumber >= 15,
                RngRequirement = (possibility) =>
                {
                    var rng = UnityEngine.Random.Range(0, 100);
                    rng += possibility.RoomsWithoutSpawn * 2;
                    rng += roomsWithoutAnyEnemySpawn;

                    if (possibility.RoomsWithoutSpawn <= 5)
                        rng = 0;

                    return rng >= 80;
                }
            };

            allEnemyPossibilities.Add(davilotePossibility);

            sangotPossibility = new()
            {
                EnemyPrefab = sangotEnemyPrefab,
                SpawnRequirements =
                    (room) => !SangotEnemy.IsOperative
                    && !DaviloteEnemy.IsOperative
                    && !SheepyEnemy.IsOperative
                    && !SkellEnemy.IsOperative
                    && !PedroEnemy.IsOperative
                    && room is not BouserRoom
                    && room is not IsleRoom
                    && RoomManager.Singleton.LastOpenedRoomNumber >= 20,
                RngRequirement = (possibility) =>
                {
                    var rng = UnityEngine.Random.Range(0, 100);
                    rng += possibility.RoomsWithoutSpawn * 2;
                    rng += roomsWithoutAnyEnemySpawn;

                    if (possibility.RoomsWithoutSpawn <= 7)
                        rng = 0;

                    return rng >= 80;
                }
            };

            allEnemyPossibilities.Add(sangotPossibility);

            sheepyPossibility = new()
            {
                EnemyPrefab = sheepyEnemyPrefab,
                SpawnRequirements =
                    (room) => !SheepyEnemy.IsOperative
                    && room is not IsleRoom
                    && RoomManager.Singleton.LastOpenedRoomNumber >= 5,
                RngRequirement = (possibility) =>
                {
                    var rng = UnityEngine.Random.Range(0, 100);
                    rng += possibility.RoomsWithoutSpawn * 2;
                    rng += roomsWithoutAnyEnemySpawn;

                    if (possibility.RoomsWithoutSpawn <= 5)
                        rng = 0;

                    return rng >= 80;
                }
            };

            allEnemyPossibilities.Add(sheepyPossibility);

            skellPossibility = new()
            {
                EnemyPrefab = null,
                SpawnRequirements =
                    (room) => !SkellEnemy.IsOperative
                    && !PedroEnemy.IsOperative
                    && room is not IsleRoom
                    && room is not BouserRoom
                    && RoomManager.Singleton.LastOpenedRoomNumber >= 30,
                RngRequirement = (possibility) =>
                {
                    var rng = UnityEngine.Random.Range(0, 100);
                    rng += possibility.RoomsWithoutSpawn;
                    rng += roomsWithoutAnyEnemySpawn;

                    if (possibility.RoomsWithoutSpawn <= 8)
                        rng = 0;

                    return rng >= 90;
                }
            };

            allEnemyPossibilities.Add(skellPossibility);

            a90Possibility = new()
            {
                EnemyPrefab = null,
                SpawnRequirements =
                    (room) => !A90Enemy.IsOperative
                    && room is not IsleRoom
                    && RoomManager.Singleton.LastOpenedRoomNumber >= 90,
                RngRequirement = (possibility) =>
                {
                    var rng = UnityEngine.Random.Range(0, 100);
                    rng += possibility.RoomsWithoutSpawn;
                    rng += roomsWithoutAnyEnemySpawn;

                    if (possibility.RoomsWithoutSpawn <= 3)
                        rng = 0;

                    return rng >= 90;
                }
            };

            allEnemyPossibilities.Add(a90Possibility);
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
                GenerateEnemy(skellEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F6))
                GenerateEnemy(sangotEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F7))
                a90Enemy.Spawn();
#endif
        }

        public void GenerateEnemy(GameObject enemyPrefab)
        {
            var hasEnemyScript = enemyPrefab.TryGetComponent(out Enemy enemy);

            if (!hasEnemyScript)
                return;

            var shouldDisplayTip = GameSettingsManager.Singleton.CurrentSettings.EnableEnemyTips && enemy.Tip != null;

            if (shouldDisplayTip && !displayedEnemyTipNames.Contains(enemyPrefab.name))
            {
                displayedEnemyTipNames.Add(enemyPrefab.name);

                EnemyTipManager.Singleton.DisplayTip(
                    enemy.Tip.Title,
                    enemy.Tip.Message,
                    enemy.Tip.Render,
                    () => Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, enemiesGroup));
            }
            else
                Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, enemiesGroup);
        }

        private void ProcessEnemyPossibilities(Room generatedRoom)
        {
            if (generatedRoom.name.StartsWith("S7Room"))
            {
                GenerateEnemy(s7EnemyPrefab);
                return;
            }

            if (generatedRoom is BouserRoom)
                roomsWithoutAnyEnemySpawn = 0;

            if (generatedRoom is IsleRoom)
            {
                GenerateEnemy(yusufEnemyPrefab);
                roomsWithoutAnyEnemySpawn = 0;
            }

            pedroPossibility.WillSpawn = pedroPossibility.HasSpawnRequirements(generatedRoom) && pedroPossibility.HasRngRequirement();
            davilotePossibility.WillSpawn = davilotePossibility.HasSpawnRequirements(generatedRoom) && davilotePossibility.HasRngRequirement();
            sheepyPossibility.WillSpawn = sheepyPossibility.HasSpawnRequirements(generatedRoom) && sheepyPossibility.HasRngRequirement();

            skellPossibility.WillSpawn =
                !pedroPossibility.WillSpawn
                && skellPossibility.HasSpawnRequirements(generatedRoom)
                && skellPossibility.HasRngRequirement();

            sangotPossibility.WillSpawn =
                !pedroPossibility.WillSpawn
                && !davilotePossibility.WillSpawn
                && !sheepyPossibility.WillSpawn
                && !skellPossibility.WillSpawn
                && sangotPossibility.HasSpawnRequirements(generatedRoom)
                && sangotPossibility.HasRngRequirement();

            a90Possibility.WillSpawn =
                !sheepyPossibility.WillSpawn
                && a90Possibility.HasSpawnRequirements(generatedRoom)
                && a90Possibility.HasRngRequirement();

            roomsWithoutAnyEnemySpawn++;

            foreach (var possibility in allEnemyPossibilities)
            {
                if (possibility.WillSpawn)
                {
                    if (possibility == a90Possibility)
                        a90Enemy.Spawn();

                    if (possibility == skellPossibility)
                        SkellHearManager.Singleton.StartHearing();

                    possibility.DoSpawn();
                    roomsWithoutAnyEnemySpawn = 0;
                }
                else
                    possibility.RoomsWithoutSpawn++;
            }
        }
    }
}
