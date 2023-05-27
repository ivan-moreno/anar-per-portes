using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Managers/Keybind Manager")]
    public sealed class KeybindManager : MonoBehaviour
    {
        public static KeybindManager Singleton { get; private set; }
        public Keybinds CurrentKeybinds { get; private set; }
        public float HLookSpeed { get; private set; } = 2f;
        public float VLookSpeed { get; private set; } = 2f;

        public Vector3 GetInputMotion()
        {
            return new(
                x: GetHorizontalMotionPolarity(),
                y: 0f,
                z: GetFrontalMotionPolarity());
        }

        public Vector3 GetInputLook()
        {
            return new(
                x: GetHorizontalLookPolarity(),
                y: GetVerticalLookPolarity(),
                z: 0f);
        }

        private int GetFrontalMotionPolarity()
        {
            return CurrentKeybinds is null ? 0 : GetPolarity(CurrentKeybinds.MoveForward, CurrentKeybinds.MoveBackward);
        }

        private int GetHorizontalMotionPolarity()
        {
            return CurrentKeybinds is null ? 0 : GetPolarity(CurrentKeybinds.MoveRight, CurrentKeybinds.MoveLeft);
        }

        private int GetHorizontalLookPolarity()
        {
            return CurrentKeybinds is null ? 0 : GetPolarity(CurrentKeybinds.LookRight, CurrentKeybinds.LookLeft);
        }

        private int GetVerticalLookPolarity()
        {
            return CurrentKeybinds is null ? 0 : GetPolarity(CurrentKeybinds.LookUp, CurrentKeybinds.LookDown);
        }

        private int GetPolarity(KeyCode positiveKey, KeyCode negativeKey)
        {
            return Input.GetKey(positiveKey) && Input.GetKey(negativeKey) ? 0
                : Input.GetKey(positiveKey) ? 1
                : Input.GetKey(negativeKey) ? -1
                : 0;
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            LoadSerializedKeybinds();
        }

        //TODO: Save and load keybinds on disk file.
        void LoadSerializedKeybinds()
        {
            CurrentKeybinds = new();
        }
    }
}
