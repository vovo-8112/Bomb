﻿using UnityEngine;

namespace MoreMountains.Tools
{
    public struct MMDebugMenuSliderEvent
    {
        public enum EventModes { FromSlider, SetSlider }

        public delegate void Delegate(string sliderEventName, float value, EventModes eventMode = EventModes.FromSlider);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(string sliderEventName, float value, EventModes eventMode = EventModes.FromSlider)
        {
            OnEvent?.Invoke(sliderEventName, value, eventMode);
        }
    }
}