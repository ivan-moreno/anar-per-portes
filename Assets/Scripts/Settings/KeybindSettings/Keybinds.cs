using UnityEngine;

namespace AnarPerPortes
{
    public sealed class Keybinds
    {
        public KeyCode MoveForward { get; set; } = KeyCode.W;
        public KeyCode MoveBackward { get; set; } = KeyCode.S;
        public KeyCode MoveLeft { get; set; } = KeyCode.A;
        public KeyCode MoveRight { get; set; } = KeyCode.D;
        public KeyCode LookUp { get; set; } = KeyCode.UpArrow;
        public KeyCode LookDown { get; set; } = KeyCode.DownArrow;
        public KeyCode LookLeft { get; set; } = KeyCode.LeftArrow;
        public KeyCode LookRight { get; set; } = KeyCode.RightArrow;
        public KeyCode PrimaryAction { get; set; } = KeyCode.Mouse0;
        public KeyCode SecondaryAction { get; set; } = KeyCode.Mouse1;
        public KeyCode Interact { get; set; } = KeyCode.E;
        public KeyCode Pause { get; set; } = KeyCode.Escape;
    }
}
