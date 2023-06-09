using static AnarPerPortes.ShortUtils;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using AnarPerPortes.Rooms;

namespace AnarPerPortes.Enemies
{
    [AddComponentMenu("Anar per Portes/Enemies/Specimen 7 Enemy")]
    public class Specimen7Enemy : Enemy
    {
        public static UnityEvent<Specimen7Enemy> OnSpawn { get; } = new();
        [SerializeField] private string[] catchMessages;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float spawnDistance = 12f;
        [SerializeField][Min(0f)] private float advanceSpeed = 4f;
        [SerializeField][Min(1)] private int doorsUntilDespawn = 4;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;

        private static bool hasAlreadyAppeared = false;
        private float timeSinceSpawn;
        private int openedDoors = 0;

        public override void Spawn()
        {
            base.Spawn();
            audioSource = GetComponent<AudioSource>();

            var room = RoomManager.Singleton.LatestRoom.transform;
            transform.SetPositionAndRotation(room.position - room.forward * spawnDistance, room.rotation);
            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(new(1f, 0f, 0f, 0.5f), 0.5f);

            audioSource.Play(spawnSound);

            OnSpawn?.Invoke(this);
            RoomManager.Singleton.OnRoomGenerated.AddListener(OnRoomGenerated);
        }

        private void OnRoomGenerated(Room room)
        {
            if (isCatching)
                return;

            openedDoors++;

            if (openedDoors >= doorsUntilDespawn)
                Despawn();
        }

        protected override void Despawn()
        {
            if (isCatching)
                return;

            hasAlreadyAppeared = true;
            BlackoutManager.Singleton.PlayDoorOpen();
            BlurOverlayManager.Singleton.SetBlurSmooth(Color.clear, 0.5f);
            PlayerCollectTix(25, "Has evadido a Specimen 7");
            base.Despawn();
        }

        private void Update()
        {
            if (isInIntro)
                return;

            timeSinceSpawn += Time.deltaTime;

            if (EnemyIsOperative<SheepyEnemy>() || EnemyIsOperative<SangotEnemy>() || EnemyIsOperative<A90Enemy>())
                return;

            if (!hasAlreadyAppeared && timeSinceSpawn < 3f && !IsHardmodeEnabled())
                return;

            advanceSpeed += 0.2f * Time.deltaTime;

            if (advanceSpeed >= PlayerController.Singleton.WalkSpeed - 2f)
                advanceSpeed = PlayerController.Singleton.WalkSpeed - 2f;

            transform.Translate(advanceSpeed * Time.deltaTime * Vector3.forward, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            StartCoroutine(nameof(CatchPlayerCoroutine));
        }

        private IEnumerator CatchPlayerCoroutine()
        {
            if (isCatching)
                yield break;

            isCatching = true;
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
