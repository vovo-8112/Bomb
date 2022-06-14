﻿using UnityEngine;

namespace MoreMountains.Tools
{
    public struct MMDebugMenuCheckboxEvent
    {
        public enum EventModes { FromCheckbox, SetCheckbox }

        public delegate void Delegate(string checkboxEventName, bool value, EventModes eventMode = EventModes.FromCheckbox);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(string checkboxEventName, bool value, EventModes eventMode = EventModes.FromCheckbox)
        {
            OnEvent?.Invoke(checkboxEventName, value, eventMode);
        }
    }
}