﻿using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{

	public class MMInput : MonoBehaviour 
	{
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }

        public enum AxisTypes { Positive, Negative }
		public static ButtonStates ProcessAxisAsButton (string axisName, float threshold, ButtonStates currentState, AxisTypes AxisType = AxisTypes.Positive)
		{
			float axisValue = Input.GetAxis (axisName);
			ButtonStates returnState;

            bool comparison = (AxisType == AxisTypes.Positive) ? (axisValue < threshold) : (axisValue > threshold);
			
			if (comparison)
			{
				if (currentState == ButtonStates.ButtonPressed)
				{
					returnState = ButtonStates.ButtonUp;
				}
				else
				{
					returnState = ButtonStates.Off;
				}
			}
			else
			{
				if (currentState == ButtonStates.Off)
				{
					returnState = ButtonStates.ButtonDown;
				}
				else
				{
					returnState = ButtonStates.ButtonPressed;
				}
			}
			return returnState;
		}
		public class IMButton
		{
			public MMStateMachine<MMInput.ButtonStates> State {get;protected set;}
			public string ButtonID;

			public delegate void ButtonDownMethodDelegate();
			public delegate void ButtonPressedMethodDelegate();
			public delegate void ButtonUpMethodDelegate();

			public ButtonDownMethodDelegate ButtonDownMethod;
			public ButtonPressedMethodDelegate ButtonPressedMethod;
			public ButtonUpMethodDelegate ButtonUpMethod;
			public float TimeSinceLastButtonDown { get { return Time.unscaledTime - _lastButtonDownAt; } }
			public float TimeSinceLastButtonUp { get { return Time.unscaledTime - _lastButtonUpAt; } }
			public bool ButtonDownRecently(float time) { return (Time.unscaledTime - TimeSinceLastButtonDown <= time); }
			public bool ButtonUpRecently(float time) { return (Time.unscaledTime - TimeSinceLastButtonUp <= time); }

			protected float _lastButtonDownAt;
			protected float _lastButtonUpAt;

            public IMButton(string playerID, string buttonID, ButtonDownMethodDelegate btnDown = null, ButtonPressedMethodDelegate btnPressed = null, ButtonUpMethodDelegate btnUp = null) 
			{
				ButtonID = playerID + "_" + buttonID;
				ButtonDownMethod = btnDown;
				ButtonUpMethod = btnUp;
				ButtonPressedMethod = btnPressed;
				State = new MMStateMachine<MMInput.ButtonStates> (null, false);
				State.ChangeState (MMInput.ButtonStates.Off);
			}

			public virtual void TriggerButtonDown()
			{
				_lastButtonDownAt = Time.unscaledTime;
                if (ButtonDownMethod == null)
                {
                    State.ChangeState(MMInput.ButtonStates.ButtonDown);
                }
                else
                {
                    ButtonDownMethod();
                }
			}

			public virtual void TriggerButtonPressed()
			{
                if (ButtonPressedMethod == null)
                {
                    State.ChangeState(MMInput.ButtonStates.ButtonPressed);
                }
                else
                {
                    ButtonPressedMethod();
                }
			}

			public virtual void TriggerButtonUp()
            {
	            _lastButtonUpAt = Time.unscaledTime;
                if (ButtonUpMethod == null)
                {
                    State.ChangeState(MMInput.ButtonStates.ButtonUp);
                }
                else
                {
                    ButtonUpMethod();
                }
			}
		}
	}


}
