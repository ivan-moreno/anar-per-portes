using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Interaction Tooltip")]
    public class InteractionTooltip : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private TMP_Text messageText;

        private RectTransform rectTransform;
        private Animator animator;
        private Transform followTarget;
        private Canvas canvas;

        public void Show(Transform target, string key, string message)
        {
            followTarget = target;
            keyText.text = key;

            if (string.IsNullOrEmpty(key) || key == "None")
            {
                keyText.text = string.Empty;
                keyText.transform.GetChild(0).gameObject.SetActive(true);
                keyText.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                keyText.transform.GetChild(0).gameObject.SetActive(false);
                keyText.transform.GetChild(1).gameObject.SetActive(false);
            }

            messageText.text = message;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            animator.Play("Draw");
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);

            followTarget = null;
            keyText.text = string.Empty;
            messageText.text = string.Empty;
        }

        public void HideIfValidOwner(Transform targetOwner)
        {
            if (followTarget != targetOwner)
                return;

            Hide();
        }

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();
            canvas = transform.root.GetComponent<Canvas>();
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                Hide();
                return;
            }

            var screenPosition = PlayerController.Singleton.Camera.WorldToScreenPoint(followTarget.position);
            screenPosition -= new Vector3(rectTransform.sizeDelta.x / 2f, rectTransform.sizeDelta.y / 2f, 0f);
            screenPosition /= canvas.scaleFactor;
            rectTransform.anchoredPosition = screenPosition;
        }
    }
}
