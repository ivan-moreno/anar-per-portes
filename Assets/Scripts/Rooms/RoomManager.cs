using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Room Manager")]
    public class RoomManager : MonoBehaviour
    {
        public Room LastLoadedRoom { get; private set; }
        [SerializeField] private GameObject startRoomPrefab;
        [SerializeField] private GameObject[] generalRoomPrefabs;
        private readonly List<Room> rooms = new(capacity: maxLoadedRooms);

        private const int maxLoadedRooms = 6;

        private void Start()
        {
            GenerateNextRoom(startRoomPrefab);
        }

        private void GenerateNextRoom(GameObject roomPrefab)
        {
            var instance = Instantiate(roomPrefab);

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
            rooms.Add(room);
            room.DoorOpened += OnDoorOpened;

            if (rooms.Count >= maxLoadedRooms)
                UnloadOldestRoom();
        }

        private void UnloadOldestRoom()
        {
            rooms[0].DoorOpened -= OnDoorOpened;
            Destroy(rooms[0].gameObject);
            rooms.RemoveAt(0);
            rooms[1].CloseDoor();
        }

        private void OnDoorOpened()
        {
            var rng = Random.Range(0, generalRoomPrefabs.Length);
            var randomRoom = generalRoomPrefabs[rng];
            GenerateNextRoom(randomRoom);
        }
    }
}
