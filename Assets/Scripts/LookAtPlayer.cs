using UnityEngine;

namespace AnarPerPortes
{
    public class LookAtPlayer : MonoBehaviour
    {
        private void LateUpdate()
        {
            var targetPos = PlayerController.Singleton.transform.position;
            targetPos.y = transform.position.y;
            transform.LookAt(targetPos);
        }
    }
}
