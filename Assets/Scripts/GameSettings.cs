namespace AnarPerPortes
{
    /// <summary>
    /// Game properties that can be manipulated by the Player.
    /// </summary>
    public class GameSettings
    {
        public float HMouseSensitivity { get; set; } = 150f;
        public float VMouseSensitivity { get; set; } = 150f;
        public float Volume { get; set; } = 1f;
        public SubtitlesSetting SubtitlesSetting { get; set; } = SubtitlesSetting.DialogAndSoundEffects;
        public bool LargeSubtitles { get; set; } = true;
    }
}
