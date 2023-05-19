using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Interaction Tooltip")]
    public class InteractionTooltip : MonoBehaviour
    {
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private TMP_Text messageText;
        private RectTransform rectTransform;
        private Transform followTarget;
        private Canvas canvas;

        public void Show(Transform target, string key, string message)
        {
            followTarget = target;
            keyText.text = key;
            messageText.text = message;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
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
            canvas = transform.root.GetComponent<Canvas>();
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                Hide();
                return;
            }

            var screenPosition = PlayerController.Instance.Camera.WorldToScreenPoint(followTarget.position);
            screenPosition -= new Vector3(rectTransform.sizeDelta.x / 2f, rectTransform.sizeDelta.y / 2f, 0f);
            screenPosition /= canvas.scaleFactor;
            rectTransform.anchoredPosition = screenPosition;
        }
    }
}
