using AnarPerPortes.Rooms;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Pedro Enemy")]
    public class PedroEnemy : Enemy
    {
        public static UnityEvent<PedroEnemy> OnSpawn { get; } = new();
        public bool IsOnBreak { get; private set; } = false;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float runSpeed = 16f;
        [SerializeField][Min(0f)] private float chaseRange = 8f;
        [SerializeField][Min(0f)] private float graceRange = 6f;
        [SerializeField][Min(0f)] private float maxGraceTime = 4f;
        [SerializeField][Min(0f)] private float catchRange = 2f;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource spawnAheadSound;
        [SerializeField] private SoundResource finishRunSound;
        [SerializeField] private AudioClip catchMusic;
        [SerializeField] private SoundResource meetBouserSound;
        [SerializeField] private SoundResource laughAtBouserSound;
        [SerializeField] private SoundResource[] meetDaviloteSounds;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource[] loseSounds;

        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool isGrace = false;
        private bool lostPlayer = false;
        private bool lineOfSightCheck = false;
        private bool metBouser = false;
        private float graceTime;
        private int waypointsTraversed = 0;
        private int roomsTraversed = 0;
        private BouserEnemy bouserEnemy;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            transform.position = RoomManager.Singleton.Rooms[0].transform.position;
            targetLocation = RoomManager.Singleton.Rooms[0].WaypointGroup.GetChild(0).position;
            audioSource.Play(spawnSound);

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
            RoomManager.Singleton.OnRoomGenerated.AddListener(RoomGenerated);
            BouserBossEnemy.OnSpawn.AddListener((_) => Despawn());
            Specimen7Enemy.OnSpawn.AddListener((_) => Despawn());
            AssPancakesEnemy.OnSpawn.AddListener((_) => Despawn());

            if (EnemyIsOperative<BouserEnemy>())
                bouserEnemy = GetEnemyInstance<BouserEnemy>();
            else
                BouserEnemy.OnSpawn.AddListener((spawnedBouser) => bouserEnemy = spawnedBouser);

            OnSpawn?.Invoke(this);
        }

        private void RoomGenerated(Room room)
        {
            if (IsOnBreak)
                return;

            if (room.RoomSet != null && room.RoomSet.Id == "Toymaker")
                Despawn();

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

        //TODO: Update Waypoint AI to latest (Pom-Pom's)
        private void Update()
        {
            if (isInIntro)
                return;

            var distanceToPlayer = Vector3.Distance(transform.position, PlayerPosition());

            // Player camouflaged while chasing
            if (!lostPlayer && isChasing && isGrace && PlayerController.Singleton.IsCamouflaged)
            {
                StartCoroutine(nameof(LosePlayerCoroutine));
                return;
            }

            isChasing = !IsOnBreak
                && distanceToPlayer <= chaseRange
                && lineOfSightCheck
                && !PlayerController.Singleton.IsCamouflaged
                && !PlayerController.Singleton.IsCaught;

            if (!IsOnBreak && isChasing && distanceToPlayer <= catchRange)
                CatchPlayer();

            if (isCatching)
                return;

            // Choose whether to go to the next map point or towards the Player.
            var determinedTargetLocation = isChasing ? PlayerPosition() : targetLocation;

            var targetRunSpeed = runSpeed;

            if (isGrace)
            {
                targetRunSpeed = PlayerController.Singleton.WalkSpeed;
                graceTime += Time.deltaTime;
            }

            isGrace = graceTime < maxGraceTime
                && distanceToPlayer < graceRange
                && !PlayerController.Singleton.IsCamouflaged;

            var nextPosition = Vector3.MoveTowards(transform.position, determinedTargetLocation, targetRunSpeed * Time.deltaTime);

            if (!EnemyIsOperative<OshoskyEnemy>()
                && !EnemyIsOperative<SangotEnemy>()
                && !EnemyIsOperative<SheepyEnemy>()
                && !EnemyIsOperative<A90Enemy>())
                transform.position = nextPosition;

            reachedTarget = !isChasing && Vector3.Distance(transform.position, targetLocation) <= runSpeed * Time.deltaTime;

            if (reachedTarget)
            {
                if (IsOnBreak)
                {
                    runSpeed = 0f;
                    animator.Play("Idle");
                    model.rotation = RoomManager.Singleton.LatestRoom.PedroBreakPoint.rotation;
                    audioSource.Stop();
                    audioSource.PlayOneShot(finishRunSound);
                    enabled = false;
                    PlayerCollectTix(20, "Has evadido a Pedro");
                    return;
                }

                if (waypointsTraversed >= RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup.childCount - 1)
                {
                    RoomManager.Singleton.Rooms[roomsTraversed].Door.BreakThrough("Pedro");

                    roomsTraversed++;
                    waypointsTraversed = 0;
                }
                else
                    waypointsTraversed++;

                if (roomsTraversed >= RoomManager.Singleton.Rooms.Count)
                {
                    if (RoomManager.Singleton.LatestRoom is BouserRoom)
                    {
                        PlayerCollectTix(20, "Has evadido a Pedro");
                        Despawn();
                        return;
                    }

                    IsOnBreak = true;
                    targetLocation = RoomManager.Singleton.LatestRoom.PedroBreakPoint.position;
                    LatestRoom().OnUnloading.AddListener(Despawn);
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
            lineOfSightCheck = PlayerIsInLineOfSight(transform.position + Vector3.up);

            if (!metBouser && RoomManager.Singleton.LatestRoom is BouserRoom bouserRoom)
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

        private IEnumerator LosePlayerCoroutine()
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

        private void CatchPlayer()
        {
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            if (isCatching || IsOnBreak || runSpeed <= 0f)
                yield break;

            if (IsRoblomanDisguise)
            {
                RevealRoblomanDisguise();
                Despawn();
                yield break;
            }

            if (TryConsumePlayerImmunityItem())
            {
                Despawn();
                yield break;
            }

            isCatching = true;
            
            PlayerController.Singleton.BlockAll();
            audioSource.Stop();

            if (EnemyIsOperative<DaviloteEnemy>())
            {
                LatestRoom().OnUnloading.AddListener(Despawn);
                runSpeed = 0f;
                model.LookAt(PlayerPosition());
                animator.Play("Idle");
                var rngDaviloteSound = meetDaviloteSounds.RandomItem();
                audioSource.PlayOneShot(rngDaviloteSound);
                yield return new WaitForSeconds(rngDaviloteSound.AudioClip.length + 0.5f);

                PlayerController.Singleton.SetVisionTarget(GetEnemyInstance<DaviloteEnemy>().transform);
                yield return new WaitUntil(() => !EnemyIsOperative<DaviloteEnemy>());

                PlayerController.Singleton.UnblockAll();
                PlayerController.Singleton.ClearVisionTarget();
                yield return new WaitForSeconds(2f);

                audioSource.PlayOneShot(loseSounds.RandomItem());
                yield return new WaitForSeconds(1.5f);

                enabled = false;
                isCatching = false;
                yield break;
            }

            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.SetVisionTarget(transform);
            
            audioSource.PlayOneShot(jumpscareSound);
            yield return new WaitForSeconds(0.7f);

            audioSource.Stop();
            audioSource.PlayOneShot(catchMusic);
            CatchManager.Singleton.CatchPlayer("PEDRO ENDING", "Parece que quiso pasar un mal rato, chico. Hehehehehe.", broskyTip);
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
            var bouserRoom = RoomManager.Singleton.LatestRoom as BouserRoom;
            bouserRoom.WakeUpBouser();
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
    }
}
