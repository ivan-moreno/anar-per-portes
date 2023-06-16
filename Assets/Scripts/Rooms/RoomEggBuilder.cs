using AnarPerPortes.Enemies;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes.Rooms
{
    /// <summary>
    /// Builder class for <see cref="RoomEgg"/>.
    /// </summary>
    public class RoomEggBuilder
    {
        private readonly RoomEgg egg;
        private const string prefabResourcesPath = "Prefabs/Rooms/";

        /// <summary>
        /// Generates an instance of a builder for a new <see cref="RoomEgg"/>.
        /// </summary>
        public RoomEggBuilder()
        {
            egg = new();
        }

        /// <summary>
        /// Bakes and gets the Room Egg with all the specified modifications applied to it.
        /// </summary>
        public RoomEgg Build()
        {
            if (egg.Prefab != null || string.IsNullOrEmpty(egg.Id))
                return egg.Bake();

            var targetResourcePath = string.Concat(prefabResourcesPath, egg.Id);
            egg.Prefab = Resources.Load<GameObject>(targetResourcePath);

            return egg.Bake();
        }

        /// <summary>
        /// Specifies the <see cref="RoomSetEgg"/> to which this Room will belong to.
        /// </summary>
        public RoomEggBuilder WithRoomSet(RoomSetEgg roomSet)
        {
            egg.RoomSet = roomSet;
            egg.RoomSet.Rooms.Add(egg);
            return this;
        }

        /// <summary>
        /// Specifies a Prefab that will be supplied to the Room Manager and instantiated into the scene on spawn.
        /// <br/>If no Prefab is specified, it will be looked up in the <c>Resources/Prefabs/Rooms</c> folder with the specified ID.
        /// </summary>
        public RoomEggBuilder WithPrefab(GameObject prefab)
        {
            egg.Prefab = prefab;
            return this;
        }

        /// <summary>
        /// Specifies an ID for this Room Egg. Can be used as a tag of sorts.
        /// </summary>
        public RoomEggBuilder WithId(string id)
        {
            egg.Id = id;
            return this;
        }

        /// <summary>
        /// Specifies the maximum amount of times that the Room can spawn in a single game. There is no limit by default.
        /// </summary>
        /// <param name="maxSpawnCount">A value of 0 represents no limit.</param>
        public RoomEggBuilder WithMaxSpawnCount(int maxSpawnCount)
        {
            egg.MaxSpawnCount = maxSpawnCount;
            return this;
        }

        /// <summary>
        /// Specifies the minimum room number in which the Room may start spawning. Inclusive.
        /// </summary>
        public RoomEggBuilder WithMinRoom(int minRoom)
        {
            egg.MinRoom = minRoom;
            return this;
        }

        /// <summary>
        /// Specifies the maximum room number after which the Room will not spawn anymore.
        /// </summary>
        public RoomEggBuilder WithMaxRoom(int maxRoom)
        {
            egg.MaxRoom = maxRoom;
            return this;
        }

        /// <summary>
        /// Specifies the minimum amount of rooms that must be generated before the Room can be able to spawn again.
        /// </summary>
        public RoomEggBuilder WithMinRoomsBetweenSpawns(int minRoomsBetweenSpawns)
        {
            egg.MinRoomsBetweenSpawns = minRoomsBetweenSpawns;
            return this;
        }

        /// <summary>
        /// Specifies the maximum amount of rooms that can be generated since the last spawn before the Room is forced to spawn.
        /// </summary>
        /// <param name="maxRoomsBetweenSpawns">A value of 0 represents no maximum.</param>
        public RoomEggBuilder WithMaxRoomsBetweenSpawns(int maxRoomsBetweenSpawns)
        {
            egg.MaxRoomsBetweenSpawns = maxRoomsBetweenSpawns;
            return this;
        }

        /// <summary>
        /// Specifies the room numbers in which the Room will always be spawned.
        /// </summary>
        public RoomEggBuilder ForceSpawnOnRoomNumber(int roomNumber)
        {
            egg.ForceSpawnRoomNumbers.Add(roomNumber);
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="Room"/> Types after which the Room may not spawn.
        /// </summary>
        public RoomEggBuilder IncompatibleWithRoom<T>() where T : Room
        {
            egg.IncompatibleRoomTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="Enemy"/> Types that the Room is incompatible with. The Room may not spawn when any of these are operative.
        /// </summary>
        public RoomEggBuilder IncompatibleWithEnemy<T>() where T : Enemy
        {
            egg.IncompatibleEnemyTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the starting spawn chance.
        /// </summary>
        public RoomEggBuilder WithBaseChance(float baseChance)
        {
            egg.BaseChance = baseChance;
            return this;
        }

        /// <summary>
        /// Specifies the minimum spawn chance. The spawn chance will never drop below the specified number.
        /// </summary>
        public RoomEggBuilder WithMinChance(float minChance)
        {
            egg.MinChance = minChance;
            return this;
        }

        /// <summary>
        /// Specifies the increase or decrease to the spawn chance that is applied when a room is generated.
        /// </summary>
        public RoomEggBuilder WithChanceChangePerRoom(float changeAmount)
        {
            egg.ChanceChangePerRoom = changeAmount;
            return this;
        }

        /// <summary>
        /// Specifies any additional checks that must be fulfilled in order for the Room to spawn.
        /// </summary>
        public RoomEggBuilder WithAdditionalRequirements(Func<bool> additionalRequirements)
        {
            egg.AdditionalRequirements = additionalRequirements;
            return this;
        }

        /// <summary>
        /// Disables this Room's capability to spawn under normal circumstances.
        /// Can only be spawned by rooms numbers that force it.
        /// </summary>
        public RoomEggBuilder MarkAsForcedSpawnOnly()
        {
            egg.IsForcedSpawnOnly = true;
            return this;
        }

        /// <summary>
        /// Disables this Room's ability to spawn on Hardmode.
        /// </summary>
        public RoomEggBuilder MarkAsNormalmodeExclusive()
        {
            egg.IsNormalmodeExclusive = true;
            return this;
        }

        /// <summary>
        /// Disables this Room's ability to spawn on Normalmode.
        /// </summary>
        public RoomEggBuilder MarkAsHardmodeExclusive()
        {
            egg.IsHardmodeExclusive = true;
            return this;
        }

        /// <summary>
        /// Sets a callback action that gets invoked whenever the Room spawns.
        /// </summary>
        public RoomEggBuilder WithOnSpawnCallback(UnityAction<RoomEgg> callback)
        {
            egg.OnSpawnCallback.AddListener(callback);
            return this;
        }
    }
}
