using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Camera/Field of View")]
    [FeedbackHelp("This feedback lets you control a camera's field of view over time. You'll need a MMCameraFieldOfViewShaker on your camera.")]
    public class MMFeedbackCameraFieldOfView : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Field of View Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Field of View")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeFieldOfView = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeFieldOfView = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 179f)]
        public float RemapFieldOfViewZero = 60f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 179f)]
        public float RemapFieldOfViewOne = 120f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero, RemapFieldOfViewOne, RelativeFieldOfView,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero, RemapFieldOfViewOne, stop: true);
            }
        }
    }
}
