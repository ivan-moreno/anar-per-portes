using System;
using System.Collections.Generic;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

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

            public void DoSpawnAsRoblomanDisguise()
            {
                if (EnemyPrefab != null)
                    EnemyManager.Singleton.GenerateRoblomanDisguiseEnemy(EnemyPrefab);

                WillSpawn = false;
            }
        }

        public static EnemyManager Singleton { get; private set; }

        //FIXME: Spawn Bouser and make him wait (inactive) instead of instantiating when his big door is opened.
        public Transform SangotRealm => sangotRealm;
        public GameObject BouserEnemyPrefab => bouserEnemyPrefab;
        public GameObject SkellEnemyPrefab => skellEnemyPrefab;
        public GameObject SkellBetaEnemyPrefab => skellBetaEnemyPrefab;
        public GameObject S7EnemyPrefab => s7EnemyPrefab;

        [Header("Stats")]
        [SerializeField] private float skellChance = 5f;
        [SerializeField] private int skellMinDoorsOpened = 35;
        [SerializeField] private float roblomanChance = 5f;
        [SerializeField] private int roblomanMinDoorsOpened = 40;


        [Header("Components")]
        [SerializeField] private Transform enemiesGroup;
        [SerializeField] private Transform sangotRealm;
        [SerializeField] private A90Enemy a90Enemy;

        [Header("Prefabs")]
        [SerializeField] private GameObject daviloteEnemyPrefab;
        [SerializeField] private GameObject bouserEnemyPrefab;
        [SerializeField] private GameObject pedroEnemyPrefab;
        [SerializeField] private GameObject roblomanEnemyPrefab;
        [SerializeField] private GameObject sangotEnemyPrefab;
        [SerializeField] private GameObject sheepyEnemyPrefab;
        [SerializeField] private GameObject skellEnemyPrefab;
        [SerializeField] private GameObject skellBetaEnemyPrefab;
        [SerializeField] private GameObject yusufEnemyPrefab;
        [SerializeField] private GameObject s7EnemyPrefab;

        private int roomsWithoutAnyEnemySpawn = 0;
        private static readonly HashSet<string> displayedEnemyTipNames = new();

        private EnemyPossibility davilotePossibility;
        private EnemyPossibility pedroPossibility;
        private EnemyPossibility sangotPossibility;
        private EnemyPossibility sheepyPossibility;
        private EnemyPossibility skellPossibility;
        private EnemyPossibility skellBetaPossibility;
        private EnemyPossibility a90Possibility;
        private readonly List<EnemyPossibility> allEnemyPossibilities = new();

        public RoblomanEnemy SpawnRoblomanAt(Vector3 location)
        {
            var instance = Instantiate(roblomanEnemyPrefab, location, Quaternion.identity, enemiesGroup);
            return instance.GetComponent<RoblomanEnemy>();
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            // TODO: Use a HashSet with enemies that are operative
            BouserEnemy.IsOperative = false;
            DaviloteEnemy.IsOperative = false;
            PedroEnemy.IsOperative = false;
            SangotEnemy.IsOperative = false;
            RoblomanEnemy.IsOperative = false;
            SheepyEnemy.IsOperative = false;
            SkellEnemy.IsOperative = false;
            SkellBetaEnemy.IsOperative = false;
            YusufEnemy.IsOperative = false;
            A90Enemy.IsOperative = false;
            S7Enemy.IsOperative = false;
            CatalanBirdDriverEnemy.IsCursed = false;

            RoomManager.Singleton.OnRoomGenerated.AddListener(ProcessEnemyPossibilities);

            pedroPossibility = new()
            {
                EnemyPrefab = pedroEnemyPrefab,
                SpawnRequirements =
                    (room) => !PedroEnemy.IsOperative
                    && !SkellHearManager.Singleton.IsHearing
                    && !SkellBetaEnemy.IsOperative
                    && room is not IsleRoom
                    && room.HasHidingSpots
                    && (IsHardmodeEnabled() || RoomManager.Singleton.LastOpenedRoomNumber >= 10),
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
                    && (IsHardmodeEnabled() || RoomManager.Singleton.LastOpenedRoomNumber >= 15),
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
                    && !SkellBetaEnemy.IsOperative
                    && !PedroEnemy.IsOperative
                    && room is not BouserRoom
                    && room is not IsleRoom
                    && (IsHardmodeEnabled() || RoomManager.Singleton.LastOpenedRoomNumber >= skellMinDoorsOpened),
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
                    && (IsHardmodeEnabled() || RoomManager.Singleton.LastOpenedRoomNumber >= 5),
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
                EnemyPrefab = skellEnemyPrefab,
                SpawnRequirements =
                    (room) => !SkellEnemy.IsOperative
                    && SkellHearManager.Singleton.IsHunting
                    && room is not BouserRoom
                    && (IsHardmodeEnabled() || RoomManager.Singleton.LastOpenedRoomNumber >= 0), //30
                RngRequirement = (possibility) =>
                {
                    var rng = UnityEngine.Random.Range(0, 100);
                    rng += possibility.RoomsWithoutSpawn * 5;
                    rng += roomsWithoutAnyEnemySpawn * 50;

                    if (possibility.RoomsWithoutSpawn <= 2)
                        rng = 0;

                    return rng >= 90;
                }
            };

            allEnemyPossibilities.Add(skellPossibility);

            // Unused
            skellBetaPossibility = new()
            {
                EnemyPrefab = null,
                SpawnRequirements =
                    (room) => !SkellBetaEnemy.IsOperative
                    && !PedroEnemy.IsOperative
                    && room is not IsleRoom
                    && room is not BouserRoom
                    && (IsHardmodeEnabled() || RoomManager.Singleton.LastOpenedRoomNumber >= 30),
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

            a90Possibility = new()
            {
                EnemyPrefab = null,
                SpawnRequirements =
                    (room) => !A90Enemy.IsOperative
                    && room is not IsleRoom
                    && (IsHardmodeEnabled() || RoomManager.Singleton.LastOpenedRoomNumber >= 90),
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
                GenerateEnemy(skellBetaEnemyPrefab);
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

        public void GenerateRoblomanDisguiseEnemy(GameObject enemyPrefab)
        {
            if (RoblomanEnemy.IsOperative)
                return;

            var shouldDisplayTip =
                GameSettingsManager.Singleton.CurrentSettings.EnableEnemyTips
                && !displayedEnemyTipNames.Contains(roblomanEnemyPrefab.name);

            if (shouldDisplayTip)
            {
                displayedEnemyTipNames.Add(roblomanEnemyPrefab.name);

                var roblomanTip = roblomanEnemyPrefab.GetComponent<RoblomanEnemy>().Tip;

                EnemyTipManager.Singleton.DisplayTip(
                    roblomanTip.Title,
                    roblomanTip.Message,
                    roblomanTip.Render);
            }

            var instance = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, enemiesGroup);
            instance.GetComponent<Enemy>().MarkAsRoblomanDisguise();
            RoblomanEnemy.IsOperative = true;
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

            if ((RoomManager.Singleton.LastOpenedRoomNumber >= skellMinDoorsOpened || IsHardmodeEnabled())
                && UnityEngine.Random.Range(0f, 100f) <= skellChance)
                SkellHearManager.Singleton.StartHearing();

            skellPossibility.WillSpawn =
                skellPossibility.HasSpawnRequirements(generatedRoom)
                && skellPossibility.HasRngRequirement();

            // Unused
            skellBetaPossibility.WillSpawn =
                !pedroPossibility.WillSpawn
                && skellBetaPossibility.HasSpawnRequirements(generatedRoom)
                && skellBetaPossibility.HasRngRequirement();

            sangotPossibility.WillSpawn =
                !pedroPossibility.WillSpawn
                && !davilotePossibility.WillSpawn
                && !sheepyPossibility.WillSpawn
                && !skellBetaPossibility.WillSpawn
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

                    var canSpawnRoblomanDisguise =
                        !RoblomanEnemy.IsOperative
                        && (RoomManager.Singleton.LastOpenedRoomNumber >= roblomanMinDoorsOpened || IsHardmodeEnabled())
                        && UnityEngine.Random.Range(0f, 100f) <= roblomanChance;

                    if (canSpawnRoblomanDisguise)
                        possibility.DoSpawnAsRoblomanDisguise();
                    else
                        possibility.DoSpawn();

                    roomsWithoutAnyEnemySpawn = 0;
                }
                else
                    possibility.RoomsWithoutSpawn++;
            }
        }
    }
}
