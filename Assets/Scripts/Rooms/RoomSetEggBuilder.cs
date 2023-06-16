using AnarPerPortes.Enemies;
using System;
using UnityEngine.Events;

namespace AnarPerPortes.Rooms
{
    /// <summary>
    /// Builder class for <see cref="RoomSetEgg"/>.
    /// </summary>
    public class RoomSetEggBuilder
    {
        private readonly RoomSetEgg egg;

        /// <summary>
        /// Generates an instance of a builder for a new <see cref="RoomSetEgg"/>.
        /// </summary>
        public RoomSetEggBuilder()
        {
            egg = new();
        }

        /// <summary>
        /// Bakes and gets the Room Set Egg with all the specified modifications applied to it.
        /// </summary>
        public RoomSetEgg Build()
        {
            return egg.Bake();
        }

        /// <summary>
        /// Specifies a <see cref="RoomEgg"/> to add to this Room Set Egg.
        /// </summary>
        public RoomSetEggBuilder WithRoom(RoomEgg room)
        {
            egg.Rooms.Add(room);
            return this;
        }

        /// <summary>
        /// Specifies an ID for this Room Set Egg. Can be used as a tag of sorts.
        /// </summary>
        public RoomSetEggBuilder WithId(string id)
        {
            egg.Id = id;
            return this;
        }

        /// <summary>
        /// Specifies the maximum amount of times that the Room Set can spawn in a single game. There is no limit by default.
        /// </summary>
        /// <param name="maxSpawnCount">A value of 0 represents no limit.</param>
        public RoomSetEggBuilder WithMaxSpawnCount(int maxSpawnCount)
        {
            egg.MaxSpawnCount = maxSpawnCount;
            return this;
        }

        /// <summary>
        /// Specifies the minimum room number in which the Room Set may start spawning. Inclusive.
        /// </summary>
        public RoomSetEggBuilder WithMinRoom(int minRoom)
        {
            egg.MinRoom = minRoom;
            return this;
        }

        /// <summary>
        /// Specifies the maximum room number after which the Room Set will not spawn anymore.
        /// </summary>
        public RoomSetEggBuilder WithMaxRoom(int maxRoom)
        {
            egg.MaxRoom = maxRoom;
            return this;
        }

        /// <summary>
        /// Specifies the minimum amount of rooms that must be generated before the Room Set can be able to spawn again.
        /// </summary>
        public RoomSetEggBuilder WithMinRoomsBetweenSpawns(int minRoomsBetweenSpawns)
        {
            egg.MinRoomsBetweenSpawns = minRoomsBetweenSpawns;
            return this;
        }

        /// <summary>
        /// Specifies the maximum amount of rooms that can be generated since the last spawn before the Room Set is forced to spawn.
        /// </summary>
        /// <param name="maxRoomsBetweenSpawns">A value of 0 represents no maximum.</param>
        public RoomSetEggBuilder WithMaxRoomsBetweenSpawns(int maxRoomsBetweenSpawns)
        {
            egg.MaxRoomsBetweenSpawns = maxRoomsBetweenSpawns;
            return this;
        }

        /// <summary>
        /// Specifies the room numbers in which the Room Set will always be spawned.
        /// </summary>
        public RoomSetEggBuilder ForceSpawnOnRoomNumber(int roomNumber)
        {
            egg.ForceSpawnRoomNumbers.Add(roomNumber);
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="Room"/> Types after which the Room Set may not spawn.
        /// </summary>
        public RoomSetEggBuilder IncompatibleWithRoom<T>() where T : Room
        {
            egg.IncompatibleRoomTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="RoomSetEgg"/> Types after which the Room Set may not spawn.
        /// </summary>
        public RoomSetEggBuilder IncompatibleWithRoomSet<T>() where T : Room
        {
            egg.IncompatibleRoomSetTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="Enemy"/> Types that the Room Set is incompatible with. The Room Set may not spawn when any of these are operative.
        /// </summary>
        public RoomSetEggBuilder IncompatibleWithEnemy<T>() where T : Enemy
        {
            egg.IncompatibleEnemyTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the starting spawn chance.
        /// </summary>
        public RoomSetEggBuilder WithBaseChance(float baseChance)
        {
            egg.BaseChance = baseChance;
            return this;
        }

        /// <summary>
        /// Specifies the minimum spawn chance. The spawn chance will never drop below the specified number.
        /// </summary>
        public RoomSetEggBuilder WithMinChance(float minChance)
        {
            egg.MinChance = minChance;
            return this;
        }

        /// <summary>
        /// Specifies the increase or decrease to the spawn chance that is applied when a room is generated.
        /// </summary>
        public RoomSetEggBuilder WithChanceChangePerRoom(float changeAmount)
        {
            egg.ChanceChangePerRoom = changeAmount;
            return this;
        }

        /// <summary>
        /// Specifies any additional checks that must be fulfilled in order for the Room Set to spawn.
        /// </summary>
        public RoomSetEggBuilder WithAdditionalRequirements(Func<bool> additionalRequirements)
        {
            egg.AdditionalRequirements = additionalRequirements;
            return this;
        }

        /// <summary>
        /// Disables this Room Set's capability to spawn under normal circumstances.
        /// Can only be spawned by rooms numbers that force it.
        /// </summary>
        public RoomSetEggBuilder MarkAsForcedSpawnOnly()
        {
            egg.IsForcedSpawnOnly = true;
            return this;
        }

        /// <summary>
        /// Disables this Room Set's ability to spawn on Hardmode.
        /// </summary>
        public RoomSetEggBuilder MarkAsNormalmodeExclusive()
        {
            egg.IsNormalmodeExclusive = true;
            return this;
        }

        /// <summary>
        /// Disables this Room Set's ability to spawn on Normalmode.
        /// </summary>
        public RoomSetEggBuilder MarkAsHardmodeExclusive()
        {
            egg.IsHardmodeExclusive = true;
            return this;
        }

        /// <summary>
        /// Sets a callback action that gets invoked whenever the Room Set spawns.
        /// </summary>
        public RoomSetEggBuilder WithOnSpawnCallback(UnityAction<RoomSetEgg> callback)
        {
            egg.OnSpawnCallback.AddListener(callback);
            return this;
        }
    }
}
