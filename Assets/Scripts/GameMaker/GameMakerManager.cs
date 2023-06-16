using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Game Maker Manager")]
    [RequireComponent(typeof(AudioSource))]
    public class GameMakerManager : MonoBehaviour
    {
        public static GameMakerManager Singleton { get; private set; }

        [Header("Components")]
        [SerializeField] private Animator jumpscareAnimator;
        [SerializeField] private Text jumpscareLabel;

        [Header("Audio")]
        [SerializeField] private SoundResource jumpscareSound;

        private AudioSource audioSource;
        private string jumpscareTargetMessage;

        public void CatchPlayer()
        {
            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            //TODO: Use this Player property on all enemies' catch functionality
            if (PlayerController.Singleton.IsInCatchSequence)
                yield break;

            PlayerController.Singleton.BeginCatchSequence();
            PlayerController.Singleton.BlockAll();
            jumpscareAnimator.Play("Draw");
            audioSource.PlayOneShot(jumpscareSound);

            jumpscareLabel.text = string.Empty;

            while (jumpscareLabel.text.Length < jumpscareTargetMessage.Length - 2)
            {
                jumpscareLabel.text += jumpscareTargetMessage.Substring(jumpscareLabel.text.Length, 2);
                yield return new WaitForEndOfFrame();
            }

            FadeScreenManager.Singleton.Display();
            yield return new WaitForSeconds(1f);
            GameManager.Singleton.RestartLevel();

        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            jumpscareTargetMessage = jumpscareLabel.text;
        }
    }
}
