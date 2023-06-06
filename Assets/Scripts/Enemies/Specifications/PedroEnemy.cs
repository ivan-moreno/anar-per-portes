using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Pedro Enemy")]
    public class PedroEnemy : Enemy
    {
        public static bool EnemyIsActive { get; set; } = false;

        [SerializeField] private float runSpeed = 16f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource finishRunSound;
        [SerializeField] private SoundResource meetBouserSound;
        [SerializeField] private SoundResource laughAtBouserSound;
        [SerializeField] private SoundResource jumpscareSound;

        private AudioSource audioSource;
        private Animator animator;
        private Transform model;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool isOnBreak = false;
        private bool lineOfSightCheck = false;
        private bool isCatching = false;
        private bool metBouser = false;
        private int waypointsTraversed = 0;
        private int roomsTraversed = 0;
        private BouserEnemy bouserEnemy;

        private void Start()
        {
            EnemyIsActive = true;
            transform.position = RoomManager.Singleton.Rooms[0].transform.position;
            targetLocation = RoomManager.Singleton.Rooms[0].WaypointGroup.GetChild(0).position;
            audioSource = GetComponent<AudioSource>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
            bouserEnemy = FindObjectOfType<BouserEnemy>();
            SubtitleManager.Singleton.PushSubtitle(spawnSound.SubtitleText, Team.Hostile);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
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

            var nextPosition = Vector3.MoveTowards(transform.position, determinedTargetLocation, runSpeed * Time.deltaTime);

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
                        StartCoroutine(nameof(LaughAtBouserEnumerator));
                }
                else
                {
                    var dist = Vector3.Distance(transform.position, bouserRoom.BouserSpawnPoint.position);

                    if (dist <= 18f)
                        StartCoroutine(nameof(MeetBouserEnumerator));
                }
            }
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
            if (isCatching || isOnBreak)
                return;

            isCatching = true;
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            EnemyIsActive = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.7f);
            CatchManager.Singleton.CatchPlayer("PEDRO ENDING", "Parece que quiso pasar un mal rato, chico. Hehehehehe.");
        }

        private IEnumerator MeetBouserEnumerator()
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

        private IEnumerator LaughAtBouserEnumerator()
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
            EnemyIsActive = false;
            Destroy(gameObject);
        }
    }
}
