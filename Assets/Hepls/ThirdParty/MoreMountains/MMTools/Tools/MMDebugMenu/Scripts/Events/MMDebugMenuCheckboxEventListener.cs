using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMDCheckboxPressedEvent : UnityEvent<bool> { }
    [Serializable]
    public class MMDCheckboxTrueEvent : UnityEvent { }
    [Serializable]
    public class MMDCheckboxFalseEvent : UnityEvent { }
    public class MMDebugMenuCheckboxEventListener : MonoBehaviour
    {
        [Header("Events")]
        public string CheckboxEventName = "CheckboxEventName";
        public MMDCheckboxPressedEvent MMDPressedEvent;
        public MMDCheckboxTrueEvent MMDTrueEvent;
        public MMDCheckboxFalseEvent MMDFalseEvent;

        [Header("Test")]
        public bool TestValue = true;
        [MMInspectorButton("TestSetValue")]
        public bool TestSetValueButton;
        protected virtual void TestSetValue()
        {
            MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, TestValue, MMDebugMenuCheckboxEvent.EventModes.SetCheckbox);
        }
        protected virtual void OnMMDebugMenuCheckboxEvent(string checkboxNameEvent, bool value, MMDebugMenuCheckboxEvent.EventModes eventMode)
        {
            if ((eventMode == MMDebugMenuCheckboxEvent.EventModes.FromCheckbox) && (checkboxNameEvent == CheckboxEventName))
            {
                if (MMDPressedEvent != null)
                {
                    MMDPressedEvent.Invoke(value);
                }

                if (value)
                {
                    if (MMDTrueEvent != null)
                    {
                        MMDTrueEvent.Invoke();
                    }
                }
                else
                {
                    if (MMDFalseEvent != null)
                    {
                        MMDFalseEvent.Invoke();
                    }
                }
            }
        }
        public virtual void OnEnable()
        {
            MMDebugMenuCheckboxEvent.Register(OnMMDebugMenuCheckboxEvent);
        }
        public virtual void OnDisable()
        {
            MMDebugMenuCheckboxEvent.Unregister(OnMMDebugMenuCheckboxEvent);
        }
    }
}
