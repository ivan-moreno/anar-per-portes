using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Player Model")]
    public class PlayerModel : MonoBehaviour
    {
        [SerializeField] private AudioClip[] stepSounds;
        private AudioSource audioSource;

        public void Step()
        {
            var hVelocityMagnitude = new Vector3(PlayerController.Singleton.Velocity.x, 0f, PlayerController.Singleton.Velocity.z).sqrMagnitude;

            if (hVelocityMagnitude <= 0.1f)
                return;

            audioSource.PlayOneShot(stepSounds.RandomItem());
            SkellHearManager.Singleton.AddNoise(1f);
        }

        private void Start()
        {
            audioSource = GetComponentInParent<AudioSource>();
        }
    }
}
