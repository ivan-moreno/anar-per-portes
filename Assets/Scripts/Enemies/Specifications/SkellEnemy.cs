using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Skell Enemy")]
    public class SkellEnemy : Enemy
    {
        public static bool EnemyIsActive { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float sprintSpeed = 16f;
        [SerializeField] private float sprintAtDistance = 24f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private int doorsUntilDespawn = 5;

        [Header("Sound")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource endingMusic;
        [SerializeField] private SoundResource meetBouserMusic;

        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool isOnBreak = false;
        private bool lineOfSightCheck = false;
        private bool isCatching = false;
        private bool metBouser = false;
        private int waypointsTraversed = 0;
        private int roomsTraversed = 0;
        private int openedDoors = 0;
        private BouserEnemy bouserEnemy;

        private void Start()
        {
            EnemyIsActive = true;
            CacheComponents();

            transform.position = RoomManager.Singleton.Rooms[0].transform.position;
            targetLocation = RoomManager.Singleton.Rooms[0].WaypointGroup.GetChild(0).position;
            audioSource.Play(spawnSound);
            animator.Play("Run");
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
            RoomManager.Singleton.OnRoomGenerated.AddListener(RoomGenerated);

            if (BouserEnemy.EnemyIsActive)
                bouserEnemy = FindObjectOfType<BouserEnemy>();
            else
                BouserEnemy.OnSpawn.AddListener((spawnedBouser) => bouserEnemy = spawnedBouser);
        }

        private void RoomGenerated(Room room)
        {
            if (isOnBreak)
                return;

            if (RoomManager.Singleton.Rooms[0] == RoomManager.Singleton.Rooms[roomsTraversed])
                waypointsTraversed = 0;

            roomsTraversed--;

            if (roomsTraversed < 0)
                roomsTraversed = 0;

            var waypointGroup = RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup;

            targetLocation = waypointGroup.GetChild(waypointsTraversed).position;

            openedDoors++;

            if (openedDoors >= doorsUntilDespawn)
                Despawn();
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void Update()
        {
            if (metBouser)
                return;

            var distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Singleton.transform.position);

            isChasing = !isOnBreak
                && distanceToPlayer <= chaseRange
                && lineOfSightCheck
                && !PlayerController.Singleton.IsCaught;

            if (!isOnBreak && isChasing && distanceToPlayer <= catchRange)
                CatchPlayer();

            if (isCatching)
                return;

            // Choose whether to go to the next map point or towards the Player.
            var determinedTargetLocation = isChasing ? PlayerController.Singleton.transform.position : targetLocation;

            var targetRunSpeed = runSpeed;

            if (distanceToPlayer > sprintAtDistance)
            {
                targetRunSpeed = sprintSpeed;
                animator.speed = sprintSpeed / runSpeed;
            }
            else
            {
                animator.speed = 1f;
            }

            var nextPosition = Vector3.MoveTowards(transform.position, determinedTargetLocation, targetRunSpeed * Time.deltaTime);

            if (!A90Enemy.EnemyIsActive)
                transform.position = nextPosition;

            reachedTarget = !isChasing && Vector3.Distance(transform.position, targetLocation) <= runSpeed * Time.deltaTime;

            if (reachedTarget)
            {
                if (isOnBreak)
                {
                    runSpeed = 0f;
                    animator.Play("Idle");
                    model.rotation = RoomManager.Singleton.LastLoadedRoom.PedroBreakPoint.rotation;
                    animator.Play("Idle");
                    audioSource.Stop();
                    enabled = false;
                    return;
                }

                if (waypointsTraversed >= RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup.childCount - 1)
                {
                    roomsTraversed++;
                    waypointsTraversed = 0;
                }
                else
                    waypointsTraversed++;

                if (roomsTraversed >= RoomManager.Singleton.Rooms.Count)
                {
                    if (RoomManager.Singleton.LastLoadedRoom is BouserRoom)
                    {
                        Despawn();
                        return;
                    }

                    isOnBreak = true;
                    targetLocation = RoomManager.Singleton.LastLoadedRoom.PedroBreakPoint.position;
                    RoomManager.Singleton.LastLoadedRoom.OnUnloading.AddListener(Despawn);
                    return;
                }

                targetLocation = RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup.GetChild(waypointsTraversed).position;
            }

            if (runSpeed <= 0f)
                return;

            var direction = Vector3.Normalize(determinedTargetLocation - transform.position);

            if (direction.magnitude < Mathf.Epsilon)
                return;

            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 16f);
        }

        private void FixedUpdate()
        {
            lineOfSightCheck = PlayerIsInLineOfSight();

            if (metBouser || bouserEnemy == null || bouserEnemy.IsDefeated)
                return;

            var dist = Vector3.Distance(transform.position, bouserEnemy.transform.position);

            if (dist <= 14f)
                StartCoroutine(nameof(MeetBouserCoroutine));
        }

        private bool PlayerIsInLineOfSight()
        {
            return Physics.Linecast(
                    start: transform.position + Vector3.up,
                    end: PlayerController.Singleton.transform.position + Vector3.up,
                    hitInfo: out var hit,
                    layerMask: LayerMask.GetMask("Default", "Player"),
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        private void CatchPlayer()
        {
            if (isCatching || isOnBreak || metBouser)
                return;

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.Stop();
            //audioSource.PlayOneShot(jumpscareSound); consider
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            yield return new WaitForSeconds(1.4f);
            audioSource.PlayOneShot(jumpscareSound);
            yield return new WaitForSeconds(1.15f);
            audioSource.spatialBlend = 0f;
            audioSource.PlayOneShot(endingMusic);
            CatchManager.Singleton.CatchPlayer("SKELL ENDING", "ni modo");
        }

        private IEnumerator MeetBouserCoroutine()
        {
            EnemyIsActive = false;
            metBouser = true;
            bouserEnemy.MeetSkell(this);
            model.LookAt(bouserEnemy.transform);
            runSpeed = 0f;
            animator.Play("Idle");
            audioSource.Stop();
            yield return new WaitForSeconds(14f);
            audioSource.PlayOneShot(meetBouserMusic);
            //TODO: Make Bouser react, move both to singing location
        }

        private void Despawn()
        {
            EnemyIsActive = false;
            Destroy(gameObject);
        }
    }
}
