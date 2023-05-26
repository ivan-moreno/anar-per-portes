using UnityEngine;
using UnityEngine.Rendering;

namespace AnarPerPortes
{
    /// <summary>
    /// A Singleton class that represents the game's state.
    /// </summary>
    [RequireComponent(typeof(SubtitleManager))]
    [RequireComponent(typeof(RoomManager))]
    [RequireComponent(typeof(InteractionManager))]
    [RequireComponent(typeof(ItemManager))]
    [RequireComponent(typeof(EnemyManager))]
    [RequireComponent(typeof(EnemyTipManager))]
    [RequireComponent(typeof(CaughtManager))]
    [RequireComponent(typeof(AccessibilityFontManager))]
    [RequireComponent(typeof(Volume))]
    [AddComponentMenu("Anar per Portes/Game")]
    public class Game : MonoBehaviour
    {
        public static GameSettings Settings { get; private set; } = new();
        public static SubtitleManager SubtitleManager { get; private set; }
        public static RoomManager RoomManager { get; private set; }
        public static InteractionManager InteractionManager { get; private set; }
        public static ItemManager ItemManager { get; private set; }
        public static EnemyManager EnemyManager { get; private set; }
        public static EnemyTipManager EnemyTipManager { get; private set; }
        public static CaughtManager CaughtManager { get; private set; }
        public static AccessibilityFontManager AccessibilityFontManager { get; private set; }
        public static Volume GlobalVolume { get; private set; }

        private void Awake()
        {
            CheckForMultipleInstances();

            SubtitleManager = GetComponent<SubtitleManager>();
            RoomManager = GetComponent<RoomManager>();
            InteractionManager = GetComponent<InteractionManager>();
            ItemManager = GetComponent<ItemManager>();
            EnemyManager = GetComponent<EnemyManager>();
            EnemyTipManager = GetComponent<EnemyTipManager>();
            CaughtManager = GetComponent<CaughtManager>();
            AccessibilityFontManager = GetComponent<AccessibilityFontManager>();
            GlobalVolume = GetComponent<Volume>();
        }

        private void Start()
        {
            // TODO: Consider this
            //DontDestroyOnLoad(gameObject);

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
            var gameScripts = FindObjectsOfType<Game>(true);

            if (gameScripts.Length > 1)
            {
                foreach (var script in gameScripts)
                {
                    if (script == this)
                        continue;

                    script.enabled = false;
                    Destroy(script.gameObject);
                }
            }    
        }
    }
}
