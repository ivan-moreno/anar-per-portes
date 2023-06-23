using AnarPerPortes.Rooms;
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

        public bool DesktopEnabled { get; private set; } = false;

        [Header("Components")]
        [SerializeField] private GameObject screen;
        [SerializeField] private Button startButton;
        [SerializeField] private Animator jumpscareAnimator;
        [SerializeField] private Text jumpscareLabel;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;
        [SerializeField] private SoundResource despawnSound;
        [SerializeField] private SoundResource jumpscareSound;

        private string jumpscareTargetMessage;

        public override void Spawn()
        {
            if (EnemyIsOperative<GameMakerEnemy>())
                return;

            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            EnemyManager.Singleton.MarkAsOperative(this);

            PlayerController.Singleton.PackItem("OperativeSystem");
            PlayerController.Singleton.GetItem("OperativeSystem").OnEquipped.AddListener(ShowDesktop);
            PlayerController.Singleton.GetItem("OperativeSystem").OnUnequipped.AddListener(HideDesktop);

            foreach (Transform t in transform)
                t.gameObject.SetActive(true);

            audioSource.PlayOneShot(spawnSound);
        }

        protected override void Despawn()
        {
            if (!EnemyIsOperative<GameMakerEnemy>())
                return;

            if (PlayerController.Singleton.IsInCatchSequence)
                return;

            EnemyManager.Singleton.UnmarkAsOperative(this);

            PlayerController.Singleton.ConsumeItem("OperativeSystem");
            HideDesktop();

            foreach (Transform t in transform)
                t.gameObject.SetActive(false);

            audioSource.PlayOneShot(despawnSound);
        }

        public void ShowDesktop()
        {
            DesktopEnabled = true;
            PlayerController.Singleton.BlockAll();
            Cursor.lockState = CursorLockMode.None;
            screen.SetActive(true);
        }

        public void HideDesktop()
        {
            DesktopEnabled = false;
            PlayerController.Singleton.UnblockAll();
            Cursor.lockState = CursorLockMode.Locked;
            screen.SetActive(false);
        }

        public void CatchPlayer()
        {
            StartCoroutine(nameof(CatchPlayerCoroutine));
            Cursor.lockState = CursorLockMode.Locked;
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
            startButton.onClick.AddListener(HideDesktop);
            RoomManager.Singleton.OnRoomGenerated.AddListener(OnRoomGenerated);
        }

        private void OnRoomGenerated(Room room)
        {
            if (room is not GameMakerRoom)
                Despawn();
        }
    }
}
