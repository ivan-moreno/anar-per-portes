using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Game Maker/Game Maker Sound Mixer")]
    [RequireComponent(typeof(AudioSource))]
    public class GameMakerSoundMixer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button soundMixerButton;

        [Header("Stats")]
        [SerializeField][Min(0f)] private float minSpawnTime = 30f;
        [SerializeField][Min(0f)] private float maxSpawnTime = 60f;
        [SerializeField][Min(0.01f)] private float warningDuration = 3f;

        [Header("Audio")]
        [SerializeField] private SoundResource spawnSound;

        private GameMakerManager manager;
        private AudioSource audioSource;
        private bool isFunctioning = false;
        private float spawnTime;
        private float warningTime;

        private void Start()
        {
            manager = GetComponentInParent<GameMakerManager>();
            audioSource = GetComponent<AudioSource>();
            spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            soundMixerButton.onClick.AddListener(Despawn);
            PauseManager.Singleton.OnPauseChanged.AddListener(OnPauseChanged);
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }

        private void Update()
        {
            if (isFunctioning)
                UpdateFunctioning();
            else
                UpdateNonFunctioning();
        }

        private void UpdateNonFunctioning()
        {
            spawnTime -= Time.deltaTime;

            if (spawnTime <= 0f)
                Spawn();
        }

        private void UpdateFunctioning()
        {
            warningTime += Time.deltaTime;

            if (warningTime >= warningDuration)
            {
                audioSource.Stop();
                manager.CatchPlayer();
            }
        }

        private void Spawn()
        {
            if (isFunctioning)
                return;

            isFunctioning = true;
            audioSource.Play(spawnSound);
        }

        private void Despawn()
        {
            if (!isFunctioning)
                return;

            isFunctioning = false;
            warningTime = 0f;
            spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            audioSource.Stop();
        }
    }
}
