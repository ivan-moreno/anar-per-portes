using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Bouser Enemy")]
    public class BouserEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;
        public static UnityEvent<BouserEnemy> OnSpawn { get; } = new();
        public bool IsDefeated { get; private set; } = false;

        [Header("Stats")]
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float sprintSpeed = 14f;
        [SerializeField] private float sprintAtDistance = 15f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private float sightAngle = 45f;

        [Header("Audio")]
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource meetSkellSound;
        [SerializeField] private SoundResource[] meetSkellDialogs;
        [SerializeField] private SoundResource[] warningSounds;
        [SerializeField] private SoundResource[] loseSounds;
        [SerializeField] private SoundResource[] searchSounds;
        [SerializeField] private SoundResource[] findSounds;
        [SerializeField] private SoundResource[] tailSounds;
        [SerializeField] private SoundResource[] meetPedroSounds;

        private BouserRoom room;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool isCatching = false;
        private bool isMeetSkell = false;
        private bool isGrabbingTail = false;
        private float nextMoveTime;
        private float audioCooldown = 0f;
        private int targetReachesToTalk = 1;
        private float timeSinceReachedTarget = 0f;
        private SkellEnemy skellEnemy;

        private const float nextMoveMinTime = 0.2f;
        private const float nextMoveMaxTime = 2f;

        public void GrabTail()
        {
            if (isGrabbingTail)
                return;

            isGrabbingTail = true;
            animator.SetBool("IsWalking", false);
            animator.Play("TailGrab");
            audioSource.Stop();
            audioCooldown = 0f;
            Talk(tailSounds.RandomItem());
            room.OpenBouserDoor();
            GetComponent<BoxCollider>().enabled = false;

            //TODO: Launch animation
            IsDefeated = true;

            if (isMeetSkell)
                StartCoroutine(nameof(GrabTailWithSkellCoroutine));
        }

        public void MeetSkell(SkellEnemy skellEnemy)
        {
            if (isCatching || IsDefeated)
                return;

            this.skellEnemy = skellEnemy;
            StartCoroutine(nameof(MeetSkellCoroutine));
        }

        //TODO: Rename all XEnumerator to XCoroutine
        private IEnumerator MeetSkellCoroutine()
        {
            room.OpenBouserDoor();
            isMeetSkell = true;
            transform.LookAt(skellEnemy.transform);
            animator.SetBool("IsWalking", false);
            animator.Play("Idle");
            audioSource.PlayOneShot(meetSkellSound);
            yield return new WaitForSeconds(1f);

            SubtitleManager.Singleton.PushSubtitle(meetSkellDialogs[0]);
            yield return new WaitForSeconds(1.3f);

            SubtitleManager.Singleton.PushSubtitle(meetSkellDialogs[1]);
            yield return new WaitForSeconds(3.6f);

            SubtitleManager.Singleton.PushSubtitle(meetSkellDialogs[2]);
            yield return new WaitForSeconds(2.9f);

            SubtitleManager.Singleton.PushSubtitle(meetSkellDialogs[3]);
            yield return new WaitForSeconds(3.2f);
            animator.Play("Funkin");
        }

        private IEnumerator GrabTailWithSkellCoroutine()
        {
            yield return new WaitForSeconds(1f);
            PlayerController.Singleton.BlockAll();
            CatchManager.Singleton.CatchPlayer("BAD ENDING", "Has arruinado una batalla de rap legendaria.");
        }

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            room = RoomManager.Singleton.LastLoadedRoom as BouserRoom;
            room.OnUnloading.AddListener(Despawn);

            transform.SetPositionAndRotation(room.BouserSpawnPoint.position, room.BouserSpawnPoint.rotation);

            if (PedroEnemy.IsOperative)
            {
                var pedroPos = FindObjectOfType<PedroEnemy>().transform.position;
                var dist = Vector3.Distance(transform.position, pedroPos);

                if (dist <= 32f)
                    Talk(meetPedroSounds.RandomItem());

                else
                    Talk(warningSounds.RandomItem());
            }
            else
                Talk(warningSounds.RandomItem());

            nextMoveTime = Random.Range(nextMoveMinTime, nextMoveMaxTime);
            ChangeTargetLocationRandom();
            OnSpawn?.Invoke(this);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        //TODO: Try to not choose the same location twice in a row.
        private void ChangeTargetLocationRandom()
        {
            var rng = Random.Range(0, room.BouserWaypointGroup.childCount);
            targetLocation = room.BouserWaypointGroup.GetChild(rng).position;
            targetReachesToTalk--;

            if (targetReachesToTalk <= 0)
            {
                Talk(searchSounds.RandomItem());
                targetReachesToTalk = Random.Range(2, 4);
            }
        }

        private void Update()
        {
            if (audioCooldown > 0f)
                audioCooldown -= Time.deltaTime;

            if (isGrabbingTail || isMeetSkell)
                return;

            var distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Singleton.transform.position);
            var playerIsInSight = IsWithinAngle(transform, PlayerController.Singleton.transform);

            // Player camouflaged while chasing
            if (isChasing && PlayerController.Singleton.IsHidingAsStatue)
            {
                Talk(loseSounds.RandomItem());
            }

            var wasChasing = isChasing;

            isChasing = playerIsInSight
                && !PlayerController.Singleton.IsHidingAsStatue
                && !PlayerController.Singleton.IsCaught
                && room.PlayerIsInsideRoom;

            if (isChasing && distanceToPlayer <= catchRange)
                CatchPlayer();

            if (isCatching)
                return;

            // Found Player, started chasing
            if (!wasChasing && isChasing)
                Talk(findSounds.RandomItem());

            // Choose whether to go to the next map point or towards the Player.
            var determinedTargetLocation = isChasing ? PlayerController.Singleton.transform.position : targetLocation;

            var targetRunSpeed = runSpeed;

            if (isChasing && distanceToPlayer > sprintAtDistance)
            {
                targetRunSpeed = sprintSpeed;
                animator.speed = sprintSpeed / runSpeed;
            }
            else
            {
                animator.speed = 1f;
            }

            var nextPosition = Vector3.MoveTowards(transform.position, determinedTargetLocation, targetRunSpeed * Time.deltaTime);
            transform.position = nextPosition;

            reachedTarget = !isChasing && Vector3.Distance(transform.position, targetLocation) <= runSpeed * Time.deltaTime;
            animator.SetBool("IsWalking", !reachedTarget);

            if (reachedTarget)
            {
                timeSinceReachedTarget += Time.deltaTime;

                if (timeSinceReachedTarget >= nextMoveTime)
                {
                    timeSinceReachedTarget = 0f;
                    nextMoveTime = Random.Range(nextMoveMinTime, nextMoveMaxTime);
                    ChangeTargetLocationRandom();
                }

                return;
            }

            var direction = Vector3.Normalize(determinedTargetLocation - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 16f);
        }

        private bool IsWithinAngle(Transform source, Transform target)
        {
            var directionToTarget = target.position - source.position;
            var angle = Vector3.Angle(source.forward, directionToTarget);
            return angle <= sightAngle;
        }

        private void CatchPlayer()
        {
            if (isCatching || isGrabbingTail || isMeetSkell)
                return;

            if (PlayerController.Singleton.EquippedItemIs("Roblobolita"))
            {
                PlayerController.Singleton.ConsumeEquippedItem();
                BlurOverlayManager.Singleton.SetBlur(Color.white);
                BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 2f);
                GrabTail();
                return;
            }

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0.5f, 0f));
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.7f);
            CatchManager.Singleton.CatchPlayer("BOUSER ENDING", "a veces el amor te hace salir del caparazon jeje");
            audioSource.Play();
        }

        private void Talk(SoundResource soundResource)
        {
            if (audioCooldown > 0f || isMeetSkell)
                return;

            audioSource.PlayOneShot(soundResource);
            audioCooldown = soundResource.AudioClip.length;
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            IsOperative = false;
            Destroy(gameObject);
        }
    }
}
