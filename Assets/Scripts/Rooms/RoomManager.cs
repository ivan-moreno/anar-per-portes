using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Room Manager")]
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] generalRoomPrefabs;
        private readonly List<Room> rooms = new(capacity: maxLoadedRooms);
        private Room lastLoadedRoom;

        private const int maxLoadedRooms = 5;

        private void Start()
        {
            var rng = Random.Range(0, generalRoomPrefabs.Length);
            var randomRoom = generalRoomPrefabs[rng];
            GenerateNextRoom(randomRoom);
        }

        private void GenerateNextRoom(GameObject roomPrefab)
        {
            var instance = Instantiate(roomPrefab);

            if (lastLoadedRoom == null)
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            else
            {
                instance.transform.SetPositionAndRotation(
                    lastLoadedRoom.NextRoomGenerationPoint.position,
                    lastLoadedRoom.NextRoomGenerationPoint.rotation);
            }

            var hasRoomComponent = instance.TryGetComponent(out Room room);

            if (!hasRoomComponent)
            {
                Debug.LogError("Room Prefab hasn't been assigned a Room behaviour script.");
                return;
            }

            lastLoadedRoom = room;
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
        }

        private void OnDoorOpened()
        {
            var rng = Random.Range(0, generalRoomPrefabs.Length);
            var randomRoom = generalRoomPrefabs[rng];
            GenerateNextRoom(randomRoom);
        }
    }
}
