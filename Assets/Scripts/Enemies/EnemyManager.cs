using AnarPerPortes.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Enemy Manager")]
    public sealed class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Singleton { get; private set; }

        public GameObject SkellEnemyPrefab => skellEnemyPrefab;
        public Transform SangotRealm => sangotRealm;

        private static readonly HashSet<string> displayedEnemyTipNames = new();

        [Header("Components")]
        [SerializeField] private Transform enemiesGroup;
        [SerializeField] private Transform sangotRealm;
        [SerializeField] private A90Enemy a90Enemy;

        [Header("Prefabs")]
        [SerializeField] private GameObject bouserEnemyPrefab;
        [SerializeField] private GameObject catalanBirdEnemyPrefab;
        [SerializeField] private GameObject daviloteEnemyPrefab;
        [SerializeField] private GameObject pedroEnemyPrefab;
        [SerializeField] private GameObject roblomanEnemyPrefab;
        [SerializeField] private GameObject sangotEnemyPrefab;
        [SerializeField] private GameObject sheepyEnemyPrefab;
        [SerializeField] private GameObject skellEnemyPrefab;
        [SerializeField] private GameObject yusufEnemyPrefab;
        [SerializeField] private GameObject specimen7EnemyPrefab;

        private EnemyEgg[] eggs;
        private readonly Dictionary<Type, Enemy> operativeEnemies = new();

        public Enemy GetEnemyInstance<T>() where T : Enemy
        {
            if (operativeEnemies.TryGetValue(typeof(T), out var enemyInstance))
                return enemyInstance;

            return null;
        }

        public void MarkAsOperative(Enemy enemyInstance)
        {
            if (operativeEnemies.ContainsKey(enemyInstance.GetType()))
            {
                operativeEnemies[enemyInstance.GetType()] = enemyInstance;
                return;
            }

            operativeEnemies.Add(enemyInstance.GetType(), enemyInstance);
        }

        public void UnmarkAsOperative<T>() where T : Enemy
        {
            operativeEnemies.Remove(typeof(T));
        }

        public void UnmarkAsOperative(Enemy enemyInstance)
        {
            operativeEnemies.Remove(enemyInstance.GetType());
        }

        public bool EnemyIsOperative<T>() where T : Enemy
        {
            return operativeEnemies.ContainsKey(typeof(T));
        }

        public bool EnemyIsOperative(Type type)
        {
            return operativeEnemies.ContainsKey(type);
        }

        public void SpawnEnemy(GameObject enemyPrefab, bool isDisguise = false)
        {
            if (isDisguise && EnemyIsOperative<RoblomanEnemy>())
                return;

            var instance = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, enemiesGroup);

            instance.TryGetComponent(out Enemy enemy);
            enemy.Spawn();

            TryDisplayTip(enemy);

            if (isDisguise)
                instance.GetComponent<Enemy>().MarkAsRoblomanDisguise();
        }

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
            CatalanBirdEnemy.IsCursed = false;

            RoomManager.Singleton.OnRoomGenerated.AddListener((_) => TrySpawnEggs());

            eggs = new EnemyEgg[]
            {
                new EnemyEggBuilder()
                .WithId("specimen7")
                .WithPrefab(specimen7EnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<Specimen7Room>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("catalan-bird")
                .WithPrefab(catalanBirdEnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<CatalunyaRoom>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("bouser")
                .WithPrefab(bouserEnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<BouserRoom>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("yusuf")
                .WithPrefab(yusufEnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<IsleRoom>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("sangot")
                .WithPrefab(sangotEnemyPrefab)
                .WithMinRoom(35)
                .WithMinRoomsBetweenSpawns(10)
                .WithMaxRoomsBetweenSpawns(30)
                .WithBaseChance(20f)
                .WithChanceChangePerRoom(+1)
                .IncompatibleWithRoom<IsleRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("sheepy")
                .WithPrefab(sheepyEnemyPrefab)
                .WithMinRoom(5)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(20)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+5)
                .IncompatibleWithRoom<IsleRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<A90Enemy>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("davilote")
                .WithPrefab(daviloteEnemyPrefab)
                .WithMinRoom(10)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(20)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+5)
                .IncompatibleWithRoom<BouserRoom>()
                .IncompatibleWithRoom<IsleRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<BouserEnemy>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("pedro")
                .WithPrefab(pedroEnemyPrefab)
                .WithMinRoom(15)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(20)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+1)
                //TODO Require room to have pedestals via Room Builder or something!
                .WithAdditionalRequirements(() => LatestRoom().HasPedestals)
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("skell-hear")
                .WithMinRoom(50)
                .WithMinRoomsBetweenSpawns(15)
                .WithMaxRoomsBetweenSpawns(25)
                .WithBaseChance(10f)
                .WithChanceChangePerRoom(+2)
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .WithOnSpawnCallback(egg => SkellHearManager.Singleton.StartHearing())
                .Build(),

                new EnemyEggBuilder()
                .WithId("skell")
                .WithPrefab(skellEnemyPrefab)
                .WithMinRoomsBetweenSpawns(0)
                .WithMaxRoomsBetweenSpawns(3)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+5)
                .WithAdditionalRequirements(() => SkellHearManager.Singleton.IsHunting)
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("a-90")
                .WithMinRoom(90)
                .WithMinRoomsBetweenSpawns(4)
                .WithMaxRoomsBetweenSpawns(10)
                .WithBaseChance(20f)
                .IncompatibleWithRoom<BouserRoom>()
                .IncompatibleWithRoom<IsleRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<BouserEnemy>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .WithOnSpawnCallback(egg => a90Enemy.Spawn())
                .Build(),

                new EnemyEggBuilder()
                .WithId("robloman")
                .WithMinRoom(40)
                .WithMinRoomsBetweenSpawns(5)
                .WithMaxRoomsBetweenSpawns(10)
                .WithBaseChance(40f)
                .WithChanceChangePerRoom(+5)
                .WithOnSpawnCallback(egg => SpawnRoblomanDisguise())
                .Build(),
            };
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.F2))
                SpawnEnemy(pedroEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F3))
                SpawnEnemy(daviloteEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F4))
                SpawnEnemy(sheepyEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F5))
                SpawnEnemy(skellEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F6))
                SpawnEnemy(sangotEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F7))
                a90Enemy.Spawn();
#endif
        }

        private void TryDisplayTip(Enemy enemy)
        {
            var targetName = enemy.IsRoblomanDisguise ? "robloman" : enemy.name;
            var targetTip = enemy.IsRoblomanDisguise ? roblomanEnemyPrefab.GetComponent<Enemy>().Tip : enemy.Tip;

            var shouldDisplayTip =
                GameSettingsManager.Singleton.CurrentSettings.EnableEnemyTips
                && targetTip != null
                && !displayedEnemyTipNames.Contains(targetName);

            if (!shouldDisplayTip)
                return;

            displayedEnemyTipNames.Add(targetName);
            EnemyTipManager.Singleton.DisplayTip(targetTip);
        }

        private void TrySpawnEggs()
        {
            foreach (var egg in eggs)
                egg.TrySpawn();
        }

        private void SpawnRoblomanDisguise()
        {
            eggs.Where(egg => egg.IsDisguiseable).ToArray().RandomItem().SpawnAsDisguise();
        }
    }
}
