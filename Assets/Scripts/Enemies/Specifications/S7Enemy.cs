using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Enemies/S7 Enemy")]
    public class S7Enemy : Enemy
    {
        public static bool IsOperative { get; set; } = false;
        public static UnityEvent<S7Enemy> OnSpawn { get; } = new();
        [SerializeField] private string[] catchMessages;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float spawnDistance = 12f;
        [SerializeField][Min(0f)] private float advanceSpeed = 4f;
        [SerializeField][Min(1)] private int doorsUntilDespawn = 4;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;

        private int openedDoors = 0;

        private void Start()
        {
            IsOperative = true;
            audioSource = GetComponent<AudioSource>();

            var room = RoomManager.Singleton.LastLoadedRoom.transform;
            transform.SetPositionAndRotation(room.position - (room.forward * spawnDistance), room.rotation);
            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(new(1f, 0f, 0f, 0.5f), 0.5f);

            audioSource.Play(spawnSound);

            OnSpawn?.Invoke(this);
            RoomManager.Singleton.OnRoomGenerated.AddListener(OnRoomGenerated);
        }

        private void OnRoomGenerated(Room room)
        {
            openedDoors++;

            if (openedDoors >= doorsUntilDespawn)
                Despawn();
        }

        private void Despawn()
        {
            IsOperative = false;
            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 0.5f);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (SheepyEnemy.IsOperative || SangotEnemy.IsOperative || A90Enemy.IsOperative)
                return;

            transform.Translate(Vector3.forward * advanceSpeed * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            StartCoroutine(nameof(CatchCoroutine));
        }

        private IEnumerator CatchCoroutine()
        {
            advanceSpeed = 0f;
            GetComponent<BoxCollider>().enabled = false;
            audioSource.Stop();
            PlayerController.Singleton.BlockAll();
            FadeScreenManager.Singleton.Display(catchMessages.RandomItem(), GameManager.Singleton.RestartLevel);
            yield return new WaitForSeconds(1f);
            Despawn();
        }
    }
}
