using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Widgets/Toggle Widget")]
    public class ToggleWidget : Widget
    {
        [SerializeField] private Toggle toggle;
        private Action<bool> targetSetting;

        public ToggleWidget Initialize(string title, bool value)
        {
            titleLabel.text = title;
            toggle.isOn = value;
            GameSettingsManager.Singleton.OnApplySettings.AddListener(OnApplySettings);
            return this;
        }

        public ToggleWidget WithTarget(Action<bool> targetSetting)
        {
            this.targetSetting = targetSetting;
            return this;
        }

        private void OnApplySettings()
        {
            targetSetting?.Invoke(toggle.isOn);
        }
    }
}
