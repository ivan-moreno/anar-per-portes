using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Skell Enemy")]
    public class SkellEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float minLookDistance = 20f;
        [SerializeField][Range(0f, 0.5f)] private float hViewLookRange = 0.1f;
        [SerializeField][Range(0f, 0.5f)] private float vViewLookRange = 0.1f;
        [SerializeField][Min(0f)] private float lookTimeToCatch = 0.5f;

        [Header("Audio")]
        [SerializeField] private SoundResource glimpseSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource endingMusic;

        private bool isLooked = false;
        private bool isInLookRange = false;
        private bool hasLineOfSight = false;
        private bool isCatching = false;
        private float lookTime;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            RepositionInRoom();
        }

        private void Update()
        {
            if (!isLooked || isCatching)
                return;

            lookTime += Time.deltaTime;

            if (lookTime >= lookTimeToCatch)
                CheckCatchOrDespawn();
        }

        private void LateUpdate()
        {
            if (isCatching)
                return;

            transform.LookAt(PlayerPosition());
        }

        private void FixedUpdate()
        {
            if (isLooked)
                return;

            if (IsLooking())
                Glimpse();
        }

        private bool IsLooking()
        {
            isInLookRange = DistanceToPlayer(transform) < minLookDistance;

            if (!isInLookRange)
                return false;

            hasLineOfSight = PlayerIsInLineOfSight(transform.position + Vector3.up);

            if (!hasLineOfSight)
                return false;

            var viewportPos = PlayerController.Singleton.Camera.WorldToViewportPoint(transform.position + Vector3.up);

            if (viewportPos.z <= 0f)
                return false;

            var isInHorizontalViewRange = viewportPos.x > 0.5f - hViewLookRange && viewportPos.x < 0.5f + hViewLookRange;
            var isInVerticalViewRange = viewportPos.y > 0.5f - vViewLookRange && viewportPos.y < 0.5f + vViewLookRange;

            return isInHorizontalViewRange && isInVerticalViewRange;
        }

        private void RepositionInRoom()
        {
            var room = RoomManager.Singleton.LastLoadedRoom;

            if (room.SkellLocationsGroup == null)
            {
                Despawn();
                return;
            }

            var rngLocationIndex = Random.Range(0, room.SkellLocationsGroup.childCount);

            transform.position = room.SkellLocationsGroup.GetChild(rngLocationIndex).position;
        }

        private void Glimpse()
        {
            if (isLooked)
                return;

            isLooked = true;
            audioSource.PlayOneShot(glimpseSound);
            SkellHearManager.Singleton.PauseHuntMusic();
        }

        private void CheckCatchOrDespawn()
        {
            if (IsLooking())
                CatchPlayer();
            else
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
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new(0f, -0.25f, 0f));
            SkellHearManager.Singleton.PauseHuntMusic();
            animator.Play("Jumpscare");
            audioSource.PlayOneShot(jumpscareSound);

            yield return new WaitForSeconds(1f);

            var timer = 0f;
            var originalPos = transform.position;
            var targetPos = PlayerPosition() + new Vector3(0f, 0.5f, 0f) - transform.forward * 2f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * 4f;
                transform.position = Vector3.Lerp(originalPos, targetPos, timer);
                yield return null;
            }

            CatchManager.Singleton.CatchPlayer("SKELL ENDING", "ni modo");
            audioSource.PlayOneShot(endingMusic);
        }

        private void Despawn()
        {
            if (isCatching)
                return;

            IsOperative = false;
            SkellHearManager.Singleton.UnpauseHuntMusic();
            Destroy(gameObject);
        }
    }
}
