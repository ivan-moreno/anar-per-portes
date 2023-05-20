using UnityEngine;
using UnityEngine.Rendering;

namespace AnarPerPortes
{
    /// <summary>
    /// A Singleton class that represents the game's state.
    /// </summary>
    [AddComponentMenu("Anar per Portes/Game")]
    public class Game : MonoBehaviour
    {
        public static GameSettings Settings { get; private set; } = new();
        public static SubtitleManager SubtitleManager { get; private set; }
        public static RoomManager RoomManager { get; private set; }
        public static InteractionManager InteractionManager { get; private set; }
        public static ItemManager ItemManager { get; private set; }
        public static Volume GlobalVolume { get; private set; }

        private void Awake()
        {
            SubtitleManager = GetComponent<SubtitleManager>();
            RoomManager = GetComponent<RoomManager>();
            InteractionManager = GetComponent<InteractionManager>();
            ItemManager = GetComponent<ItemManager>();
            GlobalVolume = GetComponent<Volume>();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            CheckForMultipleInstances();

            // TODO: Transfer responsibility to another class, check for setting changes.
            if (Settings.EnableLightMode)
                RenderSettings.ambientLight = new(0.5f, 0.5f, 0.5f);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
                Application.Quit();
        }

        private void Reset()
        {
            CheckForMultipleInstances();
        }

        private void CheckForMultipleInstances()
        {
            // Check for multiple Game MonoBehaviours.
            var gameScriptCount = FindObjectsOfType<Game>(true).Length;

            if (gameScriptCount > 1)
                Debug.LogError($"There can only be one Game MonoBehaviour present on the Scene. There are currently {gameScriptCount}.");
        }
    }
}
