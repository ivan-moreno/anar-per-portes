using System;
using System.Collections.Generic;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;
using System.Linq;

namespace AnarPerPortes.Rooms
{
    public class RoomSetEgg
    {
        public List<RoomEgg> Rooms { get; set; } = new();
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
        public HashSet<Type> IncompatibleRoomSetTypes { get; } = new();
        public HashSet<Type> IncompatibleEnemyTypes { get; } = new();
        public UnityEvent<RoomSetEgg> OnSpawnCallback { get; } = new();

        public float Chance { get; private set; }
        public int SpawnCount { get; private set; }
        public int RoomsBetweenSpawns { get; private set; }
        public bool HasSpawnedAtLeastOnce { get; private set; } = false;
        private readonly List<RoomEgg> roomsInPool = new();

        /// <summary>
        /// Initializes runtime properties that depend on buildtime properties.
        /// </summary>
        public RoomSetEgg Bake()
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

            if (!IsCompatibleWithLatestRoomSet())
                return false;

            if (!IsCompatibleWithOperativeEnemies())
                return false;

            return true;
        }

        public bool CanSpawnDebug(out string result)
        {
            if (IsNormalmodeExclusive && IsHardmodeEnabled())
            {
                result = "Room Set is exclusive to Normal mode.";
                return false;
            }

            if (IsHardmodeExclusive && !IsHardmodeEnabled())
            {
                result = "Room Set is exclusive to Hard mode.";
                return false;
            }

            if (IsForcedToSpawnInRoomNumber())
            {
                result = "Room Set is forced to spawn in the latest room number.";
                return true;
            }

            if (IsForcedSpawnOnly)
            {
                result = "Room Set can only be forced to spawn.";
                return false;
            }

            if (!AreAdditionalRequirementsFulfilled())
            {
                result = "Room Set has additional requirements that are not fulfilled.";
                return false;
            }

            if (HasReachedMaxSpawnCount())
            {
                result = $"Room Set has reached the maximum amount of spawns. ({MaxSpawnCount})";
                return false;
            }

            if (!IsInRoomSpawnRange())
            {
                result = $"Room Set's room range is between {MinRoom} and {MaxRoom}.";
                return false;
            }

            if (HasSpawnedAtLeastOnce && RoomsBetweenSpawns < MinRoomsBetweenSpawns)
            {
                result = $"Room Set has spawned already in the last {MinRoomsBetweenSpawns} rooms.";
                return false;
            }

            if (!IsCompatibleWithLatestRoom())
            {
                result = "Room Set is incompatible with the latest room.";
                return false;
            }

            if (!IsCompatibleWithLatestRoomSet())
            {
                result = "Room Set is incompatible with the latest room set.";
                return false;
            }

            if (!IsCompatibleWithOperativeEnemies())
            {
                result = "Room Set is incompatible with at least one of the operative enemies.";
                return false;
            }

            result = "Room Set can spawn.";
            return true;
        }

        public bool TrySpawn()
        {
            var canSpawn = CanSpawnDebug(out var result);

            ConsoleWriteLine($"[{Id}] {result}");

            if (!canSpawn)
                return false;

            if (IsForcedToSpawnInRoomNumber() || HasReachedMaxRoomsBetweenSpawnsDebug() || canSpawn)
            {
                var randomRoom = ChooseRandomRoom();

                if (randomRoom == null)
                    return false;

                Spawn(randomRoom);
                return true;
            }

            return false;
        }

        public void Spawn(RoomEgg room)
        {
            ConsoleWriteLine($"[{Id}] Spawned.");

            SpawnCount++;
            Chance = BaseChance;
            RoomsBetweenSpawns = 0;
            HasSpawnedAtLeastOnce = true;
            room.Spawn();
            OnSpawnCallback?.Invoke(this);
        }

        public override string ToString()
        {
            return Id + " Room Set Egg";
        }

        private RoomEgg ChooseRandomRoom()
        {
            RefillRoomPool();

            var maxRng = 0f;
            roomsInPool.ForEach(room => maxRng += room.Chance);

            var rng = UnityEngine.Random.Range(0f, maxRng);

            foreach (var room in roomsInPool)
            {
                if (rng <= room.Chance)
                    return room;

                rng -= room.Chance;
            }

            return null;
        }

        private void RefillRoomPool()
        {
            roomsInPool.Clear();
            roomsInPool.AddRange(Rooms.Where(room => room.CanSpawn()));
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
                ConsoleWriteLine($"[{Id}] Room Set has not spawned for {RoomsBetweenSpawns} rooms and will spawn whenever possible.");

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

        private bool IsCompatibleWithLatestRoomSet()
        {
            return LatestRoom().RoomSet == null || !IncompatibleRoomSetTypes.Contains(LatestRoom().RoomSet.GetType());
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
