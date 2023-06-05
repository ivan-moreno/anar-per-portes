using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Enemy Tip Manager")]
    public sealed class EnemyTipManager : MonoBehaviour
    {
        public static EnemyTipManager Singleton { get; private set; }
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image renderImage;
        private Animator screenAnimator;
        private bool isDisplaying = false;
        private Action onHideTipCallback;
        private float timeSinceDisplay = 0f;
        private const float minDisplayTime = 4f;

        public void DisplayTip(string title, string message, Sprite render, Action onHideTipCallback)
        {
            if (isDisplaying)
                return;

            isDisplaying = true;
            timeSinceDisplay = 0f;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            titleText.text = title;
            messageText.text = message;
            renderImage.sprite = render;
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            this.onHideTipCallback = onHideTipCallback;
            screenAnimator.Play("Draw", 0, 0f);
            Time.timeScale = 0f;
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            screenAnimator = canvasGroup.GetComponent<Animator>();
        }

        private void HideTip()
        {
            if (!isDisplaying)
                return;

            isDisplaying = false;
            canvasGroup.blocksRaycasts = false;
            PlayerController.Singleton.UnblockMove();
            PlayerController.Singleton.UnblockLook();
            Time.timeScale = 1f;
            onHideTipCallback?.Invoke();
        }

        private void Update()
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, isDisplaying ? 1f : 0f, 4f * Time.unscaledDeltaTime);

            if (isDisplaying)
                timeSinceDisplay += Time.unscaledDeltaTime;

            if (!PauseManager.Singleton.IsPaused
                && Input.GetMouseButtonUp(0)
                && isDisplaying
                && timeSinceDisplay >= minDisplayTime)
                HideTip();
        }
    }
}
