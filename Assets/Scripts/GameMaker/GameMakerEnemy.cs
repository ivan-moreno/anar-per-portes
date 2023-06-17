using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Game Maker Enemy")]
    [RequireComponent(typeof(AudioSource))]
    public class GameMakerEnemy : Enemy
    {
        public static GameMakerEnemy Singleton { get; private set; }

        [Header("Components")]
        [SerializeField] private GameObject screen;
        [SerializeField] private Animator jumpscareAnimator;
        [SerializeField] private Text jumpscareLabel;

        [Header("Audio")]
        [SerializeField] private SoundResource jumpscareSound;

        private string jumpscareTargetMessage;

        public override void Spawn()
        {
            if (EnemyIsOperative<GameMakerEnemy>())
                return;

            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            EnemyManager.Singleton.MarkAsOperative(this);
            screen.SetActive(true);

            foreach (Transform t in transform)
                t.gameObject.SetActive(true);
        }

        protected override void Despawn()
        {
            if (!EnemyIsOperative<GameMakerEnemy>())
                return;

            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            screen.SetActive(false);

            foreach (Transform t in transform)
                t.gameObject.SetActive(false);

            EnemyManager.Singleton.UnmarkAsOperative(this);
        }

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
