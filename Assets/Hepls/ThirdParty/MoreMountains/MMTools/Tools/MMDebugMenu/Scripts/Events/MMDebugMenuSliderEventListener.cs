using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMDSliderValueChangedEvent : UnityEvent<float> { }
    public class MMDebugMenuSliderEventListener : MonoBehaviour
    {
        [Header("Events")]
        public string SliderEventName = "SliderEventName";
        public MMDSliderValueChangedEvent MMDValueChangedEvent;

        [Header("Test")]
        [Range(0f, 1f)]
        public float TestValue = 1f;
        [MMInspectorButton("TestSetValue")]
        public bool TestSetValueButton;
        protected virtual void TestSetValue()
        {
            MMDebugMenuSliderEvent.Trigger(SliderEventName, TestValue, MMDebugMenuSliderEvent.EventModes.SetSlider);
        }
        protected virtual void OnMMDebugMenuSliderEvent(string sliderEventName, float value, MMDebugMenuSliderEvent.EventModes eventMode)
        {
            if ( (eventMode == MMDebugMenuSliderEvent.EventModes.FromSlider) 
                    && (sliderEventName == SliderEventName))
            {
                if (MMDValueChangedEvent != null)
                {
                    MMDValueChangedEvent.Invoke(value);
                }
            }
        }
        public virtual void OnEnable()
        {
            MMDebugMenuSliderEvent.Register(OnMMDebugMenuSliderEvent);
        }
        public virtual void OnDisable()
        {
            MMDebugMenuSliderEvent.Unregister(OnMMDebugMenuSliderEvent);
        }
    }
}
