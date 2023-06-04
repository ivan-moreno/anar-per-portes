using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    public class BouserEnemy : Enemy
    {
        public static bool EnemyIsActive { get; private set; } = false;

        public override bool EnemyTipWasDisplayed
        {
            get => enemyTipWasDisplayed;
            set => enemyTipWasDisplayed = value;
        }

        private static bool enemyTipWasDisplayed = false;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private float sightAngle = 45f;
        [SerializeField] private AudioClip[] warningSounds;
        [SerializeField] private string[] warningSoundSubtitles;
        [SerializeField] private AudioClip[] loseSounds;
        [SerializeField] private string[] loseSoundSubtitles;
        [SerializeField] private AudioClip[] tailSounds;
        [SerializeField] private string[] tailSoundSubtitles;
        [SerializeField] private AudioClip[] meetPedroSounds;
        [SerializeField] private string[] meetPedroSoundSubtitles;
        [SerializeField] private AudioClip jumpscareSound;
        private Animator animator;
        private Transform model;
        private AudioSource audioSource;
        private BouserRoom room;
        private Vector3 targetLocation;
        private bool reachedTarget = false;
        private bool isChasing = false;
        private bool isCatching = false;
        private bool isGrabbingTail = false;
        private float timeSinceReachedTarget = 0f;

        public void GrabTail()
        {
            if (isGrabbingTail)
                return;

            isGrabbingTail = true;
            animator.SetBool("IsWalking", false);
            PlayRandomAudio(tailSounds, tailSoundSubtitles);
            transform.Rotate(-90f, 0f, 0f);
            room.OpenBouserDoor();
        }

        private void Start()
        {
            room = RoomManager.Singleton.LastLoadedRoom as BouserRoom;
            transform.SetPositionAndRotation(room.BouserSpawnPoint.position, room.BouserSpawnPoint.rotation);
            audioSource = GetComponent<AudioSource>();
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
            EnemyIsActive = true;

            if (PedroEnemy.EnemyIsActive)
            {
                var pedroPos = FindObjectOfType<PedroEnemy>().transform.position;
                var dist = Vector3.Distance(transform.position, pedroPos);

                if (dist <= 32f)
                    PlayRandomAudio(meetPedroSounds, meetPedroSoundSubtitles);
                else
                    PlayRandomAudio(warningSounds, warningSoundSubtitles);
            }
            else
                PlayRandomAudio(warningSounds, warningSoundSubtitles);

            ChangeTargetLocationRandom();
        }

        //TODO: Try to not choose the same location twice in a row.
        private void ChangeTargetLocationRandom()
        {
            var rng = Random.Range(0, room.BouserWaypointGroup.childCount);
            targetLocation = room.BouserWaypointGroup.GetChild(rng).position;
        }

        private void Update()
        {
            if (isGrabbingTail)
                return;

            var distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Singleton.transform.position);
            var playerIsInSight = IsWithinAngle(transform, PlayerController.Singleton.transform);

            if (isChasing && PlayerController.Singleton.IsHidingAsStatue)
            {
                PlayRandomAudio(loseSounds, loseSoundSubtitles);
            }

            isChasing = playerIsInSight
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
            transform.position = nextPosition;

            reachedTarget = !isChasing && Vector3.Distance(transform.position, targetLocation) <= runSpeed * Time.deltaTime;
            animator.SetBool("IsWalking", !reachedTarget);

            if (reachedTarget)
            {
                timeSinceReachedTarget += Time.deltaTime;

                if (timeSinceReachedTarget >= 3f)
                {
                    timeSinceReachedTarget = 0f;
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
            if (isCatching || isGrabbingTail)
                return;

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(Bouser grita)", SubtitleCategory.Dialog, SubtitleSource.Hostile);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0.5f, 0f));
            EnemyIsActive = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.7f);
            CatchManager.Singleton.CatchPlayer("BOUSER ENDING", "a veces el amor te hace salir del caparazon jeje");
            audioSource.Play();
        }

        private void PlayRandomAudio(AudioClip[] audios, string[] subtitles)
        {
            var rngAudioIndex = Random.Range(0, audios.Length);
            var rngAudio = audios[rngAudioIndex];
            audioSource.PlayOneShot(rngAudio);
            SubtitleManager.Singleton.PushSubtitle(subtitles[rngAudioIndex], SubtitleCategory.Dialog, SubtitleSource.Hostile);
        }
    }
}
