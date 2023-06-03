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

            var rng = Random.Range(0, stepSounds.Length);
            var rngStepSound = stepSounds[rng];
            audioSource.PlayOneShot(rngStepSound);
        }

        private void Start()
        {
            audioSource = GetComponentInParent<AudioSource>();
        }
    }
}
