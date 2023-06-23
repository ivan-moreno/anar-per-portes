using AnarPerPortes.Enemies;
using AnarPerPortes.Rooms;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Room Manager")]
    public sealed class RoomManager : MonoBehaviour
    {
        public static RoomManager Singleton { get; private set; }
        public Room LatestRoom { get; private set; }
        public int LatestRoomNumber { get; private set; } = 0;
        public List<Room> Rooms { get; } = new(capacity: maxLoadedRooms);

        public UnityEvent<Room> OnRoomGenerated { get; } = new();
        public UnityEvent<Room> OnRoomUnloading { get; } = new();

        public const int maxGeneratedRooms = 9999;

        [Header("Components")]
        [SerializeField] private Transform roomsGroup;
        [SerializeField] private GameObject startRoomPrefab;

        private RoomSetEgg[] eggs;
        private Room lastGeneratedRoom;
        private const int maxLoadedRooms = 4;

        public Room SpawnRoom(GameObject roomPrefab)
        {
            if (LatestRoomNumber >= maxGeneratedRooms)
            {
                PushSubtitle("Has llegado a la sala 9999. El juego no va a seguir generando salas. ¡¿Felicidades...?!", 30f, Team.Friendly);
                return null;
            }

            var instance = Instantiate(roomPrefab, roomsGroup);
            instance.name = roomPrefab.name;

            if (LatestRoom == null)
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            else
            {
                instance.transform.SetPositionAndRotation(
                    LatestRoom.NextRoomSpawnPoint.position,
                    LatestRoom.NextRoomSpawnPoint.rotation);
            }

            instance.TryGetComponent(out Room room);

            LatestRoom = room;
            Rooms.Add(room);

            room.Initialize();
            room.OnDoorOpened.AddListener(OnDoorOpened);

            if (lastGeneratedRoom != null)
                lastGeneratedRoom.NextRoom = room;

            if (Rooms.Count >= maxLoadedRooms)
                UnloadOldestRoom();

            lastGeneratedRoom = room;
            OnRoomGenerated?.Invoke(room);

            return room;
        }

        public void SpawnRoomAndOpenDoor()
        {
            LatestRoom.OpenDoor();
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            SpawnRoom(startRoomPrefab);
            BuildRoomsAndRoomSets();
        }

        private void BuildRoomsAndRoomSets()
        {
            eggs = new RoomSetEgg[]
            {
                new RoomSetEggBuilder()
                .WithId("Bouser")
                .MarkAsForcedSpawnOnly()
                .ForceSpawnOnRoomNumber(50)
                .WithRoom(new RoomEggBuilder().WithId("BouserRoom0").Build())
                .Build(),

                new RoomSetEggBuilder()
                .WithId("GameMaker")
                .MarkAsHardmodeExclusive()
                .ForceSpawnOnRoomNumber(10)
                .WithBaseChance(20f)
                .WithMinRoom(10)
                .WithMaxSpawnCount(5)
                .WithMaxRoomsBetweenSpawns(1)
                .WithRoom(new RoomEggBuilder().WithId("GameMakerRoom0").Build())
                .Build(),

                //TODO: check for a cleaner way to force the first toom to be the trap 
                /*new RoomSetEggBuilder()
                .WithId("Toymaker")
                .WithBaseChance(100f) // 5
                .WithMinRoom(2) // 60
                .WithMaxSpawnCount(3)
                .WithMaxRoomsBetweenSpawns(1)
                .WithRoom(
                    new RoomEggBuilder()
                    .WithId("ToymakerTrapRoom")
                    .WithBaseChance(float.MaxValue * 0.5f)
                    .WithMaxSpawnCount(1)
                    .WithOnSpawnCallback((roomEgg) =>
                    {
                        AudioManager.Singleton.StopAmbiance();
                        roomEgg.AdditionalRequirements = () => false;
                    })
                    .Build())
                .WithRoom(
                    new RoomEggBuilder()
                    .WithId("ToymakerRoom0")
                    .WithMaxSpawnCount(1)
                    .Build())
                .WithRoom(
                    new RoomEggBuilder()
                    .WithId("ToymakerRoom1")
                    .WithMaxSpawnCount(1)
                    .Build())
                .Build(),*/

                new RoomSetEggBuilder()
                .WithId("Snowdin")
                .ForceSpawnOnRoomNumber(51)
                .WithBaseChance(20f)
                .WithMinRoomsBetweenSpawns(8)
                .WithMaxRoomsBetweenSpawns(40)
                .WithRoom(new RoomEggBuilder().WithId("SnowdinRoom0").Build())
                .Build(),

                new RoomSetEggBuilder()
                .WithId("Catalunya")
                .WithBaseChance(20f)
                .WithMaxSpawnCount(1)
                .WithAdditionalRequirements(() => CatalanBirdEnemy.IsCursed)
                .WithRoom(new RoomEggBuilder().WithId("CatalunyaRoom0").Build())
                .Build(),

                new RoomSetEggBuilder()
                .WithId("Specimen7")
                .WithBaseChance(10f)
                .WithMinRoom(60)
                .WithMinRoomsBetweenSpawns(10)
                .WithMaxRoomsBetweenSpawns(50)
                .WithRoom(new RoomEggBuilder().WithId("Specimen7Room0").Build())
                .WithRoom(new RoomEggBuilder().WithId("Specimen7Room1").Build())
                .WithRoom(new RoomEggBuilder().WithId("Specimen7Room2").Build())
                .Build(),

                new RoomSetEggBuilder()
                .WithId("Isle")
                .WithBaseChance(30f)
                .WithMinRoom(20)
                .WithMinRoomsBetweenSpawns(10)
                .WithMaxRoomsBetweenSpawns(30)
                .WithRoom(new RoomEggBuilder().WithId("IsleRoom0").Build())
                .Build(),

                new RoomSetEggBuilder()
                .WithId("Cameo")
                .WithBaseChance(20f) //20
                .WithMinRoom(20) // 20
                .WithMinRoomsBetweenSpawns(20)
                .WithRoom(new RoomEggBuilder().WithId("CameoRoom0").Build())
                .WithRoom(new RoomEggBuilder().WithId("CameoRoom1").Build())
                .WithRoom(new RoomEggBuilder().WithId("CameoRoom2").Build())
                .WithRoom(new RoomEggBuilder().WithId("CameoRoom3").Build())
                .Build(),

                new RoomSetEggBuilder()
                .WithId("General")
                .WithBaseChance(100f)
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom0").Build())
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom1").Build())
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom2").Build())
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom3").Build())
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom6").Build())
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom7").Build())
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom8").Build())
                .WithRoom(new RoomEggBuilder().WithId("GeneralRoom9").Build())
                .Build()
            };
        }

        private void UnloadOldestRoom()
        {
            StartCoroutine(nameof(UnloadOldestRoomCoroutine));
        }

        private IEnumerator UnloadOldestRoomCoroutine()
        {
            Rooms[1].CloseDoor();
            Rooms[1].DeactivateDoor();
            var oldestRoom = Rooms[0];
            Rooms.RemoveAt(0);
            OnRoomUnloading?.Invoke(oldestRoom);
            yield return new WaitForSeconds(0.5f);

            oldestRoom.Unload();
        }

        private void OnDoorOpened()
        {
            if (Input.GetKey(KeyCode.LeftShift))
                LatestRoomNumber = 49;
            else
                LatestRoomNumber++;

            PushSubtitle("Puerta " + LatestRoomNumber, 2f);
            ConsoleClear();
            TrySpawnEggs();
        }
        
        private void TrySpawnEggs()
        {
            foreach (var egg in eggs)
            {
                if (egg.TrySpawn())
                    break;
            }
        }
    }
}
