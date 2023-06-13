using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Pedro Stand")]
    public class PedroStand : MonoBehaviour
    {
        [SerializeField] private SoundResource[] spawnSounds;
        [SerializeField] private Animator pedroAnimator;
        private AudioSource pedroAudioSource;

        private void Start()
        {
            pedroAudioSource = pedroAnimator.GetComponent<AudioSource>();
            pedroAudioSource.PlayOneShot(spawnSounds.RandomItem());
        }
    }
}
