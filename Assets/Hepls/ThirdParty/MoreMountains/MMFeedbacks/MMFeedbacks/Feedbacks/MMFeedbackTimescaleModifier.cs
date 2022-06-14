using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback triggers a MMTimeScaleEvent, which, if you have a MMTimeManager object in your scene, will be caught and used to modify the timescale according to the specified settings. These settings are the new timescale (0.5 will be twice slower than normal, 2 twice faster, etc), the duration of the timescale modification, and the optional speed at which to transition between normal and altered time scale.")]
    [FeedbackPath("Time/Timescale Modifier")]
    public class MMFeedbackTimescaleModifier : MMFeedback
    {
        public enum Modes { Shake, Change, Reset }
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
#endif

        [Header("Mode")]
        [Tooltip("the selected mode : shake : changes the timescale for a certain duration" +
                 "- change : sets the timescale to a new value, forever (until you change it again)" +
                 "- reset : resets the timescale to its previous value")]
        public Modes Mode = Modes.Shake;

        [Header("Timescale Modifier")]
        [Tooltip("the new timescale to apply")]
        public float TimeScale = 0.5f;
        [Tooltip("the duration of the timescale modification")]
        [MMFEnumCondition("Mode", (int)Modes.Shake)]
        public float TimeScaleDuration = 1f;
        [Tooltip("whether or not we should lerp the timescale")]
        [MMFEnumCondition("Mode", (int)Modes.Shake)]
        public bool TimeScaleLerp = false;
        [Tooltip("the speed at which to lerp the timescale")]
        [MMFEnumCondition("Mode", (int)Modes.Shake)]
        public float TimeScaleLerpSpeed = 1f;
        [Tooltip("whether to reset the timescale on Stop or not")]
        public bool ResetTimescaleOnStop = false;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(TimeScaleDuration); } set { TimeScaleDuration = value; } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                switch (Mode)
                {
                    case Modes.Shake:
                        MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, FeedbackDuration, TimeScaleLerp, TimeScaleLerpSpeed, false);
                        break;
                    case Modes.Change:
                        MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, 0f, false, 0f, true);
                        break;
                    case Modes.Reset:
                        MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
                        break;
                }                
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && ResetTimescaleOnStop)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
            }
        }
    }
}
