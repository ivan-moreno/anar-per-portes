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
        private AudioSource audioSource;
        private Transform model;

        private void Start()
        {
            var isleRoom = RoomManager.Singleton.LastLoadedRoom as IsleRoom;
            transform.SetPositionAndRotation(isleRoom.YusufSpawnPoint.position, isleRoom.YusufSpawnPoint.rotation);
            audioSource = GetComponent<AudioSource>();
            model = transform.GetChild(0);
            EnemyIsActive = true;
            audioSource.PlayOneShot(walkieTalkieAlertSound);
            SubtitleManager.Singleton.PushSubtitle("[YUSUF] Mi dirección es el Observatorio. Corto.", SubtitleCategory.Dialog, SubtitleSource.Hostile);

            isleRoom.OnIncorrectDoorOpened.AddListener(CatchPlayer);
            isleRoom.OnDoorOpened.AddListener(Despawn);
        }

        private void CatchPlayer()
        {
            audioSource.PlayOneShot(jumpscareSound);
            SubtitleManager.Singleton.PushSubtitle("(Yusuf grita)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
            PlayerController.Singleton.CanMove = false;
            PlayerController.Singleton.CanLook = false;
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
