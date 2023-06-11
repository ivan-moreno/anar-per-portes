using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Robloman Enemy")]
    [RequireComponent(typeof(AudioSource))]
    public class RoblomanEnemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;
        public UnityEvent OnPlayerPackedReward { get; } = new();
        [SerializeField] private PickableItem rewardPickable;

        [Header("Sound")]
        [SerializeField] private SoundResource[] rewardStartSounds;
        [SerializeField] private SoundResource rewardEndSound;

        private bool isRewardCollected = false;

        private void Start()
        {
            IsOperative = true;
            CacheComponents();

            audioSource.PlayOneShot(rewardStartSounds.RandomItem());

            rewardPickable.OnPacked.AddListener(OnRewardPacked);
            RoomManager.Singleton.LastLoadedRoom.OnUnloading.AddListener(Despawn);
        }

        private void OnRewardPacked()
        {
            if (isRewardCollected)
                return;

            isRewardCollected = true;
            animator.Play("RewardEnd", 0, 0f);
            audioSource.PlayOneShot(rewardEndSound);
            IsOperative = false;
            OnPlayerPackedReward?.Invoke();
            Destroy(gameObject, 0.85f);
        }

        private void LateUpdate()
        {
            if (isRewardCollected)
                return;

            transform.LookAt(PlayerController.Singleton.transform);
        }

        void Despawn()
        {
            IsOperative = false;
            Destroy(gameObject);
        }
    }
}
