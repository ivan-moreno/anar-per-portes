using static AnarPerPortes.ShortUtils;
using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Sheepy Enemy")]
    public class SheepyEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;

        [Header("Stats")]
        [SerializeField] private float checkMotionTime = 1.2f;
        [SerializeField] private float checkMotionTimeHard = 0.6f;
        [SerializeField] private float despawnTime = 2.2f;

        [Header("Audio")]
        [SerializeField] private SoundResource warningSound;
        [SerializeField] private SoundResource safeSound;
        [SerializeField] private SoundResource jumpscareSound;

        private float timeSinceSpawn = 0f;
        private bool checkedMotion = false;
        private bool isCatching = false;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            var lastRoom = RoomManager.Singleton.LastLoadedRoom.transform;
            var targetPos = lastRoom.position + (lastRoom.forward * 4f);
            transform.position = targetPos;
            transform.LookAt(PlayerController.Singleton.transform.position);
            audioSource.Play(warningSound);
            SkellHearManager.Singleton.AddNoise(8f);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
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
            timeSinceSpawn += Time.deltaTime;

            var targetCheckMotionTime = IsHardmodeEnabled() ? checkMotionTimeHard : checkMotionTime;

            if (!checkedMotion && timeSinceSpawn >= targetCheckMotionTime)
                CheckForMotion();

            if (!isCatching && timeSinceSpawn >= despawnTime)
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
                animator.Play("Retreat");
                SubtitleManager.Singleton.PushSubtitle(safeSound);
            }
        }

        private void CatchPlayer()
        {
            if (isCatching)
                return;

            if (PlayerController.Singleton.EquippedItemIs("Roblobolita"))
            {
                PlayerController.Singleton.ConsumeEquippedItem();
                BlurOverlayManager.Singleton.SetBlur(Color.white);
                BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 2f);
                Despawn();
                return;
            }

            isCatching = true;
            animator.Play("Jumpscare");
            audioSource.PlayOneShot(jumpscareSound);
            PlayerController.Singleton.BlockAll();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(0.8f);
            CatchManager.Singleton.CatchPlayer("SHEEPY ENDING", "¡¡Deja paso a las ovejaaas!!");
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
