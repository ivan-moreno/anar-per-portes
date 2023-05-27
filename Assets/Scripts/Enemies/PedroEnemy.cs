using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Pedro Enemy")]
    public class PedroEnemy : Enemy
    {
        public static bool EnemyIsActive { get; private set; } = false;

        public override bool EnemyTipWasDisplayed
        {
            get => enemyTipWasDisplayed;
            set => enemyTipWasDisplayed = value;
        }

        private static bool enemyTipWasDisplayed = false;
        [SerializeField] private float runSpeed = 16f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private AudioClip jumpscareSound;
        private AudioSource audioSource;
        private Transform model;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool lineOfSightCheck = false;
        private int roomsTraversed = 0;

        private void Start()
        {
            transform.position = RoomManager.Singleton.Rooms[0].transform.position;
            targetLocation = RoomManager.Singleton.Rooms[0].NextRoomGenerationPoint.position;
            audioSource = GetComponent<AudioSource>();
            model = transform.GetChild(0);
            EnemyIsActive = true;
            SubtitleManager.Singleton.PushSubtitle("(pasos rápidos a la lejanía)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
        }

        private void Update()
        {
            var distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Singleton.transform.position);

            isChasing = distanceToPlayer <= chaseRange
                && lineOfSightCheck
                && !PlayerController.Singleton.IsHidingAsStatue
                && !PlayerController.Singleton.IsCaught;

            if (isChasing && distanceToPlayer <= catchRange)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(jumpscareSound);
                SubtitleManager.Singleton.PushSubtitle("(Pedro grita)", SubtitleCategory.Dialog, SubtitleSource.Hostile);
                CatchManager.Singleton.CatchPlayer("PEDRO ENDING", "Parece que quiso pasar un mal rato, chico. Hehehehehe.");
            }

            // Choose whether to go to the next map point or towards the Player.
            var determinedTargetLocation = isChasing ? PlayerController.Singleton.transform.position : targetLocation;

            var nextPosition = Vector3.MoveTowards(transform.position, determinedTargetLocation, runSpeed * Time.deltaTime);
            transform.position = nextPosition;

            reachedTarget = !isChasing && Vector3.Distance(transform.position, targetLocation) <= runSpeed * Time.deltaTime;

            if (reachedTarget)
            {
                roomsTraversed++;

                if (roomsTraversed >= RoomManager.Singleton.Rooms.Count)
                {
                    RoomManager.Singleton.OpenDoorAndGenerateNextRoomRandom();
                    EnemyIsActive = false;
                    Destroy(gameObject);
                    return;
                }

                targetLocation = RoomManager.Singleton.Rooms[roomsTraversed].NextRoomGenerationPoint.position;
            }

            var direction = Vector3.Normalize(determinedTargetLocation - transform.position);
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 16f);
        }

        private void FixedUpdate()
        {
            lineOfSightCheck =
                Physics.Linecast(
                    start: transform.position + Vector3.up,
                    end: PlayerController.Singleton.transform.position + Vector3.up,
                    hitInfo: out var hit,
                    layerMask: LayerMask.GetMask("Default", "Player"),
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player");
        }
    }
}
