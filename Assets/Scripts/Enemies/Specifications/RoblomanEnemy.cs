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

            rewardPickable.OnPacked.AddListener(OnRewardPacked);
            LatestRoom().OnUnloading.AddListener(Despawn);
            PlayerCollectTix(25, "¡Te has encontrado con Robloman!");

            if (PlayerController.Singleton.HasItem("Roblobolita"))
                StartCoroutine(nameof(OnHasRewardCoroutine));
            else
                audioSource.PlayOneShot(rewardStartSounds.RandomItem());
        }

        private void OnRewardPacked()
        {
            StartCoroutine(nameof(OnRewardPackedCoroutine));
        }

        private IEnumerator OnHasRewardCoroutine()
        {
            rewardPickable.gameObject.SetActive(false);
            transform.LookAt(PlayerController.Singleton.transform);
            animator.Play("RewardEnd", 0, 0f);
            audioSource.PlayOneShot(rewardEndSound);
            yield return new WaitForSeconds(0.6f);

            audioSource.PlayOneShot(despawnSound.AudioClip);
            yield return new WaitForSeconds(0.25f);
            OnPlayerPackedReward?.Invoke();

            Despawn();
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
