using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Rooms
{
    public class RoomEgg
    {
        public RoomSetEgg RoomSet { get; set; }
        public GameObject Prefab { get; set; }
        public string Id { get; set; }
        public float BaseChance { get; set; } = 100f;
        public float ChanceChangePerRoom { get; set; }
        public float MinChance { get; set; } = 0f;
        public int MaxSpawnCount { get; set; }
        public int MinRoom { get; set; } = 1;
        public int MaxRoom { get; set; } = RoomManager.maxGeneratedRooms;
        public int MinRoomsBetweenSpawns { get; set; }
        public int MaxRoomsBetweenSpawns { get; set; }
        public bool IsForcedSpawnOnly { get; set; } = false;
        public bool IsNormalmodeExclusive { get; set; } = false;
        public bool IsHardmodeExclusive { get; set; } = false;
        public Func<bool> AdditionalRequirements { get; set; }
        public HashSet<int> ForceSpawnRoomNumbers { get; } = new();
        public HashSet<Type> IncompatibleRoomTypes { get; } = new();
        public HashSet<Type> IncompatibleEnemyTypes { get; } = new();
        public UnityEvent<RoomEgg> OnSpawnCallback { get; } = new();

        public float Chance { get; private set; }
        public int SpawnCount { get; private set; }
        public int RoomsBetweenSpawns { get; private set; }
        public bool HasSpawnedAtLeastOnce { get; private set; } = false;

        /// <summary>
        /// Initializes runtime properties that depend on buildtime properties.
        /// </summary>
        public RoomEgg Bake()
        {
            Chance = BaseChance;
            return this;
        }

        public bool CanSpawn()
        {
            if (IsNormalmodeExclusive && IsHardmodeEnabled())
                return false;

            if (IsHardmodeExclusive && !IsHardmodeEnabled())
                return false;

            if (IsForcedToSpawnInRoomNumber())
                return true;

            if (IsForcedSpawnOnly)
                return false;

            if (!AreAdditionalRequirementsFulfilled())
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
            if (IsNormalmodeExclusive && IsHardmodeEnabled())
            {
                result = "Room is exclusive to Normal mode.";
                return false;
            }

            if (IsHardmodeExclusive && !IsHardmodeEnabled())
            {
                result = "Room is exclusive to Hard mode.";
                return false;
            }

            if (IsForcedToSpawnInRoomNumber())
            {
                result = "Room is forced to spawn in the latest room number.";
                return true;
            }

            if (IsForcedSpawnOnly)
            {
                result = "Room can only be forced to spawn.";
                return false;
            }

            if (!AreAdditionalRequirementsFulfilled())
            {
                result = "Room has additional requirements that are not fulfilled.";
                return false;
            }

            if (HasReachedMaxSpawnCount())
            {
                result = $"Room has reached the maximum amount of spawns. ({MaxSpawnCount})";
                return false;
            }

            if (!IsInRoomSpawnRange())
            {
                result = $"Room's room range is between {MinRoom} and {MaxRoom}.";
                return false;
            }

            if (HasSpawnedAtLeastOnce && RoomsBetweenSpawns < MinRoomsBetweenSpawns)
            {
                result = $"Room has spawned already in the last {MinRoomsBetweenSpawns} rooms.";
                return false;
            }

            if (!IsCompatibleWithLatestRoom())
            {
                result = "Room is incompatible with the latest room.";
                return false;
            }

            if (!IsCompatibleWithOperativeEnemies())
            {
                result = "Room is incompatible with at least one of the operative enemies.";
                return false;
            }

            result = "Room can spawn.";
            return true;
        }

        public bool TrySpawn()
        {
            var canSpawn = CanSpawnDebug(out var result);

            ConsoleWriteLine($"[{Id}] {result}");

            if (!canSpawn)
                return false;

            if (IsForcedToSpawnInRoomNumber() || HasReachedMaxRoomsBetweenSpawnsDebug() || RollChanceDebug())
            {
                Spawn();
                return true;
            }

            return false;
        }

        public void Spawn()
        {
            ConsoleWriteLine($"[{Id}] Spawned.");

            SpawnCount++;
            Chance = BaseChance;
            RoomsBetweenSpawns = 0;
            HasSpawnedAtLeastOnce = true;

            if (Prefab != null)
            {
                var instance = RoomManager.Singleton.SpawnRoom(Prefab);

                if (instance == null)
                    return;

                instance.RoomSet = RoomSet;
            }

            OnSpawnCallback?.Invoke(this);
        }

        public override string ToString()
        {
            return Id + " Room Egg";
        }

        private bool RollChance()
        {
            return UnityEngine.Random.Range(0, 100f) <= Chance;
        }

        private bool RollChanceDebug()
        {
            var rng = UnityEngine.Random.Range(0, 100f);
            ConsoleWriteLine($"[{Id}] RNG: {rng:0.0} (must be <= {Chance})");
            return rng <= Chance;
        }

        private bool IsForcedToSpawnInRoomNumber()
        {
            return ForceSpawnRoomNumbers.Contains(LatestRoomNumber());
        }

        private bool HasReachedMaxRoomsBetweenSpawns()
        {
            return MaxRoomsBetweenSpawns > 0 && RoomsBetweenSpawns >= MaxRoomsBetweenSpawns;
        }

        private bool HasReachedMaxRoomsBetweenSpawnsDebug()
        {
            if (MaxRoomsBetweenSpawns <= 0)
                return false;

            var result = RoomsBetweenSpawns >= MaxRoomsBetweenSpawns;

            if (result)
                ConsoleWriteLine($"[{Id}] Room has not spawned for {RoomsBetweenSpawns} rooms and will spawn whenever possible.");

            return result;
        }

        private bool HasReachedMaxSpawnCount()
        {
            return MaxSpawnCount != 0 && SpawnCount >= MaxSpawnCount;
        }

        private bool IsInRoomSpawnRange()
        {
            return LatestRoomNumber() >= MinRoom && LatestRoomNumber() <= MaxRoom;
        }

        private bool IsCompatibleWithLatestRoom()
        {
            return !IncompatibleRoomTypes.Contains(LatestRoom().GetType());
        }

        private bool IsCompatibleWithOperativeEnemies()
        {
            foreach (var enemyType in IncompatibleEnemyTypes)
            {
                if (EnemyIsOperative(enemyType))
                    return false;
            }

            return true;
        }

        private bool AreAdditionalRequirementsFulfilled()
        {
            return AdditionalRequirements is null || AdditionalRequirements.Invoke();
        }
    }
}
