using UnityEngine;

namespace AnarPerPortes
{
    /// <summary>
    /// Game properties that can be manipulated by the Player.
    /// </summary>
    public sealed class GameSettings
    {
        public EnemyTipSetting EnemyTipSetting { get; set; } = EnemyTipSetting.ShowWhenCaught;
        public SubtitlesSetting SubtitlesSetting { get; set; } = SubtitlesSetting.DialogAndSoundEffects;
        public float Volume { get; set; } = 1f;
        public float FieldOfView { get; set; } = 70f;
        public float HMouseSensitivity { get; set; } = 150f;
        public float VMouseSensitivity { get; set; } = 150f;
        public bool EnableSmaa { get; set; } = true;
        public bool EnablePostProcessing { get; set; } = true;
        public bool EnableLightMode { get; set; } = false;
        public bool EnableVisionMotion { get; set; } = true;
        public bool EnableLargeSubtitles { get; set; } = false;
        public bool EnableDyslexicFriendlyFont { get; set; } = false;
        public bool EnableFlamboyantGraphics { get; set; } = false;
    }
}
