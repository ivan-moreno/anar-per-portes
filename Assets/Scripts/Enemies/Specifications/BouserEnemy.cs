using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Bouser Enemy")]
    public class BouserEnemy : Enemy
    {
        public static bool EnemyIsActive { get; set; } = false;
        public bool IsDefeated { get; private set; } = false;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float catchRange = 2f;
        [SerializeField] private float sightAngle = 45f;
        [SerializeField] private AudioClip[] warningSounds;
        [SerializeField] private string[] warningSoundSubtitles;
        [SerializeField] private AudioClip[] loseSounds;
        [SerializeField] private string[] loseSoundSubtitles;
        [SerializeField] private AudioClip[] findSounds;
        [SerializeField] private string[] findSoundSubtitles;
        [SerializeField] private AudioClip[] searchSounds;
        [SerializeField] private string[] searchSoundSubtitles;
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
        private float audioCooldown = 0f;
        private float nextMoveTime;
        private float timeSinceReachedTarget = 0f;
        private float timeSinceLastFindAudio = 0f;
        private float timeSinceLastLoseAudio = 0f;
        private float timeSinceLastSearchAudio = 0f;
        private const float nextMoveMinTime = 1.5f;
        private const float nextMoveMaxTime = 3f;
        private const float minTimeBetweenFindAudios = 3f;
        private const float minTimeBetweenLoseAudios = 3f;
        private const float minTimeBetweenSearchAudios = 7f;

        public void GrabTail()
        {
            if (isGrabbingTail)
                return;

            isGrabbingTail = true;
            animator.SetBool("IsWalking", false);
            animator.Play("TailGrab");
            audioSource.Stop();
            audioCooldown = 0f;
            PlayRandomAudio(tailSounds, tailSoundSubtitles, Team.Common);
            room.OpenBouserDoor();

            //TODO: Launch animation
            IsDefeated = true;
        }

        private void Start()
        {
            room = RoomManager.Singleton.LastLoadedRoom as BouserRoom;
            room.OnUnloading.AddListener(Despawn);

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

            nextMoveTime = Random.Range(nextMoveMinTime, nextMoveMaxTime);
            ChangeTargetLocationRandom();
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

            if (timeSinceLastSearchAudio >= minTimeBetweenSearchAudios)
            {
                PlayRandomAudio(searchSounds, searchSoundSubtitles, Team.Common);
                timeSinceLastSearchAudio = 0f;
            }
        }

        private void Update()
        {
            if (audioCooldown > 0f)
                audioCooldown -= Time.deltaTime;

            if (isGrabbingTail)
                return;

            var distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Singleton.transform.position);
            var playerIsInSight = IsWithinAngle(transform, PlayerController.Singleton.transform);

            // Player camouflaged while chasing
            if (isChasing && PlayerController.Singleton.IsHidingAsStatue)
            {
                if (timeSinceLastLoseAudio >= minTimeBetweenLoseAudios)
                {
                    PlayRandomAudio(loseSounds, loseSoundSubtitles, Team.Common);
                    timeSinceLastLoseAudio = 0f;
                }
            }

            var wasChasing = isChasing;

            isChasing = playerIsInSight
                && !PlayerController.Singleton.IsHidingAsStatue
                && !PlayerController.Singleton.IsCaught;

            if (isChasing && distanceToPlayer <= catchRange)
            {
                CatchPlayer();
            }

            if (isCatching)
                return;

            // Found Player, started chasing
            if (!wasChasing && isChasing)
            {
                if (timeSinceLastFindAudio >= minTimeBetweenFindAudios)
                {
                    PlayRandomAudio(findSounds, findSoundSubtitles);
                    timeSinceLastFindAudio = 0f;
                }
            }

            timeSinceLastFindAudio += Time.deltaTime;
            timeSinceLastLoseAudio += Time.deltaTime;
            timeSinceLastSearchAudio += Time.deltaTime;

            // Choose whether to go to the next map point or towards the Player.
            var determinedTargetLocation = isChasing ? PlayerController.Singleton.transform.position : targetLocation;

            var nextPosition = Vector3.MoveTowards(transform.position, determinedTargetLocation, runSpeed * Time.deltaTime);
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
            if (isCatching || isGrabbingTail)
                return;

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(Bouser grita)", Team.Hostile);
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

        //TODO: Static method
        private void PlayRandomAudio(AudioClip[] audios, string[] subtitles, Team source = Team.Hostile)
        {
            if (audioCooldown > 0f)
                return;

            var rngAudioIndex = Random.Range(0, audios.Length);
            var rngAudio = audios[rngAudioIndex];
            audioSource.PlayOneShot(rngAudio);
            SubtitleManager.Singleton.PushSubtitle(subtitles[rngAudioIndex], source);
            audioCooldown = rngAudio.length;
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            EnemyIsActive = false;
            Destroy(gameObject);
        }
    }
}
