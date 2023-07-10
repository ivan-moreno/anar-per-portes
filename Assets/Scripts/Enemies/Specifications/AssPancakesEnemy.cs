using AnarPerPortes.Rooms;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Ass Pancakes Enemy")]
    [RequireComponent(typeof(AudioSource))]
    public class AssPancakesEnemy : Enemy
    {
        public static UnityEvent<AssPancakesEnemy> OnSpawn { get; } = new();

        [Header("Components")]
        [SerializeField] private Transform attackOrigin;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float moveSpeed = 14f;
        [SerializeField][Min(0f)] private float attackDistance = 2.5f;
        [SerializeField][Min(0f)] private float attackHitRange = 1f;
        [SerializeField][Min(0f)] private float bouserRescueTime = 30f;
        [SerializeField][Min(0)] private int attacksToJump = 6;

        [Header("Audio")]
        [SerializeField] private SoundResource battleMusic;
        [SerializeField] private SoundResource introSound;
        [SerializeField] private SoundResource rageSound;
        [SerializeField] private SoundResource attackSound;
        [SerializeField] private SoundResource jumpSound;
        [SerializeField] private SoundResource defeatSound;
        [SerializeField] private SoundResource endingChatSound;

        private bool isWaiting = true;
        private bool isJumping = false;
        private bool isAttacking = false;
        private bool isRage = false;
        private float timeSinceSpawn = 0f;
        private int attacksSinceJump = 0;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            PlayerController.Singleton.OnBeginCatchSequence.AddListener(Despawn);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);

            OnSpawn?.Invoke(this);

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
            AudioManager.Singleton.SetVolume(1f);
            AudioManager.Singleton.PlayMusic(battleMusic.AudioClip);
            yield return new WaitForSeconds(rageSound.AudioClip.length * 0.7f);

            PlayerController.Singleton.ClearVisionTarget();
            PlayerController.Singleton.UnblockAll();

            yield return new WaitForSeconds(1f);
            isWaiting = false;
        }

        private void Update()
        {
            if (isInIntro || isWaiting || isJumping || EnemyIsOperative<A90Enemy>())
                return;

            if (!isAttacking)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                    animator.Play("Run");

                var nextPosition = Vector3.MoveTowards(transform.position, PlayerPosition(), moveSpeed * Time.deltaTime);
                transform.position = nextPosition;
                transform.LookAt(PlayerPosition());
            }
            else if (Vector3.Distance(attackOrigin.position, PlayerPosition()) < attackHitRange)
                transform.LookAt(PlayerPosition());

            timeSinceSpawn += Time.deltaTime;

            if (!isJumping && attacksSinceJump >= attacksToJump)
                StartCoroutine(nameof(JumpCoroutine));

            if (!isRage && timeSinceSpawn >= bouserRescueTime * 0.7f)
                StartCoroutine(nameof(RageCoroutine));

            if (timeSinceSpawn >= bouserRescueTime)
                StartCoroutine(nameof(BouserRescueCoroutine));

            var inAttackRange = Vector3.Distance(attackOrigin.position, PlayerPosition()) < attackDistance;

            if (!isAttacking && inAttackRange)
                StartCoroutine(nameof(AttackCoroutine));
        }

        private IEnumerator AttackCoroutine()
        {
            if (isAttacking || isJumping || isWaiting || EnemyIsOperative<A90Enemy>())
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
                audioSource.PlayOneShot(endingChatSound);
                CatchManager.Singleton.CatchPlayer("ASS PANCAKES ENDING", "HE-HEY! YOU SHAPE-SHIFTED INTO A DEAD GUY!", broskyTip);
                yield break;
            }

            if (isRage)
                yield return new WaitForSeconds(0.2f);
            else
                yield return new WaitForSeconds(1.0f);

            attacksSinceJump++;
            isAttacking = false;
            animator.Play("Idle");
        }

        IEnumerator RageCoroutine()
        {
            isWaiting = true;
            isRage = true;
            animator.Play("Rage");
            audioSource.PlayOneShot(rageSound);
            yield return new WaitForSeconds(0.8f);
            isWaiting = false;
        }

        IEnumerator JumpCoroutine()
        {
            isJumping = true;
            attacksSinceJump -= (attacksToJump - 1);

            animator.Play("Roll");
            audioSource.PlayOneShot(jumpSound);

            var dir = Vector3.Normalize(PlayerPosition() - transform.position);
            var timer = 0f;
            var startPosition = transform.position;
            var targetPosition = PlayerPosition() + dir * 4f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * 1.25f;
                var nextPosition = Vector3.Lerp(startPosition, targetPosition, timer);
                var yPos = Mathf.PingPong(timer * 16f, startPosition.y + 8f);
                nextPosition.y += yPos;
                transform.position = nextPosition;
                yield return new WaitForEndOfFrame();
            }

            transform.position = new(transform.position.x, startPosition.y, transform.position.z);

            if (isRage && attacksSinceJump > 0)
                StartCoroutine(nameof(JumpCoroutine));
                

            isJumping = false;
        }

        private IEnumerator BouserRescueCoroutine()
        {
            isWaiting = true;
            animator.Play("Idle");
            AudioManager.Singleton.SetTargetVolume(0f);
            (LatestRoom() as BouserRoom).WakeUpBouser();
            PlayerCollectTix(50, "Has evadido a Ass Pancakes");
            yield return new WaitForSeconds(4.5f);

            GetComponent<BoxCollider>().enabled = false;
            transform.LookAt(LatestRoom().transform.position);
            animator.Play("Run");
            audioSource.PlayOneShot(defeatSound);

            var timer = 0f;
            var startPosition = transform.position;

            while (timer < 1f)
            {
                timer += Time.deltaTime * 0.5f;
                var nextPosition = Vector3.Lerp(startPosition, LatestRoom().transform.position, timer);
                transform.position = nextPosition;
                yield return new WaitForEndOfFrame();
            }

            RoomManager.Singleton.Rooms[^2].Door.BreakThrough("AssPancakes");
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
