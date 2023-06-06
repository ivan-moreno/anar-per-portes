using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Yusuf Enemy")]
    public class YusufEnemy : Enemy
    {
        public static bool EnemyIsActive { get; private set; } = false;

        public override bool EnemyTipWasDisplayed
        {
            get => enemyTipWasDisplayed;
            set => enemyTipWasDisplayed = value;
        }

        private static bool enemyTipWasDisplayed = false;

        [SerializeField] private AudioClip walkieTalkieAlertSound;
        [SerializeField] private AudioClip jumpscareSound;
        [SerializeField] private AudioClip[] bunkerTargetSounds;
        [SerializeField] private string[] bunkerTargetSoundSubtitles;
        [SerializeField] private AudioClip[] lighthouseTargetSounds;
        [SerializeField] private string[] lighthouseTargetSoundSubtitles;
        [SerializeField] private AudioClip[] observatoryTargetSounds;
        [SerializeField] private string[] observatoryTargetSoundSubtitles;
        [SerializeField] private AudioClip[] warehouseTargetSounds;
        [SerializeField] private string[] warehouseTargetSoundSubtitles;
        private AudioSource audioSource;
        private Transform model;

        private void Start()
        {
            var isleRoom = RoomManager.Singleton.LastLoadedRoom as IsleRoom;
            var targetPos = isleRoom.IncorrectDoor.transform.position + (isleRoom.IncorrectDoor.transform.forward * 4f);
            transform.position = targetPos;
            audioSource = GetComponent<AudioSource>();
            model = transform.GetChild(0);
            EnemyIsActive = true;
            audioSource.PlayOneShot(walkieTalkieAlertSound);

            var rng = Random.Range(0, 4);

            if (rng == 0)
                PlayRandomAudio(bunkerTargetSounds, bunkerTargetSoundSubtitles);
            else if (rng == 1)
                PlayRandomAudio(lighthouseTargetSounds, lighthouseTargetSoundSubtitles);
            else if (rng == 2)
                PlayRandomAudio(observatoryTargetSounds, observatoryTargetSoundSubtitles);
            else
                PlayRandomAudio(warehouseTargetSounds, warehouseTargetSoundSubtitles);

            isleRoom.OnIncorrectDoorOpened.AddListener(CatchPlayer);
            isleRoom.OnDoorOpened.AddListener(Despawn);
            PauseManager.Singleton.OnPauseChanged.AddListener(PauseChanged);
        }

        private void PauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void CatchPlayer()
        {
            transform.LookAt(PlayerController.Singleton.transform.position);
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(Yusuf grita)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
            PlayerController.Singleton.BlockMove();
            PlayerController.Singleton.BlockLook();
            PlayerController.Singleton.SetVisionTarget(transform, new Vector3(0f, 0f, 0f));
            EnemyIsActive = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(1f);
            CatchManager.Singleton.CatchPlayer("YUSUF ENDING", "Fuerzas Yusuf, Fuerzas Yusuf");
            audioSource.Play();
        }

        private void Despawn()
        {
            EnemyIsActive = false;
            Destroy(gameObject);
        }

        //TODO: Static method
        private void PlayRandomAudio(AudioClip[] audios, string[] subtitles)
        {
            var rngAudioIndex = Random.Range(0, audios.Length);
            var rngAudio = audios[rngAudioIndex];
            audioSource.PlayOneShot(rngAudio);
            SubtitleManager.Singleton.PushSubtitle(subtitles[rngAudioIndex], SubtitleCategory.Dialog, SubtitleSource.Hostile);
        }
    }
}
