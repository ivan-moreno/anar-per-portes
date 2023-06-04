using UnityEngine;

namespace AnarPerPortes
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Enemy Model")]
    public class EnemyModel : MonoBehaviour
    {
        [SerializeField] private AudioClip[] stepSounds;
        private AudioSource audioSource;

        public void Step()
        {
            var rng = Random.Range(0, stepSounds.Length);
            var rngStepSound = stepSounds[rng];
            audioSource.PlayOneShot(rngStepSound);
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}
