using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public struct MMStateChangeEvent<T> where T: struct, IComparable, IConvertible, IFormattable
	{
		public GameObject Target;
		public MMStateMachine<T> TargetStateMachine;
		public T NewState;
		public T PreviousState;

		public MMStateChangeEvent(MMStateMachine<T> stateMachine)
		{
			Target = stateMachine.Target;
			TargetStateMachine = stateMachine;
			NewState = stateMachine.CurrentState;
			PreviousState = stateMachine.PreviousState;
		}
	}
	public interface MMIStateMachine
	{
		bool TriggerEvents { get; set; }
	}
	public class MMStateMachine<T> : MMIStateMachine where T : struct, IComparable, IConvertible, IFormattable
	{
		public bool TriggerEvents { get; set; }
		public GameObject Target;
		public T CurrentState { get; protected set; }
		public T PreviousState { get; protected set; }

        public delegate void OnStateChangeDelegate();
        public OnStateChangeDelegate OnStateChange;
        public MMStateMachine(GameObject target, bool triggerEvents)
		{
			this.Target = target;
			this.TriggerEvents = triggerEvents;
		}
		public virtual void ChangeState(T newState)
		{
			if (EqualityComparer<T>.Default.Equals(newState, CurrentState))
			{
				return;
			}
			PreviousState = CurrentState;
			CurrentState = newState;

            OnStateChange?.Invoke();

            if (TriggerEvents)
			{
				MMEventManager.TriggerEvent (new MMStateChangeEvent<T> (this));
			}
		}
		public virtual void RestorePreviousState()
		{
			CurrentState = PreviousState;

            OnStateChange?.Invoke();

            if (TriggerEvents)
			{
				MMEventManager.TriggerEvent (new MMStateChangeEvent<T> (this));
			}
		}	
	}
}