using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/Audio Filter Distortion")]
    [FeedbackHelp("This feedback lets you control a distortion audio filter over time. You'll need a MMAudioFilterDistortionShaker on the filter.")]
    public class MMFeedbackAudioFilterDistortion : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Distortion Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Distortion")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeDistortion = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeDistortion = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 1f)]
        public float RemapDistortionZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 1f)]
        public float RemapDistortionOne = 1f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                float remapZero = 0f;
                float remapOne = 0f;
                
                if (!Timing.ConstantIntensity)
                {
                    remapZero = RemapDistortionZero * intensityMultiplier;
                    remapOne = RemapDistortionOne * intensityMultiplier;
                }
                
                MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, remapZero, remapOne, RelativeDistortion,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, 0f,0f, stop:true);
            }
        }
    }
}
