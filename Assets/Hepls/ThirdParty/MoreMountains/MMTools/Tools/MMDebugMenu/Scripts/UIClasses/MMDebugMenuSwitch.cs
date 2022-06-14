using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	public class MMDebugMenuSwitch : MMTouchButton 
	{
		[Header("Switch")]
		public MMDebugMenuSpriteReplace SwitchKnob;
        [MMReadOnly]
        public bool SwitchState;
		public bool InitialState = false;

		[Header("Binding")]
		public UnityEvent OnSwitchOn;
        public UnityEvent OnSwitchOff;
        public UnityEvent<bool> OnSwitchToggle;
        protected override void Initialization()
		{
			base.Initialization ();
			SwitchState = InitialState;
			InitializeState ();

            SwitchKnob.Initialization();
            if (InitialState)
            {
                SwitchKnob.SwitchToOnSprite();
            }
            else
            {
                SwitchKnob.SwitchToOffSprite();
            }
        }

		public virtual void InitializeState()
		{
		}

        public virtual void SetTrue()
        {
            SwitchState = true;
            if (_animator != null)
            {
                _animator.SetTrigger("Right");
            }
            SwitchKnob.SwitchToOnSprite();
            if (OnSwitchOn != null)
            {
                OnSwitchOn.Invoke();
            }
        }

        public virtual void SetFalse()
        {
            SwitchState = false;
            if (_animator != null)
            {
                _animator.SetTrigger("Left");
            }
            SwitchKnob.SwitchToOffSprite();
            if (OnSwitchOff != null)
            {
                OnSwitchOff.Invoke();
            }
        }
		public virtual void ToggleState()
		{
			if (SwitchState == false)
			{
                SetTrue();
			}
			else
			{
                SetFalse();	
			}
            OnSwitchToggle?.Invoke(SwitchState);
		}		
	}
}
