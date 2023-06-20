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
        public bool IsDisplaying { get; private set; } = false;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image renderImage;

        private Animator screenAnimator;
        private Action onHideTipCallback;
        private float timeSinceDisplay = 0f;
        private const float minDisplayTime = 4f;

        public void DisplayTip(EnemyTip tip, Action onHideTipCallback = null)
        {
            if (IsDisplaying)
                return;

            IsDisplaying = true;
            timeSinceDisplay = 0f;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            titleText.text = tip.Title;
            messageText.text = tip.Message;
            renderImage.sprite = tip.Render;
            PlayerController.Singleton.BlockAll();
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
            if (!IsDisplaying)
                return;

            IsDisplaying = false;
            canvasGroup.blocksRaycasts = false;
            PlayerController.Singleton.UnblockAll();
            Time.timeScale = GameSettingsManager.Singleton.CurrentSettings.EnableSpeedrunMode ? 2f : 1f;
            onHideTipCallback?.Invoke();
        }

        private void Update()
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, IsDisplaying ? 1f : 0f, 4f * Time.unscaledDeltaTime);

            if (IsDisplaying)
                timeSinceDisplay += Time.unscaledDeltaTime;

            if (!PauseManager.Singleton.IsPaused
                && Input.GetMouseButtonUp(0)
                && IsDisplaying
                && timeSinceDisplay >= minDisplayTime)
                HideTip();
        }
    }
}
