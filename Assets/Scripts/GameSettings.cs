using UnityEngine;

namespace AnarPerPortes
{
    /// <summary>
    /// Game properties that can be manipulated by the Player.
    /// </summary>
    public class GameSettings
    {
        public float HMouseSensitivity { get; set; } = 150f;
        public float VMouseSensitivity { get; set; } = 150f;
        public KeyCode InteractKey { get; set; } = KeyCode.E;
        public float Volume { get; set; } = 1f;
        public SubtitlesSetting SubtitlesSetting { get; set; } = SubtitlesSetting.DialogAndSoundEffects;
        public bool LargeSubtitles { get; set; } = false;
        public bool EnableVisionMotion { get; set; } = true;
        public bool EnableFlamboyantSilhouettes { get; set; } = true;
        public bool EnableLightMode { get; set; } = true;
        public EnemyTipSetting EnemyTipSetting { get; set; } = EnemyTipSetting.ShowOnFirstEncounterAndWhenCaught;
    }
}
