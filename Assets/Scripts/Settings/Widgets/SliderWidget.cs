using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    public class SliderWidget : Widget
    {
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private Slider slider;
        private Action<float> targetSetting;

        public SliderWidget Initialize(string title, float value, float minValue, float maxValue)
        {
            titleLabel.text = title;
            valueLabel.text = value.ToString("0");
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = value;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            GameSettingsManager.Singleton.OnApplySettings.AddListener(OnApplySettings);
            return this;
        }

        public SliderWidget WithWholeNumbers()
        {
            slider.wholeNumbers = true;
            return this;
        }

        public SliderWidget WithTarget(Action<float> targetSetting)
        {
            this.targetSetting = targetSetting;
            return this;
        }

        private void OnSliderValueChanged(float value)
        {
            valueLabel.text = value.ToString("0");
        }

        private void OnApplySettings()
        {
            targetSetting?.Invoke(slider.value);
        }
    }
}
