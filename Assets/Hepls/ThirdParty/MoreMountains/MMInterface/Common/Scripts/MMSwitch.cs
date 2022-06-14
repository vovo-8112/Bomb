using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MoreMountains.MMInterface
{
	public class MMSwitch : MMTouchButton 
	{
		[Header("Switch")]
		public MMSpriteReplace SwitchKnob;
		public enum SwitchStates { Left, Right }
		public SwitchStates CurrentSwitchState { get; set; }
		public SwitchStates InitialState = SwitchStates.Left;

		[Header("Binding")]
		public UnityEvent SwitchOn;
		public UnityEvent SwitchOff;
		protected override void Initialization()
		{
			base.Initialization ();
			CurrentSwitchState = InitialState;
			InitializeState ();
		}

		public virtual void InitializeState()
		{
			if (CurrentSwitchState == SwitchStates.Left)
			{
				_animator.Play ("RollLeft");
			}
			else
			{
				_animator.Play ("RollRight");
			}
		}
		public virtual void SwitchState()
		{
			if (CurrentSwitchState == SwitchStates.Left)
			{
				CurrentSwitchState = SwitchStates.Right;
				_animator.SetTrigger ("Right");
				if (SwitchOn != null)
				{
					SwitchOn.Invoke();
				}		
			}
			else
			{
				CurrentSwitchState = SwitchStates.Left;				
				_animator.SetTrigger ("Left");
				if (SwitchOff != null)
				{
					SwitchOff.Invoke();
				}	
			}
		}		
	}
}
