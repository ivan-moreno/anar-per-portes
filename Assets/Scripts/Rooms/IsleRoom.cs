using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    public class IsleRoom : Room
    {
        [HideInInspector] public UnityEvent OnIncorrectDoorOpened;
        public Transform YusufSpawnPoint => yusufSpawnPoint;
        [SerializeField] private Transform yusufSpawnPoint;
        [SerializeField] private Transform correctDoorPoint;
        [SerializeField] private Transform incorrectDoorPoint;
        [SerializeField] private GameObject doorPrefab;

        protected override void Start()
        {
            var correctDoor = Instantiate(doorPrefab, correctDoorPoint.position, correctDoorPoint.rotation);
            door = correctDoor.GetComponent<RoomDoor>();

            var incorrectDoor = Instantiate(doorPrefab, incorrectDoorPoint.position, incorrectDoorPoint.rotation);
            incorrectDoor.GetComponent<RoomDoor>().OnDoorOpened.AddListener(() => OnIncorrectDoorOpened?.Invoke());

            base.Start();
        }
    }
}
