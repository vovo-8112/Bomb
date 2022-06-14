using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugMenuItemButton : MonoBehaviour
    {
        [Header("Bindings")]
        public Button TargetButton;
        public Text ButtonText;
        public Image ButtonBg;
        public string ButtonEventName = "Button";

        protected bool _listening = false;
        public virtual void TriggerButtonEvent()
        {
            MMDebugMenuButtonEvent.Trigger(ButtonEventName);
        }

        protected virtual void OnMMDebugMenuButtonEvent(string checkboxEventName, bool active, MMDebugMenuButtonEvent.EventModes eventMode)
        {
            if ((eventMode == MMDebugMenuButtonEvent.EventModes.SetButton)
                    && (checkboxEventName == ButtonEventName)
                    && (TargetButton != null))
            {
                TargetButton.interactable = active;
            }
        }
        public virtual void OnEnable()
        {
            if (!_listening)
            {
                _listening = true;
                MMDebugMenuButtonEvent.Register(OnMMDebugMenuButtonEvent);
            }
        }
        public virtual void OnDestroy()
        {
            _listening = false;
            MMDebugMenuButtonEvent.Unregister(OnMMDebugMenuButtonEvent);
        }
    }
}
