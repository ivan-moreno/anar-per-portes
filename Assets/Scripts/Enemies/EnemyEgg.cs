using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    public class EnemyEgg
    {
        public GameObject Prefab { get; set; }
        public string Id { get; set; }
        public float BaseChance { get; set; }
        public float ChanceChangePerRoom { get; set; }
        public float MinChance { get; set; }
        public int MaxSpawnCount { get; set; }
        public int MinRoom { get; set; } = 1;
        public int MaxRoom { get; set; } = RoomManager.maxGeneratedRooms;
        public int MinRoomsBetweenSpawns { get; set; }
        public int MaxRoomsBetweenSpawns { get; set; }
        public bool IsDisguiseable { get; set; } = false;
        public bool IsForcedSpawnOnly { get; set; } = false;
        public bool IsNormalmodeExclusive { get; set; } = false;
        public bool IsHardmodeExclusive { get; set; } = false;
        public Func<bool> AdditionalRequirements { get; set; }
        public HashSet<Type> ForceSpawnRoomTypes { get; } = new();
        public HashSet<Type> IncompatibleRoomTypes { get; } = new();
        public HashSet<Type> IncompatibleEnemyTypes { get; } = new();
        public UnityEvent<EnemyEgg> OnSpawnCallback { get; } = new();

        public float Chance { get; private set; }
        public int SpawnCount { get; private set; }
        public int RoomsBetweenSpawns { get; private set; }
        public bool HasSpawnedAtLeastOnce { get; private set; } = false;

        public EnemyEgg()
        {
            RoomManager.Singleton.OnRoomGenerated.AddListener(OnRoomGenerated);
        }

        /// <summary>
        /// Initializes runtime properties that depend on buildtime properties.
        /// </summary>
        public EnemyEgg Bake()
        {
            Chance = BaseChance;
            return this;
        }

        public bool IsForcedToSpawnInLatestRoom()
        {
            return ForceSpawnRoomTypes.Contains(LatestRoom().GetType());
        }

        public bool HasReachedMaxSpawnCount()
        {
            return MaxSpawnCount != 0 && SpawnCount >= MaxSpawnCount;
        }

        public bool IsInRoomSpawnRange()
        {
            return LatestRoomNumber() >= MinRoom && LatestRoomNumber() <= MaxRoom;
        }

        public bool IsCompatibleWithLatestRoom()
        {
            return !IncompatibleRoomTypes.Contains(LatestRoom().GetType());
        }

        public bool IsCompatibleWithOperativeEnemies()
        {
            foreach (var enemyType in IncompatibleEnemyTypes)
            {
                if (EnemyIsOperative(enemyType))
                    return false;
            }

            return true;
        }

        public bool AreAdditionalRequirementsFulfilled()
        {
            return AdditionalRequirements is null || AdditionalRequirements.Invoke();
        }

        public bool CanSpawn()
        {
            if (IsForcedToSpawnInLatestRoom())
                return true;

            if (IsForcedSpawnOnly)
                return false;

            if (!AreAdditionalRequirementsFulfilled())
                return false;

            if (IsNormalmodeExclusive && IsHardmodeEnabled())
                return false;

            if (IsHardmodeExclusive && !IsHardmodeEnabled())
                return false;

            if (HasReachedMaxSpawnCount())
                return false;

            if (!IsInRoomSpawnRange())
                return false;

            if (HasSpawnedAtLeastOnce && RoomsBetweenSpawns < MinRoomsBetweenSpawns)
                return false;

            if (!IsCompatibleWithLatestRoom())
                return false;

            if (!IsCompatibleWithOperativeEnemies())
                return false;

            return true;
        }

        public bool CanSpawnDebug(out string result)
        {
            if (IsForcedToSpawnInLatestRoom())
            {
                result = "Enemy is forced to spawn in the latest room.";
                return true;
            }

            if (IsForcedSpawnOnly)
            {
                result = "Enemy can only be forced to spawn.";
                return false;
            }

            if (!AreAdditionalRequirementsFulfilled())
            {
                result = "Enemy has additional requirements that are not fulfilled.";
                return false;
            }

            if (IsNormalmodeExclusive && IsHardmodeEnabled())
            {
                result = "Enemy is exclusive to Normal mode.";
                return false;
            }

            if (IsHardmodeExclusive && !IsHardmodeEnabled())
            {
                result = "Enemy is exclusive to Hard mode.";
                return false;
            }

            if (HasReachedMaxSpawnCount())
            {
                result = $"Enemy has reached the maximum amount of spawns. ({MaxSpawnCount})";
                return false;
            }

            if (!IsInRoomSpawnRange())
            {
                result = $"Enemy's room range is between {MinRoom} and {MaxRoom}.";
                return false;
            }

            if (HasSpawnedAtLeastOnce && RoomsBetweenSpawns < MinRoomsBetweenSpawns)
            {
                result = $"Enemy has spawned already in the last {MinRoomsBetweenSpawns} rooms.";
                return false;
            }

            if (!IsCompatibleWithLatestRoom())
            {
                result = "Enemy is incompatible with the latest room.";
                return false;
            }

            if (!IsCompatibleWithOperativeEnemies())
            {
                result = "Enemy is incompatible with at least one of the operative enemies.";
                return false;
            }

            result = "Enemy can spawn.";
            return true;
        }

        public bool RollChance()
        {
            return UnityEngine.Random.Range(0, 100f) <= Chance;
        }

        public bool RollChanceDebug()
        {
            var rng = UnityEngine.Random.Range(0, 100f);
            Debug.Log($"[{Id}] RNG: {rng:0.0} (must be <= {Chance})");
            return rng <= Chance;
        }

        public bool TrySpawn()
        {
            var canSpawn = CanSpawnDebug(out var result);

            Debug.Log($"[{Id}] {result}");

            if (!canSpawn)
                return false;

            if (IsForcedToSpawnInLatestRoom() || RoomsBetweenSpawns >= MaxRoomsBetweenSpawns || RollChanceDebug())
            {
                Spawn();
                return true;
            }

            return false;
        }

        public void Spawn()
        {
            SpawnCount++;
            Chance = BaseChance;
            RoomsBetweenSpawns = 0;
            HasSpawnedAtLeastOnce = true;

            Debug.Log($"[{Id}] Spawned!");

            if (Prefab != null)
                EnemyManager.Singleton.SpawnEnemy(Prefab);

            OnSpawnCallback?.Invoke(this);
        }

        public void SpawnAsDisguise()
        {
            Debug.Log($"[{Id}] Spawned as a disguise!");

            EnemyManager.Singleton.SpawnEnemy(Prefab, isDisguise: true);

            if (Prefab != null)
                EnemyManager.Singleton.SpawnEnemy(Prefab);
        }

        public void OnRoomGenerated(Room room)
        {
            if (IsInRoomSpawnRange())
                RoomsBetweenSpawns++;

            if (RoomsBetweenSpawns > MinRoomsBetweenSpawns)
                Chance += ChanceChangePerRoom;
        }

        public override string ToString()
        {
            return Id + " Enemy Egg";
        }
    }
}
