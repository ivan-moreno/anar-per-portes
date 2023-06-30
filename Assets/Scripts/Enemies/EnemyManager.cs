using AnarPerPortes.Enemies;
using AnarPerPortes.Rooms;
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

        public GameObject AssPancakesEnemyPrefab => assPancakesEnemyPrefab;
        public GameObject DaviloteEnemyPrefab => daviloteEnemyPrefab;
        public GameObject PedroEnemyPrefab => pedroEnemyPrefab;
        public GameObject SangotEnemyPrefab => sangotEnemyPrefab;
        public GameObject SheepyEnemyPrefab => sheepyEnemyPrefab;
        public GameObject SkellEnemyPrefab => skellEnemyPrefab;
        public Transform SangotRealm => sangotRealm;

        private static readonly HashSet<string> displayedEnemyTipNames = new();

        [Header("Components")]
        [SerializeField] private Transform enemiesGroup;
        [SerializeField] private Transform sangotRealm;

        [Header("Prefabs")]
        [SerializeField] private GameObject assPancakesEnemyPrefab;
        [SerializeField] private GameObject bouserEnemyPrefab;
        [SerializeField] private GameObject catalanBirdEnemyPrefab;
        [SerializeField] private GameObject daviloteEnemyPrefab;
        [SerializeField] private GameObject oshoskyEnemyPrefab;
        [SerializeField] private GameObject pedroEnemyPrefab;
        [SerializeField] private GameObject roblomanEnemyPrefab;
        [SerializeField] private GameObject sangotEnemyPrefab;
        [SerializeField] private GameObject sheepyEnemyPrefab;
        [SerializeField] private GameObject skellEnemyPrefab;
        [SerializeField] private GameObject yusufEnemyPrefab;
        [SerializeField] private GameObject specimen7EnemyPrefab;

        private EnemyEgg[] eggs;
        private readonly Dictionary<Type, Enemy> operativeEnemies = new();

        public Enemy SpawnEnemy(GameObject enemyPrefab, bool isDisguise = false)
        {
            if (EnemyIsOperative(enemyPrefab.GetComponent<Enemy>().GetType()))
                return null;

            if (CurrentSettings().EnableRoblomaniaticMode)
                isDisguise = true;
            else if (isDisguise && EnemyIsOperative<RoblomanEnemy>())
                return null;

            var instance = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, enemiesGroup);

            instance.TryGetComponent(out Enemy enemy);

            enemy.Spawn();

            TryDisplayTip(enemy);

            if (isDisguise)
                enemy.MarkAsRoblomanDisguise();

            return enemy;
        }

        public RoblomanEnemy SpawnRoblomanAt(Vector3 location)
        {
            var instance = Instantiate(roblomanEnemyPrefab, location, Quaternion.identity, enemiesGroup);
            var roblomanEnemy = instance.GetComponent<RoblomanEnemy>();
            roblomanEnemy.Spawn();
            return roblomanEnemy;
        }

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

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            CatalanBirdEnemy.IsCursed = false;

            RoomManager.Singleton.OnRoomGenerated.AddListener(x => TrySpawnEggs());

            eggs = new EnemyEgg[]
            {
                new EnemyEggBuilder()
                .WithId("Specimen7")
                .WithPrefab(specimen7EnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<Specimen7Room>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("GameMaker")
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<GameMakerRoom>()
                .WithOnSpawnCallback(egg => GameMakerEnemy.Singleton.Spawn())
                .Build(),

                new EnemyEggBuilder()
                .WithId("CatalanBird")
                .WithPrefab(catalanBirdEnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<CatalunyaRoom>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("Bouser")
                .WithPrefab(bouserEnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<BouserRoom>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("Yusuf")
                .WithPrefab(yusufEnemyPrefab)
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoom<IsleRoom>()
                .Build(),

                new EnemyEggBuilder()
                .WithId("Sangot")
                .WithPrefab(sangotEnemyPrefab)
                .WithMinRoom(35)
                .WithMinRoomsBetweenSpawns(10)
                .WithMaxRoomsBetweenSpawns(30)
                .WithBaseChance(20f)
                .WithChanceChangePerRoom(+1)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
                .IncompatibleWithRoom<IsleRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("Pedro")
                .WithPrefab(pedroEnemyPrefab)
                .WithMinRoom(15)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(20)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+1)
                //TODO Require room to have pedestals via Room Builder or something!
                .WithAdditionalRequirements(() => LatestRoom().HasPedestals)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("Oshosky")
                .WithPrefab(oshoskyEnemyPrefab)
                .WithMinRoom(20)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(20)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+5)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
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
                .WithId("Sheepy")
                .WithPrefab(sheepyEnemyPrefab)
                .WithMinRoom(5)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(20)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+5)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
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
                .WithId("Davilote")
                .WithPrefab(daviloteEnemyPrefab)
                .WithMinRoom(10)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(20)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+5)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
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
                .WithId("Skell-hear")
                .WithMinRoom(50)
                .WithMinRoomsBetweenSpawns(15)
                .WithMaxRoomsBetweenSpawns(25)
                .WithBaseChance(10f)
                .WithChanceChangePerRoom(+2)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .WithOnSpawnCallback(egg => SkellHearManager.Singleton.StartHearing())
                .Build(),

                new EnemyEggBuilder()
                .WithId("Skell")
                .WithPrefab(skellEnemyPrefab)
                .WithMinRoomsBetweenSpawns(0)
                .WithMaxRoomsBetweenSpawns(3)
                .WithBaseChance(30f)
                .WithChanceChangePerRoom(+5)
                .WithAdditionalRequirements(() => SkellHearManager.Singleton.IsHunting)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .MarkAsDisguisable()
                .Build(),

                new EnemyEggBuilder()
                .WithId("A-90")
                .WithMinRoom(90)
                .WithMinRoomsBetweenSpawns(4)
                .WithMaxRoomsBetweenSpawns(10)
                .WithBaseChance(20f)
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
                .IncompatibleWithRoom<BouserRoom>()
                .IncompatibleWithRoom<IsleRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithEnemy<BouserEnemy>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<SangotEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .WithOnSpawnCallback(egg => A90Enemy.Singleton.Spawn())
                .Build(),

                new EnemyEggBuilder()
                .WithId("Robloman")
                .WithMinRoom(40)
                .WithMinRoomsBetweenSpawns(5)
                .WithMaxRoomsBetweenSpawns(10)
                .WithBaseChance(40f)
                .WithChanceChangePerRoom(+5)
                .IncompatibleWithRoomSetId("Toymaker")
                .WithOnSpawnCallback(egg => SpawnRoblomanDisguise())
                .Build(),

                new EnemyEggBuilder()
                .WithId("Danylopez")
                .WithMinRoom(5)
                .WithMinRoomsBetweenSpawns(30)
                .WithMaxRoomsBetweenSpawns(50)
                .WithBaseChance(5f)
                .WithAdditionalRequirements(() => LatestRoom().HasPedestals && (LatestRoom().IsMediumSize || LatestRoom().IsLargeSize))
                .IncompatibleWithRoomSetId("Toymaker")
                .IncompatibleWithRoom<BouserBossRoom>()
                .IncompatibleWithRoom<BouserRoom>()
                .IncompatibleWithRoom<IsleRoom>()
                .IncompatibleWithRoom<Specimen7Room>()
                .IncompatibleWithRoom<GameMakerRoom>()
                .IncompatibleWithEnemy<BouserEnemy>()
                .IncompatibleWithEnemy<CatalanBirdEnemy>()
                .IncompatibleWithEnemy<GameMakerEnemy>()
                .IncompatibleWithEnemy<Specimen7Enemy>()
                .IncompatibleWithEnemy<YusufEnemy>()
                .WithOnSpawnCallback(egg => DanylopezEnemy.Singleton.Spawn())
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
                SpawnEnemy(oshoskyEnemyPrefab);
            else if (Input.GetKeyUp(KeyCode.F8))
                DanylopezEnemy.Singleton.Spawn();
            else if (Input.GetKeyUp(KeyCode.F9))
                A90Enemy.Singleton.Spawn();
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
