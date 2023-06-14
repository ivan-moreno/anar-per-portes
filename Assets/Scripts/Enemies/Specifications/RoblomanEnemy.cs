using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Robloman Enemy")]
    [RequireComponent(typeof(AudioSource))]
    public class RoblomanEnemy : Enemy
    {
        public UnityEvent OnPlayerPackedReward { get; } = new();
        [SerializeField] private PickableItem rewardPickable;

        [Header("Sound")]
        [SerializeField] private SoundResource[] rewardStartSounds;
        [SerializeField] private SoundResource rewardEndSound;
        [SerializeField] private SoundResource despawnSound;

        private bool isRewardCollected = false;

        public override void Spawn()
        {
            base.Spawn();
            CacheComponents();

            audioSource.PlayOneShot(rewardStartSounds.RandomItem());

            rewardPickable.OnPacked.AddListener(OnRewardPacked);
            LatestRoom().OnUnloading.AddListener(Despawn);
        }

        private void OnRewardPacked()
        {
            StartCoroutine(nameof(OnRewardPackedCoroutine));
        }

        private IEnumerator OnRewardPackedCoroutine()
        {
            if (isRewardCollected)
                yield break;

            isRewardCollected = true;
            animator.Play("RewardEnd", 0, 0f);
            audioSource.PlayOneShot(rewardEndSound);
            OnPlayerPackedReward?.Invoke();
            yield return new WaitForSeconds(0.6f);

            audioSource.PlayOneShot(despawnSound.AudioClip);
            yield return new WaitForSeconds(0.25f);

            Despawn();
        }

        private void LateUpdate()
        {
            if (isRewardCollected)
                return;

            transform.LookAt(PlayerController.Singleton.transform);
        }
    }
}
