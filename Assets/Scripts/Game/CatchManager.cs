using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Catch Manager")]
    [RequireComponent(typeof(AudioSource))]
    public sealed class CatchManager : MonoBehaviour
    {
        public static CatchManager Singleton { get; private set; }
        [SerializeField] private GameObject caughtScreen;
        [SerializeField] private Image screenshot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image subscribeImage;
        private bool canClickToRetry = false;
        private bool showingBroskyTip = false;
        private SoundResource broskyTip;
        private AudioSource audioSource;

        public void CatchPlayer(string title, string message, SoundResource broskyTip = null)
        {
            PlayerController.Singleton.IsCaught = true;
            PlayerController.Singleton.BlockAll();
            titleText.text = title;
            messageText.text = message;
            this.broskyTip = broskyTip;

            if (title.ToUpper().Equals("SANGOT ENDING"))
                subscribeImage.gameObject.SetActive(true);

            var recordOfRooms = PlayerPrefs.HasKey("RecordOfRooms") ? PlayerPrefs.GetInt("RecordOfRooms") : 0;

            if (LatestRoomNumber() > recordOfRooms)
                PlayerPrefs.SetInt("RecordOfRooms", LatestRoomNumber());

            Time.timeScale = 0f;
            StartCoroutine(nameof(TakeSnapshot));
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
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
            PlayerController.Singleton.UiCamera.enabled = false;
            yield return new WaitForEndOfFrame();
            screenshot.sprite = GetScreenshot();
            caughtScreen.SetActive(true);
            PlayerController.Singleton.UiCamera.enabled = true;
            yield return new WaitForSecondsRealtime(2f);
            canClickToRetry = true;
        }

        private void Update()
        {
            if (showingBroskyTip)
                return;

            if (Input.GetMouseButtonDown(0) && canClickToRetry)
            {
                if (broskyTip == null)
                    GameManager.Singleton.RestartLevel();
                else
                    StartCoroutine(nameof(BroskyTipCoroutine));
            }

        }

        IEnumerator BroskyTipCoroutine()
        {
            showingBroskyTip = true;
            audioSource.PlayOneShot(broskyTip.AudioClip);
            yield return new WaitForSecondsRealtime(0.2f);
            FadeScreenManager.Singleton.Display(broskyTip.SubtitleText, () =>
            {
                audioSource.Stop();
                GameManager.Singleton.RestartLevel();
            });
        }
    }
}
