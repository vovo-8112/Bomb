using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Camera/Clipping Planes")]
    [FeedbackHelp("This feedback lets you control a camera's clipping planes over time. You'll need a MMCameraClippingPlanesShaker on your camera.")]
    public class MMFeedbackCameraClippingPlanes : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Clipping Planes Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeClippingPlanes = false;

        [Header("Near Plane")]
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeNear = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapNearZero = 0.3f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapNearOne = 100f;

        [Header("Far Plane")]
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeFar = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapFarZero = 0.3f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapFarOne = 100f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
                    ShakeFar, RemapFarZero, RemapFarOne,
                    RelativeClippingPlanes,
                    feedbacksIntensity, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
                    ShakeFar, RemapFarZero, RemapFarOne, stop: true);
            }
        }
    }
}
