using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    /// <summary>
    /// Handles instantiation and initialization of subtitles.
    /// </summary>
    [AddComponentMenu("Anar per Portes/Managers/Subtitle Manager")]
    public sealed class SubtitleManager : MonoBehaviour
    {
        public static SubtitleManager Singleton { get; private set; }
        [SerializeField] private Transform subtitleMessagesGroup;
        [SerializeField] private GameObject subtitleMessagePrefab;

        // TODO: Upgrade to readonly classes
        private static readonly Dictionary<SubtitleSource, Color> sourceColors = new()
        {
            { SubtitleSource.Common, new Color(0.94f, 0.94f, 1f) },
            { SubtitleSource.Friendly, new Color(0.3f, 0.8f, 0.8f) },
            { SubtitleSource.Hostile, new Color(0.9f, 0.3f, 0.2f) }
        };

        /// <summary>
        /// Generates a <see cref="SubtitleMessage"/> instance and sets its message and color depending on the <paramref name="source"/>.
        /// </summary>
        public void PushSubtitle(
            string message,
            SubtitleCategory category = SubtitleCategory.Dialog,
            SubtitleSource source = SubtitleSource.Common)
        {
            // Do not generate subtitles if the setting is Disabled.
            if (GameSettingsManager.Singleton.CurrentSettings.SubtitlesSetting is SubtitlesSetting.Disabled)
                return;

            // Do not generate non-dialog subtitles if the setting is Dialog Only.
            if (GameSettingsManager.Singleton.CurrentSettings.SubtitlesSetting is SubtitlesSetting.DialogOnly && category != SubtitleCategory.Dialog)
                return;

            var instance = Instantiate(subtitleMessagePrefab, subtitleMessagesGroup);
            var hasSubtitleMessage = instance.TryGetComponent(out SubtitleMessage subtitleMessage);

            if (!hasSubtitleMessage)
            {
                Debug.LogError("Subtitle Prefab has no SubtitleMessage script attached to it.");
                return;
            }

            subtitleMessage.Initialize(message, 4f, GetSourceColor(source));
        }

        private static Color GetSourceColor(SubtitleSource source)
        {
            return sourceColors.TryGetValue(source, out var color) ? color : Color.white;
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}
