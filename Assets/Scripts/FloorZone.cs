using UnityEngine;

namespace AnarPerPortes
{
    public sealed class FloorZone : MonoBehaviour
    {
        [SerializeField] private StepSoundGroup stepSoundGroup;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerController.Singleton.PlayerModel.SetStepSoundGroup(stepSoundGroup);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerController.Singleton.PlayerModel.ClearStepSoundGroup();
        }
    }
}
