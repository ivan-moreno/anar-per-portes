using System;
using UnityEngine;

namespace AnarPerPortes
{
    /// <summary>
    /// Game properties that can be manipulated by the Player.
    /// </summary>
    public sealed class GameSettings
    {
        public EnemyTipSetting EnemyTipSetting { get; set; } = EnemyTipSetting.ShowOnFirstEncounterAndWhenCaught;
        [Obsolete] public int SubtitlesSetting { get; set; } = 2;
        public float Volume { get; set; } = 1f;
        public float FieldOfView { get; set; } = 70f;
        public float HMouseSensitivity { get; set; } = 150f;
        public float VMouseSensitivity { get; set; } = 150f;
        public bool EnableSmaa { get; set; } = true;
        public bool EnablePostProcessing { get; set; } = true;
        public bool EnableSubtitles { get; set; } = false;
        public bool EnableLightMode { get; set; } = false;
        public bool EnableVisionMotion { get; set; } = true;
        public bool EnableLargeSubtitles { get; set; } = false;
        public bool EnableDyslexicFriendlyFont { get; set; } = false;
        public bool EnableFlamboyantGraphics { get; set; } = false;
    }
}
