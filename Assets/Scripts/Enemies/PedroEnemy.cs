using UnityEngine;

namespace AnarPerPortes
{
    public class PedroEnemy : MonoBehaviour
    {
        public static bool EnemyIsActive { get; private set; } = false;
        [SerializeField] private float runSpeed = 16f;
        private Transform model;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private int roomsTraversed = 0;

        private void Start()
        {
            transform.position = Game.RoomManager.Rooms[0].transform.position;
            targetLocation = Game.RoomManager.Rooms[0].NextRoomGenerationPoint.position;
            model = transform.GetChild(0);
            EnemyIsActive = true;
            Game.SubtitleManager.PushSubtitle("(pasos rápidos a la lejanía)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
        }

        private void Update()
        {
            var nextPosition = Vector3.MoveTowards(transform.position, targetLocation, runSpeed * Time.deltaTime);
            transform.position = nextPosition;

            reachedTarget = Vector3.Distance(transform.position, targetLocation) <= runSpeed * Time.deltaTime;

            if (reachedTarget)
            {
                roomsTraversed++;

                if (roomsTraversed >= Game.RoomManager.Rooms.Count)
                {
                    EnemyIsActive = false;
                    Destroy(gameObject);
                    return;
                }

                targetLocation = Game.RoomManager.Rooms[roomsTraversed].NextRoomGenerationPoint.position;
            }

            var direction = Vector3.Normalize(targetLocation - transform.position);
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 16f);
        }
    }
}
