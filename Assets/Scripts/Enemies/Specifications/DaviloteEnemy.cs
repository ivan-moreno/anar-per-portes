using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Davilote Enemy")]
    public class DaviloteEnemy : Enemy
    {
        public static UnityEvent<DaviloteEnemy> OnSpawn { get; } = new();

        [Header("Components")]
        [SerializeField] GameObject jinxModel;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float catchAngle = 120f;
        [SerializeField][Range(0f, 100f)] private float jinxChance = 1f;

        [Header("Audio")]
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource roblobolitaSound;
        [SerializeField] private SoundResource meetPedroSound;
        [SerializeField] private SoundResource sangotMeetDaviloteSound;
        [SerializeField] private SoundResource jinxCatchSound;
        [SerializeField] private AudioClip jinxCatchMusic;
        [SerializeField] private SoundResource[] warningSounds;
        [SerializeField] private SoundResource[] meetSheepySounds;
        [SerializeField] private SoundResource[] endingChatSounds;
        [SerializeField] private string[] endingMessages;

        public float MeetSheepy()
        {
            audioSource.Stop();
            var sound = meetSheepySounds.RandomItem();
            audioSource.PlayOneShot(sound);
            return sound.AudioClip.length + 0.1f;
        }

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            transform.rotation = PlayerController.Singleton.transform.rotation;

            if (EnemyIsOperative<PedroEnemy>() && !GetEnemyInstance<PedroEnemy>().IsOnBreak)
                audioSource.PlayOneShot(meetPedroSound);
            else
                audioSource.PlayOneShot(warningSounds.RandomItem());

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            BouserBossEnemy.OnSpawn.AddListener((_) => Despawn());
            RoomManager.Singleton.OnRoomGenerated.AddListener(x => Despawn());
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            if (Random.Range(0f, 100f) <= jinxChance)
            {
                model.gameObject.SetActive(false);
                jinxModel.SetActive(true);
            }

            OnSpawn?.Invoke(this);
        }

        protected override void Despawn()
        {
            if (isCatching)
                return;

            PlayerCollectTix(10, "Has evadido a Davilote");
            base.Despawn();
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void LateUpdate()
        {
            transform.position = PlayerPosition() - transform.forward * 4f;
        }

        private void FixedUpdate()
        {
            if (isInIntro)
                return;

            var playerAngle = PlayerController.Singleton.transform.eulerAngles.y;
            var enemyAngle = transform.eulerAngles.y;
            var angleDiff = Mathf.DeltaAngle(playerAngle, enemyAngle);

            var vCameraAngle = PlayerController.Singleton.Camera.transform.eulerAngles.x;

            if (vCameraAngle is > 60f and < 80f or > 280f and < 300f)
                return;

            if (Mathf.Abs(angleDiff) > catchAngle)
                CatchPlayer();
        }

        private void CatchPlayer()
        {
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            if (IsRoblomanDisguise)
            {
                RevealRoblomanDisguise();
                Despawn();
                yield break;
            }

            if (TryConsumePlayerImmunityItem())
            {
                PlayerSound(roblobolitaSound);
                Despawn();
                yield break;
            }

            if (isCatching)
                yield break;

            isCatching = true;

            var doMeetSangot = false;

            if (EnemyIsOperative<SangotEnemy>())
                doMeetSangot = true;

            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);

            if (jinxModel.activeSelf)
                audioSource.PlayOneShot(jinxCatchSound);
            else
            {
                animator.Play("Jumpscare");
                audioSource.PlayOneShot(jumpscareSound);
            }

            RoomManager.Singleton.SetRoomsActive(false);
            yield return new WaitForSeconds(0.84f);

            audioSource.Stop();

            var rngChat = Random.Range(0, endingChatSounds.Length);

            if (jinxModel.activeSelf)
            {
                audioSource.PlayOneShot(jinxCatchMusic);
                CatchManager.Singleton.CatchPlayer("JINX ENDING", "@bigfootjinx meow reveal!!1!");
            }
            else
            {
                audioSource.Play();
                CatchManager.Singleton.CatchPlayer("DAVILOTE ENDING", endingMessages[rngChat], broskyTip);
            }

            if (doMeetSangot)
            {
                audioSource.PlayOneShot(sangotMeetDaviloteSound);
                yield return new WaitForSecondsRealtime(sangotMeetDaviloteSound.AudioClip.length);
            }

            if (!jinxModel.activeSelf)
                audioSource.PlayOneShot(endingChatSounds[rngChat]);
        }
    }
}
