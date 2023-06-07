using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/Yusuf Enemy")]
    public class YusufEnemy : Enemy
    {
        public static bool EnemyIsActive { get; set; } = false;

        [Header("Sound")]
        [SerializeField] private SoundResource walkieTalkieAlertSound;
        [SerializeField] private SoundResource jumpscareSound;
        [SerializeField] private SoundResource[] bunkerTargetSounds;
        [SerializeField] private SoundResource[] lighthouseTargetSounds;
        [SerializeField] private SoundResource[] observatoryTargetSounds;
        [SerializeField] private SoundResource[] warehouseTargetSounds;

        private void Start()
        {
            EnemyIsActive = true;
            CacheComponents();

            var isleRoom = RoomManager.Singleton.LastLoadedRoom as IsleRoom;
            var targetPos = isleRoom.IncorrectDoor.transform.position + (isleRoom.IncorrectDoor.transform.forward * 4f);
            transform.position = targetPos;
            audioSource.PlayOneShot(walkieTalkieAlertSound);

            var rng = Random.Range(0, 4);

            if (rng == 0)
                audioSource.PlayOneShot(bunkerTargetSounds.RandomItem());
            else if (rng == 1)
                audioSource.PlayOneShot(lighthouseTargetSounds.RandomItem());
            else if (rng == 2)
                audioSource.PlayOneShot(observatoryTargetSounds.RandomItem());
            else
                audioSource.PlayOneShot(warehouseTargetSounds.RandomItem());

            isleRoom.OnDoorOpened.AddListener(Despawn);
            isleRoom.OnIncorrectDoorOpened.AddListener(CatchPlayer);
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
            PlayerController.Singleton.BlockAll();
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
    }
}
