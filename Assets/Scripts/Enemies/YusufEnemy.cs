using System.Collections;
using UnityEngine;

namespace AnarPerPortes
{
    public class YusufEnemy : Enemy
    {
        public static bool EnemyIsActive { get; private set; } = false;

        public override bool EnemyTipWasDisplayed
        {
            get => enemyTipWasDisplayed;
            set => enemyTipWasDisplayed = value;
        }

        private static bool enemyTipWasDisplayed = false;

        [SerializeField] private AudioClip jumpscareSound;
        private AudioSource audioSource;
        private Transform model;

        private void Start()
        {
            var isleRoom = Game.RoomManager.LastLoadedRoom as IsleRoom;
            transform.SetPositionAndRotation(isleRoom.YusufSpawnPoint.position, isleRoom.YusufSpawnPoint.rotation);
            audioSource = GetComponent<AudioSource>();
            model = transform.GetChild(0);
            EnemyIsActive = true;
            Game.SubtitleManager.PushSubtitle("[YUSUF] Mi dirección es el Observatorio. Corto.", SubtitleCategory.Dialog, SubtitleSource.Hostile);

            isleRoom.OnIncorrectDoorOpened.AddListener(CatchPlayer);
            isleRoom.OnDoorOpened.AddListener(Despawn);
        }

        private void CatchPlayer()
        {
            audioSource.PlayOneShot(jumpscareSound);
            Game.SubtitleManager.PushSubtitle("(Yusuf grita)", SubtitleCategory.SoundEffect, SubtitleSource.Hostile);
            PlayerController.Instance.CanMove = false;
            PlayerController.Instance.CanLook = false;
            StartCoroutine(nameof(CatchPlayerEnumerator));
        }

        private IEnumerator CatchPlayerEnumerator()
        {
            yield return new WaitForSeconds(1f);
            PlayerController.Instance.GetCaught("YUSUF ENDING", "Fuerzas Yusuf, Fuerzas Yusuf");
            audioSource.Play();
        }

        private void Despawn()
        {
            EnemyIsActive = false;
            Destroy(gameObject);
        }
    }
}
