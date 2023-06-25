using UnityEngine;

namespace AnarPerPortes
{
    public class WoolooEntity : MonoBehaviour
    {
        private float speed;

        private void Start()
        {
            speed = Random.Range(4f, 8f);
            var randomX = Random.Range(0f, -3f);
            transform.localPosition = new Vector3(randomX, 0f, transform.localPosition.z);
            GetComponent<Animator>().speed = speed / 4f;
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        }
    }
}
