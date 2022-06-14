using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you trigger a play on a target MMRadioSignal (usually used by a MMRadioBroadcaster to emit a value that can then be listened to by MMRadioReceivers. From this feedback you can also specify a duration, timescale and multiplier.")]
    [FeedbackPath("GameObject/MMRadioSignal")]
    public class MMFeedbackRadioSignal : MMFeedback
    {
        public override float FeedbackDuration { get { return 0f; } }
        [Tooltip("The target MMRadioSignal to trigger")]
        public MMRadioSignal TargetSignal;
        [Tooltip("the timescale to operate on")]
        public MMRadioSignal.TimeScales TimeScale = MMRadioSignal.TimeScales.Unscaled;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 1f;
        [Tooltip("a global multiplier to apply to the end result of the combination")]
        public float GlobalMultiplier = 1f;
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
#endif
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                if (TargetSignal != null)
                {
                    float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                    
                    TargetSignal.Duration = Duration;
                    TargetSignal.GlobalMultiplier = GlobalMultiplier * intensityMultiplier;
                    TargetSignal.TimeScale = TimeScale;
                    TargetSignal.StartShaking();
                }
            }
        }

        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                if (TargetSignal != null)
                {
                    TargetSignal.Stop();
                }
            }
        }
    }
}
