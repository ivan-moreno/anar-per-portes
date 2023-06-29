using System;
using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Fade Screen Manager")]
    public sealed class FadeScreenManager : MonoBehaviour
    {
        public static FadeScreenManager Singleton { get; private set; }
        [SerializeField] private CanvasGroup screenGroup;
        [SerializeField] private TMP_Text messageText;
        private Action onClickCallback;
        private bool isDisplaying = false;
        private float timeSinceDisplay = 0f;
        private const float minDuration = 1f;

        public void Display(string message = "", Action onClickCallback = null)
        {
            if (isDisplaying)
                return;

            isDisplaying = true;
            timeSinceDisplay = 0f;
            messageText.text = message;
            this.onClickCallback = onClickCallback;
            PlayerController.Singleton.BlockAll();
            AudioManager.Singleton.SetTargetVolume(0f);
        }

        public void Hide()
        {
            if (!isDisplaying)
                return;

            isDisplaying = false;
            timeSinceDisplay = 0f;
            PlayerController.Singleton.UnblockAll();
            AudioManager.Singleton.SetTargetVolume(1f);
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Update()
        {
            if (isDisplaying)
            {
                timeSinceDisplay += Time.unscaledDeltaTime;

                if (Input.GetMouseButtonUp(0) && timeSinceDisplay >= minDuration)
                {
                    Hide();
                    onClickCallback?.Invoke();
                }
            }


            if (screenGroup.alpha > 0.99f)
                AudioManager.Singleton.MuteAllAudioMixers();

            //AudioListener.volume = Mathf.Clamp01(Mathf.MoveTowards(AudioListener.volume, isDisplaying ? 0f : GameSettingsManager.Singleton.CurrentSettings.Volume, 4f * Time.unscaledDeltaTime));
            screenGroup.alpha = Mathf.MoveTowards(screenGroup.alpha, isDisplaying ? 1f : 0f, 4f * Time.unscaledDeltaTime);
        }
    }
}
