using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemy Tip Manager")]
    public class EnemyTipManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image renderImage;
        private bool isDisplaying = false;
        private Action onHideTipCallback;

        public void DisplayTip(string title, string message, Sprite render, Action onHideTipCallback)
        {
            if (isDisplaying)
                return;

            isDisplaying = true;
            canvasGroup.blocksRaycasts = true;
            titleText.text = title;
            messageText.text = message;
            renderImage.sprite = render;
            renderImage.transform.GetChild(0).GetComponent<Image>().sprite = render;
            PlayerController.Instance.CanMove = false;
            PlayerController.Instance.CanLook = false;
            this.onHideTipCallback = onHideTipCallback;
            Time.timeScale = 0.001f;
        }

        private void HideTip()
        {
            if (!isDisplaying)
                return;

            isDisplaying = false;
            canvasGroup.blocksRaycasts = false;
            PlayerController.Instance.CanMove = true;
            PlayerController.Instance.CanLook = true;
            Time.timeScale = 1f;
            onHideTipCallback?.Invoke();
        }

        private void Update()
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, isDisplaying ? 1f : 0f, 4f * Time.unscaledDeltaTime);

            if (Input.GetMouseButtonUp(0) && isDisplaying)
                HideTip();
        }
    }
}
