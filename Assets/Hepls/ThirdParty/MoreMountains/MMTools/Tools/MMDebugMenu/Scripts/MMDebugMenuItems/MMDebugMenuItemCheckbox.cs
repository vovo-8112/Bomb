using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugMenuItemCheckbox : MonoBehaviour
    {
        [Header("Bindings")]
        public MMDebugMenuSwitch Switch;
        public Text SwitchText;
        public string CheckboxEventName = "Checkbox";

        protected bool _valueSetThisFrame = false;
        protected bool _listening = false;
        public virtual void TriggerCheckboxEvent()
        {
            if (_valueSetThisFrame)
            {
                _valueSetThisFrame = false;
                return;
            }
            MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, Switch.SwitchState, MMDebugMenuCheckboxEvent.EventModes.FromCheckbox);
        }
        public virtual void TriggerCheckboxEventTrue()
        {
            if (_valueSetThisFrame)
            {
                _valueSetThisFrame = false;
                return;
            }
            MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, true, MMDebugMenuCheckboxEvent.EventModes.FromCheckbox);
        }
        public virtual void TriggerCheckboxEventFalse()
        {
            if (_valueSetThisFrame)
            {
                _valueSetThisFrame = false;
                return;
            }
            MMDebugMenuCheckboxEvent.Trigger(CheckboxEventName, false, MMDebugMenuCheckboxEvent.EventModes.FromCheckbox);
        }

        protected virtual void OnMMDebugMenuCheckboxEvent(string checkboxEventName, bool value, MMDebugMenuCheckboxEvent.EventModes eventMode)
        {
            if ((eventMode == MMDebugMenuCheckboxEvent.EventModes.SetCheckbox)
                    && (checkboxEventName == CheckboxEventName))
            {
                _valueSetThisFrame = true;
                if (value)
                {
                    Switch.SetTrue();
                }
                else
                {
                    Switch.SetFalse();
                }
            }
        }
        public virtual void OnEnable()
        {
            if (!_listening)
            {
                _listening = true;
                MMDebugMenuCheckboxEvent.Register(OnMMDebugMenuCheckboxEvent);
            }            
        }
        public virtual void OnDestroy()
        {
            _listening = false;
            MMDebugMenuCheckboxEvent.Unregister(OnMMDebugMenuCheckboxEvent);
        }
    }
}
