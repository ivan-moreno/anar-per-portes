using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Widgets/Slider Widget")]
    public class SliderWidget : Widget
    {
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private Slider slider;
        private string valueLabelFormat = "{0}";
        private Action<float> targetSetting;

        public SliderWidget Initialize(string title, float value, float minValue, float maxValue)
        {
            titleLabel.text = title;
            valueLabel.text = string.Format(valueLabelFormat, value.ToString("0"));
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

        public SliderWidget WithValueFormat(string format)
        {
            valueLabelFormat = format;
            valueLabel.text = string.Format(valueLabelFormat, slider.value.ToString("0"));
            return this;
        }

        public SliderWidget WithTarget(Action<float> targetSetting)
        {
            this.targetSetting = targetSetting;
            return this;
        }

        private void OnSliderValueChanged(float value)
        {
            valueLabel.text = string.Format(valueLabelFormat, value.ToString("0"));
        }

        private void OnApplySettings()
        {
            targetSetting?.Invoke(slider.value);
        }
    }
}
