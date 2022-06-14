using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMDButtonPressedEvent : UnityEvent
    {
    }
    public class MMDebugMenuButtonEventListener : MonoBehaviour
    {
        [Header("Event")]
        public string ButtonEventName = "Button";
        public MMDButtonPressedEvent MMDEvent;

        [Header("Test")]
        public bool TestValue = true;
        [MMInspectorButton("TestSetValue")]
        public bool TestSetValueButton;
        protected virtual void TestSetValue()
        {
            MMDebugMenuButtonEvent.Trigger(ButtonEventName, TestValue, MMDebugMenuButtonEvent.EventModes.SetButton);
        }
        protected virtual void OnMMDebugMenuButtonEvent(string buttonEventName, bool value, MMDebugMenuButtonEvent.EventModes eventMode)
        {
            if ((eventMode == MMDebugMenuButtonEvent.EventModes.FromButton) && (buttonEventName == ButtonEventName))
            {
                if (MMDEvent != null)
                {
                    MMDEvent.Invoke();
                }
            }
        }
        public virtual void OnEnable()
        {
            MMDebugMenuButtonEvent.Register(OnMMDebugMenuButtonEvent);
        }
        public virtual void OnDisable()
        {
            MMDebugMenuButtonEvent.Unregister(OnMMDebugMenuButtonEvent);
        }
    }
}
