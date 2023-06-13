using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Hallucination")]
    public class Hallucination : MonoBehaviour
    {
        [SerializeField] private float disappearTime = 0.5f;
        [SerializeField][Range(0f, 100f)] private float appearChance = 1f;
        private float timer;

        private void Start()
        {
            if (!GameSettingsManager.Singleton.CurrentSettings.EnableHallucinations)
            {
                gameObject.SetActive(false);
                return;
            }

            var rng = Random.Range(0f, 100f);

            if (rng > appearChance)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= disappearTime)
                gameObject.SetActive(false);
        }
    }
}
