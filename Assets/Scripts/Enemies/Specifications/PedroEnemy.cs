using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Pedro Enemy")]
    public class PedroEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private float runSpeed = 16f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private float graceRange = 6f;
        [SerializeField] private float maxGraceTime = 4f;
        [SerializeField] private float catchRange = 2f;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource finishRunSound;
        [SerializeField] private SoundResource meetBouserSound;
        [SerializeField] private SoundResource laughAtBouserSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource[] loseSounds;

        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool isOnBreak = false;
        private bool isGrace = false;
        private bool lostPlayer = false;
        private bool lineOfSightCheck = false;
        private bool isCatching = false;
        private bool metBouser = false;
        private float graceTime;
        private int waypointsTraversed = 0;
        private int roomsTraversed = 0;
        private BouserEnemy bouserEnemy;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            transform.position = RoomManager.Singleton.Rooms[0].transform.position;
            targetLocation = RoomManager.Singleton.Rooms[0].WaypointGroup.GetChild(0).position;
            audioSource.Play(spawnSound);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
            RoomManager.Singleton.OnRoomGenerated.AddListener(RoomGenerated);

            if (BouserEnemy.IsOperative)
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
            var distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Singleton.transform.position);

            // Player camouflaged while chasing
            if (!lostPlayer && isChasing && isGrace && PlayerController.Singleton.IsHidingAsStatue)
            {
                StartCoroutine(nameof(LosePlayerCoroutine));
                return;
            }

            isChasing = !isOnBreak
                && distanceToPlayer <= chaseRange
                && lineOfSightCheck
                && !PlayerController.Singleton.IsHidingAsStatue
                && !PlayerController.Singleton.IsCaught;
            
            if (!isOnBreak && isChasing && distanceToPlayer <= catchRange)
                CatchPlayer();

            if (isCatching)
                return;

            // Choose whether to go to the next map point or towards the Player.
            var determinedTargetLocation = isChasing ? PlayerController.Singleton.transform.position : targetLocation;

            var targetRunSpeed = runSpeed;

            if (isGrace)
            {
                targetRunSpeed = PlayerController.Singleton.WalkSpeed;
                graceTime += Time.deltaTime;
            }

            isGrace = graceTime < maxGraceTime
                && distanceToPlayer < graceRange
                && !PlayerController.Singleton.IsHidingAsStatue;

            var nextPosition = Vector3.MoveTowards(transform.position, determinedTargetLocation, targetRunSpeed * Time.deltaTime);

            if (!SheepyEnemy.IsOperative && !A90Enemy.IsOperative)
                transform.position = nextPosition;

            reachedTarget = !isChasing && Vector3.Distance(transform.position, targetLocation) <= runSpeed * Time.deltaTime;

            if (reachedTarget)
            {
                if (isOnBreak)
                {
                    runSpeed = 0f;
                    animator.Play("Idle");
                    model.rotation = RoomManager.Singleton.LastLoadedRoom.PedroBreakPoint.rotation;
                    audioSource.Stop();
                    audioSource.PlayOneShot(finishRunSound);
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

            if (!metBouser && RoomManager.Singleton.LastLoadedRoom is BouserRoom bouserRoom)
            {
                if (bouserEnemy != null && bouserEnemy.IsDefeated)
                {
                    var dist = Vector3.Distance(transform.position, bouserEnemy.transform.position);

                    if (dist <= 8f)
                        StartCoroutine(nameof(LaughAtBouserCoroutine));
                }
                else
                {
                    var dist = Vector3.Distance(transform.position, bouserRoom.BouserSpawnPoint.position);

                    if (dist <= 18f)
                        StartCoroutine(nameof(MeetBouserCoroutine));
                }
            }
        }

        IEnumerator LosePlayerCoroutine()
        {
            if (lostPlayer)
                yield break;

            lostPlayer = true;
            graceTime = maxGraceTime + 1f;
            audioSource.Stop();
            audioSource.PlayOneShot(loseSounds.RandomItem());
            var originalRunSpeed = runSpeed;
            runSpeed = 0f;
            animator.Play("Idle");
            yield return new WaitForSeconds(1.5f);

            if (isCatching)
                yield break;

            audioSource.Play();
            runSpeed = originalRunSpeed;
            animator.Play("Run");
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
            if (isCatching || isOnBreak || runSpeed <= 0f)
                return;

            isCatching = true;
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.7f);
            CatchManager.Singleton.CatchPlayer("PEDRO ENDING", "Parece que quiso pasar un mal rato, chico. Hehehehehe.");
        }

        private IEnumerator MeetBouserCoroutine()
        {
            metBouser = true;
            var originalRunSpeed = runSpeed;
            runSpeed = 0f;
            animator.Play("Idle");
            audioSource.Stop();
            audioSource.PlayOneShot(meetBouserSound);
            yield return new WaitForSeconds(3f);
            var bouserRoom = RoomManager.Singleton.LastLoadedRoom as BouserRoom;
            bouserRoom.SpawnBouser();
            yield return new WaitForSeconds(1f);
            runSpeed = originalRunSpeed;
            animator.Play("Run");
            audioSource.Play();
        }

        private IEnumerator LaughAtBouserCoroutine()
        {
            metBouser = true;
            var originalRunSpeed = runSpeed;
            runSpeed = 0f;
            animator.Play("Idle");
            audioSource.Stop();
            audioSource.PlayOneShot(laughAtBouserSound);
            model.LookAt(bouserEnemy.transform.position);
            yield return new WaitForSeconds(2f);
            runSpeed = originalRunSpeed;
            animator.Play("Run");
            audioSource.Play();
        }

        private void Despawn()
        {
            IsOperative = false;
            Destroy(gameObject);
        }
    }
}
