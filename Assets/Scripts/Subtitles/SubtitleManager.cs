using System.Collections;
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
        private const int maxSubtitles = 8;

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

        public void PushSubtitle(SoundResource soundResource)
        {
            var calculatedDuration = Mathf.Clamp(soundResource.AudioClip.length + 1f, 3f, 16f);

            if (soundResource.SubtitleText.StartsWith("("))
                calculatedDuration = 4;

            PushSubtitle(
                soundResource.SubtitleText,
                calculatedDuration,
                soundResource.SubtitleTeam);
        }

        public void PushSubtitle(string message, float duration = 4f, Team team = Team.Common)
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

            //TODO: Fix breaking words, allow for more lines
            if (message.Length > 128)
            {
                var lineA = message[..128];
                var lineB = message[128..];
                message = string.Concat(lineA, "\n", lineB);
            }

            subtitleMessage.Initialize(message, duration, GetSourceColor(team));
        }

        private void Update()
        {
            if (subtitleMessagesGroup.childCount > maxSubtitles)
                Destroy(subtitleMessagesGroup.GetChild(0).gameObject);
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}
