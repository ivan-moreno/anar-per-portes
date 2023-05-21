using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Room Manager")]
    public class RoomManager : MonoBehaviour
    {
        public Room LastLoadedRoom { get; private set; }
        public List<Room> Rooms { get; } = new(capacity: maxLoadedRooms);
        public int LastOpenedRoomNumber { get; private set; } = 0;
        [HideInInspector] public UnityEvent<Room> OnRoomGenerated;
        [SerializeField] private Transform roomsGroup;
        [SerializeField] private GameObject startRoomPrefab;
        [SerializeField] private GameObject[] generalRoomPrefabs;
        private const int maxLoadedRooms = 7;

        private void Start()
        {
            GenerateNextRoom(startRoomPrefab);
        }

        public void OpenDoorAndGenerateNextRoomRandom()
        {
            LastLoadedRoom.OpenDoor();
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
            Destroy(Rooms[0].gameObject);
            Rooms.RemoveAt(0);
            Rooms[1].CloseDoor();
        }

        private void OnDoorOpened()
        {
            LastOpenedRoomNumber++;
            Game.SubtitleManager.PushSubtitle("Puerta nº: " + LastOpenedRoomNumber);
            GenerateNextRoomRandom();
        }
    }
}
