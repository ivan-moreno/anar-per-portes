using UnityEngine;

namespace AnarPerPortes
{
    /// <summary>
    /// A Singleton class that represents the game's state.
    /// </summary>
    [AddComponentMenu("Anar per Portes/Game")]
    public class Game : MonoBehaviour
    {
        public static GameSettings Settings { get; private set; } = new();
        public static SubtitleManager Subtitles { get; set; }
        public static RoomManager Rooms { get; set; }

        private void Awake()
        {
            Subtitles = GetComponent<SubtitleManager>();
            Rooms = GetComponent<RoomManager>();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            CheckForMultipleInstances();
        }

        private void Reset()
        {
            CheckForMultipleInstances();
        }

#if UNITY_EDITOR
        private void CheckForMultipleInstances()
        {
            // Check for multiple Game MonoBehaviours.
            var gameScriptCount = FindObjectsOfType<Game>(true).Length;

            if (gameScriptCount > 1)
                Debug.LogError($"There can only be one Game MonoBehaviour present on the Scene. There are currently {gameScriptCount}.");
        }
#endif

    }
}
