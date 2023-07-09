using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Player Model")]
    public class PlayerModel : MonoBehaviour
    {
        [SerializeField] private StepSoundGroup defaultStepSoundGroup;
        private StepSoundGroup currentStepSoundGroup;

        private AudioSource audioSource;

        public void Step()
        {
            var hVelocityMagnitude = new Vector3(PlayerController.Singleton.Velocity.x, 0f, PlayerController.Singleton.Velocity.z).sqrMagnitude;

            if (hVelocityMagnitude <= 0.1f)
                return;

            audioSource.PlayOneShot(currentStepSoundGroup.StepSounds.RandomItem(), 0.3f);
            SkellHearManager.Singleton.AddNoise(currentStepSoundGroup.AuditivePower);
        }

        public void SetStepSoundGroup(StepSoundGroup stepSoundGroup)
        {
            currentStepSoundGroup = stepSoundGroup;
        }

        public void ClearStepSoundGroup()
        {
            currentStepSoundGroup = defaultStepSoundGroup;
        }

        private void Start()
        {
            audioSource = GetComponentInParent<AudioSource>();
        }
    }
}
