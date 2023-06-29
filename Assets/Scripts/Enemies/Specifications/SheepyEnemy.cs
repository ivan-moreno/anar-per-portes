using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Sheepy Enemy")]
    public class SheepyEnemy : Enemy
    {
        [Header("Components")]
        [SerializeField] Animator orisaAnimator;
        [SerializeField] GameObject orisaRoblomanEars;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float checkMotionTime = 1.2f;
        [SerializeField][Min(0f)] private float checkMotionTimeHard = 0.6f;
        [SerializeField][Min(0f)] private float despawnTime = 2.2f;
        [SerializeField][Min(0f)] private float orisaDespawnTime = 2.6f;
        [SerializeField][Range(0f, 100f)] private float orisaChance = 1f;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource safeSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource[] meetDaviloteSounds;
        [SerializeField] private SoundResource[] meetDaviloteEndSounds;
        [SerializeField] private SoundResource orisaWarningSound;
        [SerializeField] private SoundResource orisaSafeSound;
        [SerializeField] private SoundResource[] orisaEndingChatSounds;

        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private bool isMeetingDavilote = false;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            var lastRoom = RoomManager.Singleton.LatestRoom.transform;
            var targetPos = lastRoom.position + lastRoom.forward * 4f;
            transform.position = targetPos;
            transform.LookAt(PlayerPosition());
            
            SkellHearManager.Singleton.AddNoise(8f);

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            BouserBossEnemy.OnSpawn.AddListener((_) => Despawn());
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            if (Random.Range(0f, 100f) <= orisaChance)
            {
                model.gameObject.SetActive(false);
                orisaAnimator.gameObject.SetActive(true);
                audioSource.PlayOneShot(orisaWarningSound);
            }
            else
            {
                audioSource.PlayOneShot(warningSound);
            }
        }

        public override void MarkAsRoblomanDisguise()
        {
            base.MarkAsRoblomanDisguise();

            if (orisaAnimator.gameObject.activeSelf)
                orisaRoblomanEars.SetActive(true);
        }

        protected override void Despawn()
        {
            if (isCatching)
                return;

            if (orisaAnimator.gameObject.activeSelf)
                PlayerCollectTix(10, "Has evadido a Orisa");
            else
                PlayerCollectTix(10, "Has evadido a Sheepy");

            base.Despawn();
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
            if (isInIntro)
                return;

            timeSinceSpawn += Time.deltaTime;

            var targetCheckMotionTime = IsHardmodeEnabled() ? checkMotionTimeHard : checkMotionTime;

            if (!checkedMotion && timeSinceSpawn >= targetCheckMotionTime)
                CheckForMotion();

            var targetDespawnTime = orisaAnimator.gameObject.activeSelf ? orisaDespawnTime : despawnTime;

            if (!isCatching && !isMeetingDavilote && timeSinceSpawn >= targetDespawnTime)
                Despawn();
        }

        private void CheckForMotion()
        {
            if (checkedMotion)
                return;

            checkedMotion = true;

            var movingX = Mathf.Abs(PlayerController.Singleton.Velocity.x) > 0f;
            var movingZ = Mathf.Abs(PlayerController.Singleton.Velocity.z) > 0f;

            if (movingX || movingZ)
                CatchPlayer();
            else
            {
                if (EnemyIsOperative<DaviloteEnemy>() && !orisaAnimator.gameObject.activeSelf)
                    StartCoroutine(nameof(MeetDaviloteCoroutine));
                else
                {
                    if (orisaAnimator.gameObject.activeSelf)
                    {
                        orisaAnimator.Play("Retreat");
                        audioSource.PlayOneShot(orisaSafeSound);
                    }
                    else
                    {
                        animator.Play("Retreat");
                        audioSource.PlayOneShot(safeSound);
                    }
                }
            }
        }

        private IEnumerator MeetDaviloteCoroutine()
        {
            isMeetingDavilote = true;
            audioSource.Stop();
            var sound = meetDaviloteSounds.RandomItem();
            animator.Play("MeetDaviloteStart", 0, 0f);
            audioSource.PlayOneShot(sound);
            yield return new WaitForSeconds(sound.AudioClip.length + 0.1f);

            var waitTime = GetEnemyInstance<DaviloteEnemy>().MeetSheepy();
            yield return new WaitForSeconds(waitTime);

            animator.Play("MeetDaviloteEnd", 0, 0f);
            audioSource.PlayOneShot(meetDaviloteEndSounds.RandomItem());
            yield return new WaitForSeconds(1.2f);

            Despawn();
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
                Despawn();
                yield break;
            }

            if (isCatching)
                yield break;

            isCatching = true;
            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);
            animator.Play("Jumpscare");
            audioSource.PlayOneShot(jumpscareSound);

            var timer = 0f;
            var originalPos = transform.position;
            var targetPos = PlayerPosition() + PlayerController.Singleton.transform.forward * 2f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * 1.5f;
                transform.LookAt(PlayerPosition());
                transform.position = Vector3.Lerp(originalPos, targetPos, timer);
                yield return null;
            }

            if (orisaAnimator.gameObject.activeSelf)
            {
                var rngEndingChat = orisaEndingChatSounds.RandomItem();
                audioSource.PlayOneShot(rngEndingChat);
                CatchManager.Singleton.CatchPlayer("ORISA ENDING", rngEndingChat.SubtitleText);
            }
            else
                CatchManager.Singleton.CatchPlayer("SHEEPY ENDING", "¡¡Deja paso a las ovejaaas!!");
        }
    }
}
