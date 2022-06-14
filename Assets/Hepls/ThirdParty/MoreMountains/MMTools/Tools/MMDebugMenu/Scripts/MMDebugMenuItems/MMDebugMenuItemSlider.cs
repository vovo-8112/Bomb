using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugMenuItemSlider : MonoBehaviour
    {
        public enum Modes { Float, Int }

        [Header("Bindings")]
        public Modes Mode = Modes.Float;
        public Slider TargetSlider;
        public Text SliderText;
        public Text SliderValueText;
        public Image SliderKnob;
        public Image SliderLine;
        public float RemapZero = 0f;
        public float RemapOne = 1f;
        public string SliderEventName = "Checkbox";
        [MMReadOnly]
        public float SliderValue;
        [MMReadOnly]
        public int SliderValueInt;

        protected bool _valueSetThisFrame = false;
        protected bool _listening = false;
        protected virtual void Awake()
        {
            TargetSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        }
        public void ValueChangeCheck()
        {
            if (_valueSetThisFrame)
            {
                _valueSetThisFrame = false;
                return;
            }

            bool valueChanged = true;

            SliderValue = MMMaths.Remap(TargetSlider.value, 0f, 1f, RemapZero, RemapOne);

            if (Mode == Modes.Int)
            {
                SliderValue = Mathf.Round(SliderValue);
                if (SliderValue == SliderValueInt)
                {
                    valueChanged = false;
                }
                SliderValueInt = (int)SliderValue;
            }

            if (valueChanged)
            {
                UpdateValue(SliderValue);
            }

            TriggerSliderEvent(SliderValue);
        }

        protected virtual void UpdateValue(float newValue)
        {
            SliderValueText.text = (Mode == Modes.Int) ? newValue.ToString() : newValue.ToString("F3");
        }
        protected virtual void TriggerSliderEvent(float value)
        {
            MMDebugMenuSliderEvent.Trigger(SliderEventName, value, MMDebugMenuSliderEvent.EventModes.FromSlider);
        }
        protected virtual void OnMMDebugMenuSliderEvent(string sliderEventName, float value, MMDebugMenuSliderEvent.EventModes eventMode)
        {
            if ((eventMode == MMDebugMenuSliderEvent.EventModes.SetSlider)
                    && (sliderEventName == SliderEventName))
            {
                _valueSetThisFrame = true;
                TargetSlider.value = MMMaths.Remap(value, RemapZero, RemapOne, 0f, 1f);
                UpdateValue(value);
            }
        }
        public virtual void OnEnable()
        {
            if (!_listening)
            {
                MMDebugMenuSliderEvent.Register(OnMMDebugMenuSliderEvent);
                _listening = true;
            }            
        }
        public virtual void OnDestroy()
        {
            _listening = false;
            MMDebugMenuSliderEvent.Unregister(OnMMDebugMenuSliderEvent);
        }
    }
}