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
            audioSource.PlayOneShot(stepSounds.RandomItem());
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}
