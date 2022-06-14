using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Camera/Orthographic Size")]
    [FeedbackHelp("This feedback lets you control a camera's orthographic size over time. You'll need a MMCameraOrthographicSizeShaker on your camera.")]
    public class MMFeedbackCameraOrthographicSize : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Orthographic Size Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Orthographic Size")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeOrthographicSize = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeOrthographicSize = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapOrthographicSizeZero = 5f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapOrthographicSizeOne = 10f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration, RemapOrthographicSizeZero, RemapOrthographicSizeOne, RelativeOrthographicSize,
                    feedbacksIntensity, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {

                MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration,
                    RemapOrthographicSizeZero, RemapOrthographicSizeOne, stop: true);
            }
        }
    }
}
