using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Room Manager")]
    public sealed class RoomManager : MonoBehaviour
    {
        public static RoomManager Singleton { get; private set; }
        public Room LastLoadedRoom { get; private set; }
        public int LastOpenedRoomNumber { get; private set; } = 0;
        public List<Room> Rooms { get; } = new(capacity: maxLoadedRooms);
        [HideInInspector] public UnityEvent<Room> OnRoomGenerated;
        [SerializeField] private Transform roomsGroup;
        [SerializeField] private GameObject startRoomPrefab;
        [SerializeField] private GameObject[] generalRoomPrefabs;
        private const int maxLoadedRooms = 5;

        public void OpenDoorAndGenerateNextRoomRandom()
        {
            LastLoadedRoom.OpenDoor();
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            GenerateNextRoom(startRoomPrefab);
        }

        private void GenerateNextRoomRandom()
        {
            var rng = Random.Range(0, generalRoomPrefabs.Length);
            var randomRoom = generalRoomPrefabs[rng];
            GenerateNextRoom(randomRoom);
        }

        private void GenerateNextRoom(GameObject roomPrefab)
        {
            var instance = Instantiate(roomPrefab, roomsGroup);

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
            room.OnDoorOpened.AddListener(OnDoorOpened);

            if (Rooms.Count >= maxLoadedRooms)
                UnloadOldestRoom();

            OnRoomGenerated?.Invoke(room);
        }

        private void UnloadOldestRoom()
        {
            Rooms[0].Unload();
            Rooms.RemoveAt(0);
            Rooms[1].CloseDoor();
        }

        private void OnDoorOpened()
        {
            LastOpenedRoomNumber++;
            SubtitleManager.Singleton.PushSubtitle("Puerta nº: " + LastOpenedRoomNumber);
            GenerateNextRoomRandom();
        }
    }
}
