using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Catch Manager")]
    public sealed class CatchManager : MonoBehaviour
    {
        public static CatchManager Singleton { get; private set; }
        [SerializeField] private GameObject caughtScreen;
        [SerializeField] private Image screenshot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        private bool canClickToRetry = false;

        public void CatchPlayer(string title, string message)
        {
            PlayerController.Singleton.IsCaught = true;
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            titleText.text = title;
            messageText.text = message;
            Time.timeScale = 0f;
            StartCoroutine(nameof(TakeSnapshot));
        }

        private void Awake()
        {
            Singleton = this;
        }

        private Sprite GetScreenshot()
        {
            var screenshotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshotTexture.Apply();

            return Sprite.Create(screenshotTexture, new Rect(0, 0, Screen.width, Screen.height), Vector2.one * 0.5f);
        }

        private IEnumerator TakeSnapshot()
        {
            yield return new WaitForEndOfFrame();
            screenshot.sprite = GetScreenshot();
            caughtScreen.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            canClickToRetry = true;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && canClickToRetry)
            {
                GameManager.Singleton.RestartLevel();
            }
        }
    }
}
