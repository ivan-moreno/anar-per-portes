using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Subtitle Manager")]
    public sealed class SubtitleManager : MonoBehaviour
    {
        public static SubtitleManager Singleton { get; private set; }
        [SerializeField] private Transform subtitleMessagesGroup;
        [SerializeField] private GameObject subtitleMessagePrefab;

        // TODO: Upgrade to readonly classes
        private static readonly Dictionary<Team, Color> sourceColors = new()
        {
            { Team.Common, new Color(0.94f, 0.94f, 1f) },
            { Team.Friendly, new Color(0.3f, 0.8f, 0.8f) },
            { Team.Hostile, new Color(0.9f, 0.3f, 0.2f) }
        };

        private static Color GetSourceColor(Team source)
        {
            return sourceColors.TryGetValue(source, out var color) ? color : Color.white;
        }

        public void PushSubtitle(string message, Team team = Team.Common)
        {
            if (!GameSettingsManager.Singleton.CurrentSettings.EnableSubtitles)
                return;

            if (string.IsNullOrEmpty(message))
                return;

            var instance = Instantiate(subtitleMessagePrefab, subtitleMessagesGroup);
            var hasSubtitleMessage = instance.TryGetComponent(out SubtitleMessage subtitleMessage);

            if (!hasSubtitleMessage)
            {
                Debug.LogError("Subtitle Prefab has no SubtitleMessage script attached to it.");
                return;
            }

            subtitleMessage.Initialize(message, 4f, GetSourceColor(team));
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}
