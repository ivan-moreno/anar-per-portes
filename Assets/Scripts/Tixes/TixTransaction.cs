using System.Collections;
using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Tix Transaction")]
    public class TixTransaction : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountLabel;
        [SerializeField] private TMP_Text reasonLabel;

        private const float duration = 5f;

        public void Initialize(int amount, string reason)
        {
            amountLabel.text = amount.ToString();
            reasonLabel.text = reason;
            StartCoroutine(nameof(InitializeCoroutine));
        }

        private IEnumerator InitializeCoroutine()
        {
            yield return new WaitForSeconds(duration);

            GetComponent<Animator>().Play("Undraw");
            yield return new WaitForSeconds(0.5f);

            Destroy(gameObject);
        }
    }
}
