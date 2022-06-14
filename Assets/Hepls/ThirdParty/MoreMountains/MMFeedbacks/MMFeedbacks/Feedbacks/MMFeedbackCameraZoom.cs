using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("Define zoom properties : For will set the zoom to the specified parameters for a certain duration, " +
                  "Set will leave them like that forever. Zoom properties include the field of view, the duration of the " +
                  "zoom transition (in seconds) and the zoom duration (the time the camera should remain zoomed in, in seconds). " +
                  "For this to work, you'll need to add a MMCameraZoom component to your Camera, or a MMCinemachineZoom if you're " +
                  "using virtual cameras.")]
    [FeedbackPath("Camera/Camera Zoom")]
    public class MMFeedbackCameraZoom : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
        #endif

        [Header("Camera Zoom")]
        [Tooltip("the channel to broadcast that zoom event on")]
        public int Channel = 0;
        [Tooltip("the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)")]
        public MMCameraZoomModes ZoomMode = MMCameraZoomModes.For;
        [Tooltip("the target field of view")]
        public float ZoomFieldOfView = 30f;
        [Tooltip("the zoom transition duration")]
        public float ZoomTransitionDuration = 0.05f;
        [Tooltip("the duration for which the zoom is at max zoom")]
        public float ZoomDuration = 0.1f;
        [Tooltip("whether or not ZoomFieldOfView should add itself to the current camera's field of view value")]
        public bool RelativeFieldOfView = false;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ZoomDuration); } set { ZoomDuration = value; } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, Channel, Timing.TimescaleMode == TimescaleModes.Unscaled, false, RelativeFieldOfView);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, Channel, Timing.TimescaleMode == TimescaleModes.Unscaled, stop:true);
            }
        }
    }
}
