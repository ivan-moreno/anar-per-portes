using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Room Manager")]
    public sealed class RoomManager : MonoBehaviour
    {
        private class RoomSet
        {
            public float RoomSetChance { get; }
            public Func<RoomSet, bool> SpawnRequirements { get; set; }
            public Func<RoomSet, bool> RngRequirement { get; set; }
            public int RoomsWithoutSpawn { get; set; } = 1000;

            private readonly GameObject[] roomPrefabs;
            private readonly List<GameObject> roomPrefabsPool = new();
            private float minChanceRange;
            private float maxChanceRange;

            public RoomSet(float setChance, params GameObject[] roomPrefabs)
            {
                RoomSetChance = setChance;
                this.roomPrefabs = roomPrefabs;
                RefillRoomPool();
            }

            public bool HasSpawnRequirements()
            {
                return SpawnRequirements is null || SpawnRequirements.Invoke(this);
            }

            public bool HasRngRequirement()
            {
                return RngRequirement is null || RngRequirement.Invoke(this);
            }

            public void SpawnRandomSetRoom()
            {
                var room = roomPrefabsPool.RandomItem();
                RoomManager.Singleton.GenerateNextRoom(room);
                RoomsWithoutSpawn = 0;
                roomPrefabsPool.Remove(room);

                if (roomPrefabsPool.Count == 0)
                    RefillRoomPool();
            }

            public void DeclareChanceRange(float minChanceRange, float maxChanceRange)
            {
                this.minChanceRange = minChanceRange;
                this.maxChanceRange = maxChanceRange;
            }

            public bool IsInChanceRange(float chanceValue)
            {
                return chanceValue >= minChanceRange && chanceValue < maxChanceRange;
            }

            private void RefillRoomPool()
            {
                roomPrefabsPool.Clear();
                roomPrefabsPool.AddRange(roomPrefabs);
            }

            public override string ToString()
            {
                return roomPrefabs[0].name + " Room Set";
            }
        }

        public static RoomManager Singleton { get; private set; }
        public Room LastLoadedRoom { get; private set; }
        public int LastOpenedRoomNumber { get; private set; } = 0;
        public List<Room> Rooms { get; } = new(capacity: maxLoadedRooms);
        public UnityEvent<Room> OnRoomGenerated { get; } = new();
        public UnityEvent<Room> OnRoomUnloading { get; } = new();

        [SerializeField] private Transform roomsGroup;
        [SerializeField] private GameObject startRoomPrefab;
        [SerializeField] private GameObject[] generalRoomPrefabs;
        [SerializeField] private GameObject[] snowdinRoomPrefabs;
        [SerializeField] private GameObject[] isleRoomPrefabs;
        [SerializeField] private GameObject[] bouserRoomPrefabs;
        [SerializeField] private GameObject[] s7RoomPrefabs;
        [SerializeField] private GameObject[] cameoRoomPrefabs;

        private RoomSet generalRoomSet;
        private RoomSet snowdinRoomSet;
        private RoomSet isleRoomSet;
        private RoomSet bouserRoomSet;
        private RoomSet s7RoomSet;
        private RoomSet cameoRoomSet;
        private readonly List<RoomSet> allRoomSets = new();

        private Room lastGeneratedRoom;
        private const int maxLoadedRooms = 4;

        public void OpenDoorAndGenerateNextRoomRandom()
        {
            LastLoadedRoom.OpenDoor();
        }

        public void GenerateNextRoom(GameObject roomPrefab)
        {
            var instance = Instantiate(roomPrefab, roomsGroup);
            instance.name = roomPrefab.name;

            if (LastLoadedRoom == null)
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            else
            {
                instance.transform.SetPositionAndRotation(
                    LastLoadedRoom.NextRoomGenerationPoint.position,
                    LastLoadedRoom.NextRoomGenerationPoint.rotation);
            }

            var hasRoomComponent = instance.TryGetComponent(out Room room);

            if (!hasRoomComponent)
            {
                Debug.LogError("Room Prefab hasn't been assigned a Room behaviour script.");
                return;
            }

            LastLoadedRoom = room;
            Rooms.Add(room);
            room.Initialize();
            room.OnDoorOpened.AddListener(OnDoorOpened);

            if (lastGeneratedRoom != null)
                lastGeneratedRoom.NextRoom = room;

            if (Rooms.Count >= maxLoadedRooms)
                UnloadOldestRoom();

            lastGeneratedRoom = room;
            OnRoomGenerated?.Invoke(room);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            GenerateNextRoom(startRoomPrefab);

            AddRoomSets(
                generalRoomSet = new(100f, generalRoomPrefabs),
                snowdinRoomSet = new(20f, snowdinRoomPrefabs)
                {
                    SpawnRequirements = (roomSet) => !PlayerController.Singleton.HasItem("Roblobolita")
                },
                isleRoomSet = new(100f, isleRoomPrefabs)
                {
                    SpawnRequirements = (roomSet) => roomSet.RoomsWithoutSpawn >= 10
                },
                bouserRoomSet = new(100f, bouserRoomPrefabs)
                {
                    SpawnRequirements = (roomSet) => roomSet.RoomsWithoutSpawn >= 16
                },
                s7RoomSet = new(100f, s7RoomPrefabs)
                {
                    SpawnRequirements = (roomSet) =>
                    RoomManager.Singleton.LastOpenedRoomNumber > 50
                    && roomSet.RoomsWithoutSpawn >= 8
                },
                cameoRoomSet = new(100f, cameoRoomPrefabs)
                {
                    SpawnRequirements = (roomSet) =>
                    RoomManager.Singleton.LastOpenedRoomNumber > 10
                    && roomSet.RoomsWithoutSpawn >= 8
                });
        }

        private void AddRoomSets(params RoomSet[] roomSets)
        {
            var totalRoomSetChance = 0f;

            foreach (var roomSet in roomSets)
            {
                allRoomSets.Add(roomSet);
                roomSet.DeclareChanceRange(totalRoomSetChance, totalRoomSetChance + roomSet.RoomSetChance);
                totalRoomSetChance += roomSet.RoomSetChance;
            }
        }

        private void GenerateNextRoomRandom()
        {
            var candidateRoomSets = new List<RoomSet>();
            var candidateTotalChance = 0f;

            foreach (var roomSet in allRoomSets)
            {
                if (roomSet.HasSpawnRequirements() && roomSet.HasRngRequirement())
                {
                    candidateRoomSets.Add(roomSet);
                    candidateTotalChance += roomSet.RoomSetChance;
                }

                roomSet.RoomsWithoutSpawn++;
            }

            var spawnedRoom = false;
            var rng = UnityEngine.Random.Range(0f, candidateTotalChance);

            foreach (var roomSet in candidateRoomSets)
            {
                if (!spawnedRoom && roomSet.IsInChanceRange(rng))
                {
                    roomSet.SpawnRandomSetRoom();
                    spawnedRoom = true;
                }
            }

            if (!spawnedRoom)
                generalRoomSet.SpawnRandomSetRoom();
        }

        private void UnloadOldestRoom()
        {
            StartCoroutine(nameof(UnloadOldestRoomEnumerator));
        }

        private IEnumerator UnloadOldestRoomEnumerator()
        {
            Rooms[1].CloseDoor();
            var oldestRoom = Rooms[0];
            Rooms.RemoveAt(0);
            OnRoomUnloading?.Invoke(oldestRoom);
            yield return new WaitForSeconds(1f);
            oldestRoom.Unload();
        }

        private void OnDoorOpened()
        {
            LastOpenedRoomNumber++;
            SubtitleManager.Singleton.PushSubtitle("Puerta " + LastOpenedRoomNumber, 2f);
            GenerateNextRoomRandom();
        }
    }
}
