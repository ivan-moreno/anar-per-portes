using AnarPerPortes.Rooms;
using System.Collections;
using UnityEngine;
using static AnarPerPortes.ShortUtils;

namespace AnarPerPortes
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Anar per Portes/Entities/Bouser Jr Entity")]
    public class BouserJrEntity : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float turnSpeed = 90f;
        [SerializeField] private float wakeBouserTime = 15f;

        [Header("Audio")]
        [SerializeField] private SoundResource introVoice;
        [SerializeField] private AudioClip music;

        private Animator animator;
        private Transform model;
        private AudioSource audioSource;
        private float timeSinceIntro = 0f;
        private bool closeToPlayer = false;

        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
            model = animator.transform;
            audioSource = GetComponent<AudioSource>();
            Intro();
        }

        private void Intro()
        {
            audioSource.PlayOneShot(introVoice);
            animator.Play("Funkin");
            AudioManager.Singleton.SetVolume(1f);
            AudioManager.Singleton.PlayMusic(music);
        }

        void Despawn()
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);

            if (closeToPlayer)
            {
                var direction = Vector3.Normalize(transform.position - PlayerPosition());
                var lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
            }
            else
                transform.LookAt(PlayerPosition());

            timeSinceIntro += Time.deltaTime;

            if (timeSinceIntro > wakeBouserTime)
                StartCoroutine(nameof(WakeBouserCoroutine));
        }

        void FixedUpdate()
        {
            closeToPlayer = DistanceToPlayer(transform) < 4f;
        }

        private IEnumerator WakeBouserCoroutine()
        {
            speed = 0f;
            var room = LatestRoom() as BouserRoom;
            transform.LookAt(room.BouserSpawnPoint);
            room.WakeUpBouser();
            AudioManager.Singleton.SetTargetVolume(0f);
            yield return new WaitForSeconds(5.3f);
            speed = 16f;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, room.BouserSpawnPoint.position) < 3f);
            Despawn();
        }
    }
}
