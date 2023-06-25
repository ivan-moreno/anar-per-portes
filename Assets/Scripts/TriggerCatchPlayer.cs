using UnityEngine;

namespace AnarPerPortes
{
    public class TriggerCatchPlayer : MonoBehaviour
    {
        [SerializeField] private string header;
        [SerializeField] private string message;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            CatchManager.Singleton.CatchPlayer(header, message);
            enabled = false;
        }
    }
}
