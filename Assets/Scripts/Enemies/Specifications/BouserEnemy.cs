using AnarPerPortes.Rooms;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Bouser Enemy")]
    public class BouserEnemy : Enemy
    {
        public static UnityEvent<BouserEnemy> OnSpawn { get; } = new();
        public bool IsFriendly { get; private set; } = false;
        public bool IsDefeated { get; private set; } = false;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float runSpeed = 8f;
        [SerializeField][Min(0f)] private float sprintSpeed = 14f;
        [SerializeField][Min(0f)] private float sprintAtDistance = 15f;
        [SerializeField][Min(0f)] private float catchRange = 2f;
        [SerializeField][Min(0f)] private float sightAngle = 45f;

        [Header("Hardmode Stats")]
        [SerializeField][Min(0f)] private float runSpeedHardmode = 12f;
        [SerializeField][Min(0f)] private float sightAngleHardmode = 90f;

        [Header("Audio")]
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource meetDanylopezSound;
        [SerializeField] private SoundResource meetSkellSound;
        [SerializeField] private SoundResource[] meetSkellDialogs;
        [SerializeField] private SoundResource[] meetAssPancakesDialogs;
        [SerializeField] private SoundResource[] warningSounds;
        [SerializeField] private SoundResource[] loseSounds;
        [SerializeField] private SoundResource[] searchSounds;
        [SerializeField] private SoundResource[] findSounds;
        [SerializeField] private SoundResource[] tailSounds;
        [SerializeField] private SoundResource[] meetPedroSounds;

        private BouserRoom room;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isAwake = false;
        private bool isChasing = false;
        private bool isMeetSkell = false;
        private bool isGrabbingTail = false;
        private float nextMoveTime;
        private float audioCooldown = 0f;
        private int targetReachesToTalk = 1;
        private float timeSinceReachedTarget = 0f;
        private SkellBetaEnemy skellEnemy;

        private const float nextMoveMinTime = 0.2f;
        private const float nextMoveMaxTime = 2f;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            //TODO: Consider making disguisable and using a different AI for that case
            room = LatestRoom() as BouserRoom;

            if (room == null)
            {
                Despawn();
                return;
            }

            transform.SetPositionAndRotation(room.BouserSpawnPoint.position, room.BouserSpawnPoint.rotation);

            LatestRoom().OnUnloading.AddListener(Despawn);

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            OnSpawn?.Invoke(this);
        }

        public void WakeUp()
        {
            isAwake = true;

            if (EnemyIsOperative<AssPancakesEnemy>())
            {
                StartCoroutine(nameof(MeetAssPancakesCoroutine));
                return;
            }

            if (DanylopezEnemy.HasAppearedInThisSession)
            {
                StartCoroutine(nameof(MeetDanylopezCoroutine));
                nextMoveTime = Random.Range(nextMoveMinTime, nextMoveMaxTime);
                ChangeTargetLocationRandom();
                return;
            }

            if (EnemyIsOperative<PedroEnemy>())
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
        }

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
            room.OpenBouserDoorAsDefeated();
            GetComponent<BoxCollider>().enabled = false;

            //TODO: Launch animation
            IsDefeated = true;

            if (isMeetSkell)
                StartCoroutine(nameof(GrabTailWithSkellCoroutine));
            else
                PlayerCollectTix(50, "Has derrotado a Bouser");
        }

        IEnumerator MeetAssPancakesCoroutine()
        {
            room.OpenBouserDoorAsDefeated();
            IsFriendly = true;
            audioSource.PlayOneShot(meetAssPancakesDialogs[0]);
            yield return new WaitForSeconds(meetAssPancakesDialogs[0].AudioClip.length + 4f);

            audioSource.PlayOneShot(meetAssPancakesDialogs[1]);
            yield return new WaitForSeconds(meetAssPancakesDialogs[1].AudioClip.length + 2f);

            audioSource.PlayOneShot(meetAssPancakesDialogs[2]);
            yield return new WaitForSeconds(meetAssPancakesDialogs[2].AudioClip.length + 0.4f);
        }

        private IEnumerator MeetDanylopezCoroutine()
        {
            Talk(meetDanylopezSound);
            yield return new WaitForSeconds(meetDanylopezSound.AudioClip.length + 0.5f);
            DanylopezEnemy.Singleton.Spawn();
            audioCooldown = 4.5f;
        }

        public void MeetSkell(SkellBetaEnemy skellEnemy)
        {
            if (isCatching || IsDefeated)
                return;

            this.skellEnemy = skellEnemy;
            StartCoroutine(nameof(MeetSkellCoroutine));
        }

        //TODO: Rename all XEnumerator to XCoroutine
        private IEnumerator MeetSkellCoroutine()
        {
            room.OpenBouserDoorAsDefeated();
            isMeetSkell = true;
            transform.LookAt(skellEnemy.transform);
            animator.SetBool("IsWalking", false);
            animator.Play("Idle");
            audioSource.PlayOneShot(meetSkellSound);
            yield return new WaitForSeconds(1f);

            PushSubtitle(meetSkellDialogs[0]);
            yield return new WaitForSeconds(1.3f);

            PushSubtitle(meetSkellDialogs[1]);
            yield return new WaitForSeconds(3.6f);

            PushSubtitle(meetSkellDialogs[2]);
            yield return new WaitForSeconds(2.9f);

            PushSubtitle(meetSkellDialogs[3]);
            yield return new WaitForSeconds(3.2f);
            animator.Play("Funkin");
        }

        private IEnumerator GrabTailWithSkellCoroutine()
        {
            yield return new WaitForSeconds(1f);
            PlayerController.Singleton.BlockAll();
            CatchManager.Singleton.CatchPlayer("BAD ENDING", "Has arruinado una batalla de rap legendaria.");
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
            if (isInIntro || !isAwake || IsFriendly)
                return;

            if (audioCooldown > 0f)
                audioCooldown -= Time.deltaTime;

            if (isGrabbingTail || isMeetSkell)
                return;

            var distanceToPlayer = Vector3.Distance(transform.position, PlayerPosition());
            var playerIsInSight = IsWithinAngle(transform, PlayerController.Singleton.transform);

            // Player camouflaged while chasing
            if (isChasing && PlayerController.Singleton.IsCamouflaged)
                Talk(loseSounds.RandomItem());

            var wasChasing = isChasing;

            isChasing = playerIsInSight
                && !PlayerController.Singleton.IsCamouflaged
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
            var determinedTargetLocation = isChasing ? PlayerPosition() : targetLocation;

            var targetRunSpeed = IsHardmodeEnabled() ? runSpeedHardmode : runSpeed;

            if (!IsHardmodeEnabled() && isChasing && distanceToPlayer > sprintAtDistance)
            {
                targetRunSpeed = sprintSpeed;
                animator.speed = sprintSpeed / targetRunSpeed;
            }
            else
            {
                animator.speed = IsHardmodeEnabled() ? runSpeedHardmode / runSpeed : 1f;
            }

            if (EnemyIsOperative<A90Enemy>())
                targetRunSpeed = 0f;

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
            return angle <= (IsHardmodeEnabled() ? sightAngleHardmode : sightAngle);
        }

        private void CatchPlayer()
        {
            if (IsRoblomanDisguise)
            {
                room.OpenBouserDoorAsDefeated();
                RevealRoblomanDisguise();
                Despawn();
                return;
            }

            if (isCatching || isGrabbingTail || isMeetSkell || IsFriendly)
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
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0.5f, 0f));
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
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
    }
}
