using AnarPerPortes.Rooms;
using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Ass Pancakes Enemy")]
    [RequireComponent(typeof(AudioSource))]
    public class AssPancakesEnemy : Enemy
    {
        [Header("Components")]
        [SerializeField] private Transform attackOrigin;

        [Header("Stats")]
        [SerializeField] private float moveSpeed = 14f;
        [SerializeField] private float jumpDuration = 1f;
        [SerializeField] private float attackDistance = 2.5f;
        [SerializeField] private float attackHitRange = 1f;
        [SerializeField] private float bouserRescueTime = 30f;

        [Header("Audio")]
        [SerializeField] private SoundResource introSound;
        [SerializeField] private SoundResource rageSound;
        [SerializeField] private SoundResource attackSound;
        [SerializeField] private SoundResource jumpSound;
        [SerializeField] private SoundResource defeatSound;

        private bool isWaiting = true;
        private bool isJumping = false;
        private bool isAttacking = false;
        private float timeSinceSpawn = 0f;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            StartCoroutine(nameof(IntroCoroutine));
        }

        private IEnumerator IntroCoroutine()
        {
            transform.position = LatestRoom().transform.position + LatestRoom().transform.forward * 8f;
            transform.LookAt(PlayerPosition());
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform);
            yield return new WaitForSeconds(1f);

            audioSource.PlayOneShot(introSound);
            yield return new WaitForSeconds(introSound.AudioClip.length + 1f);

            animator.Play("Rage");
            audioSource.PlayOneShot(rageSound);
            yield return new WaitForSeconds(rageSound.AudioClip.length * 0.7f);

            PlayerController.Singleton.ClearVisionTarget();
            PlayerController.Singleton.UnblockAll();

            yield return new WaitForSeconds(1f);
            isWaiting = false;
        }

        private void Update()
        {
            if (isWaiting)
                return;

            if (!isAttacking)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                    animator.Play("Run");

                var nextPosition = Vector3.MoveTowards(transform.position, PlayerPosition(), moveSpeed * Time.deltaTime);
                transform.position = nextPosition;
                transform.LookAt(PlayerPosition());
            }

            timeSinceSpawn += Time.deltaTime;

            if (timeSinceSpawn >= bouserRescueTime)
                StartCoroutine(nameof(BouserRescueCoroutine));

            var inAttackRange = Vector3.Distance(attackOrigin.position, PlayerPosition()) < attackDistance;

            if (!isAttacking && inAttackRange)
                StartCoroutine(nameof(AttackCoroutine));
        }

        private IEnumerator AttackCoroutine()
        {
            if (isAttacking || isWaiting)
                yield break;

            isAttacking = true;
            animator.Play("Attack");
            yield return new WaitForSeconds(0.5f);

            audioSource.PlayOneShot(attackSound);

            if (Vector3.Distance(attackOrigin.position, PlayerPosition()) < attackHitRange)
            {
                if (isWaiting)
                {
                    isAttacking = false;
                    animator.Play("Idle");
                    yield break;
                }

                isCatching = true;
                PlayerController.Singleton.BeginCatchSequence();
                PlayerController.Singleton.BlockAll();
                PlayerController.Singleton.SetVisionTarget(transform);
                yield return new WaitForSeconds(0.35f);
                CatchManager.Singleton.CatchPlayer("ASS PANCAKES ENDING", "HOW");
                yield break;
            }

            yield return new WaitForSeconds(1.0f);

            isAttacking = false;
            animator.Play("Idle");
        }

        private IEnumerator BouserRescueCoroutine()
        {
            isWaiting = true;
            animator.Play("Idle");
            (LatestRoom() as BouserRoom).WakeUpBouser();
            yield return new WaitForSeconds(4.5f);

            GetComponent<BoxCollider>().enabled = false;
            transform.LookAt(LatestRoom().transform.position);
            animator.Play("Run");
            audioSource.PlayOneShot(defeatSound);

            var timer = 0f;
            var startPosition = transform.position;

            while (timer < 1f)
            {
                timer += Time.deltaTime;
                var nextPosition = Vector3.Lerp(startPosition, LatestRoom().transform.position, timer);
                transform.position = nextPosition;
                yield return new WaitForEndOfFrame();
            }

            Despawn();
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }
    }
}
