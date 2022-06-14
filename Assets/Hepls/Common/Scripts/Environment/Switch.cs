using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.Events;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Environment/Switch")]
    public class Switch : MonoBehaviour
    {
        [Header("Bindings")]
        [Tooltip("a SpriteReplace to represent the switch knob when it's on")]
        public Animator SwitchAnimator;
        public enum SwitchStates { On, Off }
        public SwitchStates CurrentSwitchState { get; set; }

        [Header("Switch")]
        [Tooltip("the state the switch should start in")]
        public SwitchStates InitialState = SwitchStates.Off;

        [Header("Events")]
        [Tooltip("the methods to call when the switch is turned on")]
        public UnityEvent SwitchOn;
        [Tooltip("the methods to call when the switch is turned off")]
        public UnityEvent SwitchOff;
        [Tooltip("the methods to call when the switch is toggled")]
        public UnityEvent SwitchToggle;

        [Header("Feedbacks")]
        [Tooltip("a feedback to play when the switch is toggled on")]
        public MMFeedbacks SwitchOnFeedback;
        [Tooltip("a feedback to play when the switch is turned off")]
        public MMFeedbacks SwitchOffFeedback;
        [Tooltip("a feedback to play when the switch changes state")]
        public MMFeedbacks ToggleFeedback;

        [MMInspectorButton("TurnSwitchOn")]
        public bool SwitchOnButton;
        [MMInspectorButton("TurnSwitchOff")]
        public bool SwitchOffButton;
        [MMInspectorButton("ToggleSwitch")]
        public bool ToggleSwitchButton;
        protected virtual void Start()
        {
            CurrentSwitchState = InitialState;
            SwitchOffFeedback?.Initialization(this.gameObject);
            SwitchOnFeedback?.Initialization(this.gameObject);
            ToggleFeedback?.Initialization(this.gameObject);
        }
        public virtual void TurnSwitchOn()
        {
            CurrentSwitchState = SwitchStates.On;
            if (SwitchOn != null) { SwitchOn.Invoke(); }
            if (SwitchToggle != null) { SwitchToggle.Invoke(); }
            SwitchOnFeedback?.PlayFeedbacks(this.transform.position);
        }
        public virtual void TurnSwitchOff()
        {
            CurrentSwitchState = SwitchStates.Off;
            if (SwitchOff != null) { SwitchOff.Invoke(); }
            if (SwitchToggle != null) { SwitchToggle.Invoke(); }
            SwitchOffFeedback?.PlayFeedbacks(this.transform.position);
        }
        public virtual void ToggleSwitch()
        {
            if (CurrentSwitchState == SwitchStates.Off)
            {
                CurrentSwitchState = SwitchStates.On;
                if (SwitchOn != null) { SwitchOn.Invoke(); }
                if (SwitchToggle != null) { SwitchToggle.Invoke(); }
                SwitchOnFeedback?.PlayFeedbacks(this.transform.position);
            }
            else
            {
                CurrentSwitchState = SwitchStates.Off;
                if (SwitchOff != null) { SwitchOff.Invoke(); }
                if (SwitchToggle != null) { SwitchToggle.Invoke(); }
                SwitchOffFeedback?.PlayFeedbacks(this.transform.position);
            }
            ToggleFeedback?.PlayFeedbacks(this.transform.position);
        }
        protected virtual void Update()
        {
            if (SwitchAnimator != null)
            {
                SwitchAnimator.SetBool("SwitchOn", (CurrentSwitchState == SwitchStates.On));
            }            
        }
    }
}
