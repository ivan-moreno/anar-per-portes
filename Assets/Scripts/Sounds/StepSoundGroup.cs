using UnityEngine;

namespace AnarPerPortes
{
    [CreateAssetMenu(menuName = "Anar per Portes/Step Sound Group")]
    public sealed class StepSoundGroup : ScriptableObject
    {
        /// <summary>
        /// How loud the footsteps are, gameplay-wise. (Skell's hearing uses this)
        /// </summary>
        public float AuditivePower => auditivePower;

        /// <summary>
        /// The array of stepping sounds for this group.
        /// </summary>
        public AudioClip[] StepSounds => stepSounds;

        [Tooltip("How loud the footsteps are, gameplay-wise. (Skell's hearing uses this)")]
        [SerializeField] private float auditivePower = 1f;

        [Tooltip("The array of stepping sounds for this group.")]
        [SerializeField] private AudioClip[] stepSounds;
    }
}
