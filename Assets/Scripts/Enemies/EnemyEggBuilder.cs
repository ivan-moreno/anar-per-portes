using System;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes.Enemies
{
    /// <summary>
    /// Builder class for <see cref="EnemyEgg"/>.
    /// </summary>
    public class EnemyEggBuilder
    {
        private readonly EnemyEgg egg;

        /// <summary>
        /// Generates an instance of a builder for a new <see cref="EnemyEgg"/>.
        /// </summary>
        public EnemyEggBuilder()
        {
            egg = new();
        }

        /// <summary>
        /// Bakes and gets the Enemy Egg with all the specified modifications applied to it.
        /// </summary>
        public EnemyEgg Build()
        {
            return egg.Bake();
        }

        /// <summary>
        /// Specifies a Prefab that will be supplied to the Enemy Manager and instantiated into the scene on spawn.
        /// </summary>
        public EnemyEggBuilder WithPrefab(GameObject prefab)
        {
            egg.Prefab = prefab;
            return this;
        }

        /// <summary>
        /// Specifies an ID for this Enemy Egg. Can be used as a tag of sorts.
        /// </summary>
        public EnemyEggBuilder WithId(string id)
        {
            egg.Id = id;
            return this;
        }

        /// <summary>
        /// Specifies the maximum amount of times that the Enemy can spawn in a single game. There is no limit by default.
        /// </summary>
        /// <param name="maxSpawnCount">A value of 0 represents no limit.</param>
        public EnemyEggBuilder WithMaxSpawnCount(int maxSpawnCount)
        {
            egg.MaxSpawnCount = maxSpawnCount;
            return this;
        }

        /// <summary>
        /// Specifies the minimum room number in which the Enemy may start spawning. Inclusive.
        /// </summary>
        public EnemyEggBuilder WithMinRoom(int minRoom)
        {
            egg.MinRoom = minRoom;
            return this;
        }

        /// <summary>
        /// Specifies the maximum room number after which the Enemy will not spawn anymore.
        /// </summary>
        public EnemyEggBuilder WithMaxRoom(int maxRoom)
        {
            egg.MaxRoom = maxRoom;
            return this;
        }

        /// <summary>
        /// Specifies the minimum amount of rooms that must be generated before the Enemy can be able to spawn again.
        /// </summary>
        public EnemyEggBuilder WithMinRoomsBetweenSpawns(int minRoomsBetweenSpawns)
        {
            egg.MinRoomsBetweenSpawns = minRoomsBetweenSpawns;
            return this;
        }

        /// <summary>
        /// Specifies the maximum amount of rooms that can be generated since the last spawn before the Enemy is forced to spawn.
        /// </summary>
        public EnemyEggBuilder WithMaxRoomsBetweenSpawns(int maxRoomsBetweenSpawns)
        {
            egg.MaxRoomsBetweenSpawns = maxRoomsBetweenSpawns;
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="Room"/> Types in which the Enemy will always be spawned.
        /// </summary>
        public EnemyEggBuilder ForceSpawnOnRoom<T>() where T : Room
        {
            egg.ForceSpawnRoomTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="Room"/> Types in which the Enemy may not spawn.
        /// </summary>
        public EnemyEggBuilder IncompatibleWithRoom<T>() where T : Room
        {
            egg.IncompatibleRoomTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the <see cref="Enemy"/> Types that the Enemy is incompatible with. The Enemy may not spawn when any of these are operative.
        /// </summary>
        public EnemyEggBuilder IncompatibleWithEnemy<T>() where T : Enemy
        {
            egg.IncompatibleEnemyTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Specifies the starting spawn chance.
        /// </summary>
        public EnemyEggBuilder WithBaseChance(float baseChance)
        {
            egg.BaseChance = baseChance;
            return this;
        }

        /// <summary>
        /// Specifies the minimum spawn chance. The spawn chance will never drop below the specified number.
        /// </summary>
        public EnemyEggBuilder WithMinChance(float minChance)
        {
            egg.MinChance = minChance;
            return this;
        }

        /// <summary>
        /// Specifies the increase or decrease to the spawn chance that is applied when a room is generated.
        /// </summary>
        public EnemyEggBuilder WithChanceChangePerRoom(float changeAmount)
        {
            egg.ChanceChangePerRoom = changeAmount;
            return this;
        }

        /// <summary>
        /// Specifies any additional checks that must be fulfilled in order for the Enemy to spawn.
        /// </summary>
        public EnemyEggBuilder WithAdditionalRequirements(Func<bool> additionalRequirements)
        {
            egg.AdditionalRequirements = additionalRequirements;
            return this;
        }

        /// <summary>
        /// Disables this Enemy's capability to spawn under normal circumstances.
        /// Can only be spawned by rooms that force it.
        /// </summary>
        public EnemyEggBuilder MarkAsForcedSpawnOnly()
        {
            egg.IsForcedSpawnOnly = true;
            return this;
        }

        /// <summary>
        /// Allows this Enemy to spawn as a Robloman disguise.
        /// </summary>
        public EnemyEggBuilder MarkAsDisguisable()
        {
            egg.IsDisguiseable = true;
            return this;
        }

        /// <summary>
        /// Disables this Enemy's ability to spawn on Hardmode.
        /// </summary>
        public EnemyEggBuilder MarkAsNormalmodeExclusive()
        {
            egg.IsNormalmodeExclusive = true;
            return this;
        }

        /// <summary>
        /// Disables this Enemy's ability to spawn on Normalmode.
        /// </summary>
        public EnemyEggBuilder MarkAsHardmodeExclusive()
        {
            egg.IsHardmodeExclusive = true;
            return this;
        }

        /// <summary>
        /// Sets a callback action that gets invoked whenever the Enemy spawns.
        /// </summary>
        public EnemyEggBuilder WithOnSpawnCallback(UnityAction<EnemyEgg> callback)
        {
            egg.OnSpawnCallback.AddListener(callback);
            return this;
        }
    }
}
