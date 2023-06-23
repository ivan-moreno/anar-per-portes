using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Items/Tix")]
    public class Tix : MonoBehaviour
    {
        private const float collectDistance = 2f;

        private void Update()
        {
            transform.Rotate(0f, 180f * Time.deltaTime, 0f);
        }

        private void FixedUpdate()
        {
            if (DistanceToPlayer(transform) < collectDistance)
            {
                PlayerController.Singleton.CollectTix(5);
                Destroy(gameObject);
            }
        }
    }
}
