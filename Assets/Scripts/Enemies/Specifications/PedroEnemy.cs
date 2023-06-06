using System.Collections;
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
        [SerializeField] private AudioClip meetBouserSound;
        [SerializeField] private string meetBouserSubtitles;
        [SerializeField] private AudioClip laughAtBouserSound;
        private AudioSource audioSource;
        private Animator animator;
        private Transform model;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool lineOfSightCheck = false;
        private bool isCatching = false;
        private bool metBouser = false;
        private int waypointsTraversed = 0;
        private int roomsTraversed = 0;
        private BouserEnemy bouserEnemy;

        private void Start()
        {
            transform.position = RoomManager.Singleton.Rooms[0].transform.position;
            targetLocation = RoomManager.Singleton.Rooms[0].WaypointGroup.GetChild(0).position;
            audioSource = GetComponent<AudioSource>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
            EnemyIsActive = true;
            SubtitleManager.Singleton.PushSubtitle("(pasos rápidos a la lejanía)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
            bouserEnemy = FindObjectOfType<BouserEnemy>();
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

            isChasing = distanceToPlayer <= chaseRange
                && lineOfSightCheck
                && !PlayerController.Singleton.IsHidingAsStatue
                && !PlayerController.Singleton.IsCaught;

            if (isChasing && distanceToPlayer <= catchRange)
            {
                CatchPlayer();
            }

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
                if (waypointsTraversed >= RoomManager.Singleton.Rooms[roomsTraversed].WaypointGroup.childCount - 1)
                {
                    roomsTraversed++;
                    waypointsTraversed = 0;
                }
                else
                    waypointsTraversed++;

                if (roomsTraversed >= RoomManager.Singleton.Rooms.Count)
                {
                    if (RoomManager.Singleton.LastLoadedRoom is not BouserRoom)
                        RoomManager.Singleton.OpenDoorAndGenerateNextRoomRandom();

                    EnemyIsActive = false;
                    Destroy(gameObject);
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
            lineOfSightCheck =
                Physics.Linecast(
                    start: transform.position + Vector3.up,
                    end: PlayerController.Singleton.transform.position + Vector3.up,
                    hitInfo: out var hit,
                    layerMask: LayerMask.GetMask("Default", "Player"),
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player");

            if (!metBouser && RoomManager.Singleton.LastLoadedRoom is BouserRoom bouserRoom)
            {
                if (bouserEnemy != null && bouserEnemy.IsDefeated)
                {
                    var dist = Vector3.Distance(transform.position, bouserEnemy.transform.position);

                    if (dist <= 8f)
                    {
                        StartCoroutine(nameof(LaughAtBouserEnumerator));
                    }
                }
                else
                {
                    var dist = Vector3.Distance(transform.position, bouserRoom.BouserSpawnPoint.position);

                    if (dist <= 18f)
                    {
                        StartCoroutine(nameof(MeetBouserEnumerator));
                    }
                }
            }
        }

        private void CatchPlayer()
        {
            if (isCatching)
                return;

            isCatching = true;
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(Pedro grita)", SubtitleCategory.Dialog, SubtitleSource.Hostile);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            EnemyIsActive = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.7f);
            CatchManager.Singleton.CatchPlayer("PEDRO ENDING", "Parece que quiso pasar un mal rato, chico. Hehehehehe.");
        }

        IEnumerator MeetBouserEnumerator()
        {
            metBouser = true;
            var originalRunSpeed = runSpeed;
            runSpeed = 0f;
            animator.Play("Idle");
            audioSource.Stop();
            audioSource.PlayOneShot(meetBouserSound);
            SubtitleManager.Singleton.PushSubtitle(meetBouserSubtitles, SubtitleCategory.Dialog, SubtitleSource.Common);
            yield return new WaitForSeconds(3f);
            var bouserRoom = RoomManager.Singleton.LastLoadedRoom as BouserRoom;
            bouserRoom.SpawnBouser();
            yield return new WaitForSeconds(1f);
            runSpeed = originalRunSpeed;
            animator.Play("Run");
            audioSource.Play();
        }

        IEnumerator LaughAtBouserEnumerator()
        {
            metBouser = true;
            var originalRunSpeed = runSpeed;
            runSpeed = 0f;
            animator.Play("Idle");
            audioSource.Stop();
            audioSource.PlayOneShot(laughAtBouserSound);
            model.LookAt(bouserEnemy.transform.position);
            SubtitleManager.Singleton.PushSubtitle("(Pedro se ríe)", SubtitleCategory.SoundEffect, SubtitleSource.Common);
            yield return new WaitForSeconds(2f);
            runSpeed = originalRunSpeed;
            animator.Play("Run");
            audioSource.Play();
        }
    }
}
