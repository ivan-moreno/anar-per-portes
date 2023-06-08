namespace AnarPerPortes
{
    public sealed class GameSettings
    {
        public float Contrast { get; set; } = 0f;
        public float Saturation { get; set; } = 0f;
        public float FieldOfView { get; set; } = 70f;
        public float MouseSensitivity { get; set; } = 150f;
        public float Volume { get; set; } = 1f;
        public bool EnableEnemyTips { get; set; } = true;
        public bool EnableFullscreen { get; set; } = true;
        public bool EnableVSync { get; set; } = true;
        public bool EnableSmaa { get; set; } = true;
        public bool EnablePostProcessing { get; set; } = true;
        public bool EnableSubtitles { get; set; } = false;
        public bool EnableLargeSubtitles { get; set; } = false;
        public bool EnableRoomBrighten { get; set; } = false;
        public bool EnableVisionMotion { get; set; } = true;
        public bool EnableOpenDyslexicFont { get; set; } = false;
        public bool EnableFlamboyantGraphics { get; set; } = false;
        public bool EnableSpeedrunMode { get; set; } = false;
        public bool EnableHallucinations { get; set; } = true;
    }
}
