using AnarPerPortes.Rooms;
using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    public class ToymakerEnemy : Enemy
    {
        [Header("Components")]
        [SerializeField] private Transform wakeProximityDetector;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float speed = 4f;
        [SerializeField][Min(0f)] private float wakeProximityDistance = 4f;
        [SerializeField][Min(0f)] private float catchDistance = 1.5f;
        [SerializeField][Min(0f)] private float spawnAtDoorTime = 3f;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private AudioClip chaseMusic;

        private bool isWaiting = true;
        private bool isAwake = false;

        public override void Spawn()
        {
            base.Spawn();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();

            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
            RoomManager.Singleton.OnRoomGenerated.AddListener(RoomGenerated);
        }

        protected override void Despawn()
        {
            AudioManager.Singleton.StopMusic();
            base.Despawn();
        }

        private IEnumerator WakeUpCoroutine()
        {
            if (isAwake)
                yield break;

            isAwake = true;
            Spawn();
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            PlayerController.Singleton.SetVisionTarget(transform);
            audioSource.PlayOneShot(spawnSound);
            audioSource.Play();
            transform.GetChild(0).gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);

            PlayerController.Singleton.UnblockMove();
            PlayerController.Singleton.UnblockLook();
            PlayerController.Singleton.ClearVisionTarget();
            yield return new WaitForSeconds(0.5f);

            isWaiting = false;
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void RoomGenerated(Room room)
        {
            if (room.Door is not ToymakerRoomDoor)
            {
                Despawn();
                return;
            }

            StopCoroutine(nameof(ReappearCoroutine));
            StartCoroutine(nameof(ReappearCoroutine));
        }

        private void FixedUpdate()
        {
            if (!isAwake && DistanceToPlayer(wakeProximityDetector) < wakeProximityDistance)
                StartCoroutine(nameof(WakeUpCoroutine));

            if (isInIntro || !isAwake || isWaiting || isCatching)
                return;

            transform.position = Vector3.MoveTowards(transform.position, PlayerPosition(), speed * Time.fixedDeltaTime);

            if (DistanceToPlayer(transform) < catchDistance)
                StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator ReappearCoroutine()
        {
            isWaiting = true;
            yield return new WaitForSeconds(spawnAtDoorTime);
            transform.position = LatestRoom().transform.position;
            isWaiting = false;
        }

        IEnumerator CatchPlayerCoroutine()
        {
            if (isCatching)
                yield break;

            isCatching = true;
            PauseManager.Singleton.CanPause = false;
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);
            audioSource.Stop();
            audioSource.PlayOneShot(jumpscareSound);
            animator.speed = 0f;

            yield return new WaitForSeconds(jumpscareSound.AudioClip.length - 0.1f);
            BlackoutManager.Singleton.PlayInstantly();
            AudioManager.Singleton.MuteAllAudioMixers();
            yield return new WaitForSeconds(5f);

            AudioManager.Singleton.UnmuteAllAudioMixers();
            GameManager.Singleton.RestartLevel();
        }
    }
}
