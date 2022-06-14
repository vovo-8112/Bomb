using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you trigger position, rotation and/or scale wiggles on an object equipped with a MMWiggle component, for the specified durations.")]
    [FeedbackPath("Transform/Wiggle")]
    public class MMFeedbackWiggle : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        #endif

        [Header("Target")]
        [Tooltip("the Wiggle component to target")]
        public MMWiggle TargetWiggle;
        
        [Header("Position")]
        [Tooltip("whether or not to wiggle position")]
        public bool WigglePosition = true;
        [Tooltip("the duration (in seconds) of the position wiggle")]
        public float WigglePositionDuration;

        [Header("Rotation")]
        [Tooltip("whether or not to wiggle rotation")]
        public bool WiggleRotation;
        [Tooltip("the duration (in seconds) of the rotation wiggle")]
        public float WiggleRotationDuration;

        [Header("Scale")]
        [Tooltip("whether or not to wiggle scale")]
        public bool WiggleScale;
        [Tooltip("the duration (in seconds) of the scale wiggle")]
        public float WiggleScaleDuration;
        public override float FeedbackDuration
        {
            get { return Mathf.Max(ApplyTimeMultiplier(WigglePositionDuration), ApplyTimeMultiplier(WiggleRotationDuration), ApplyTimeMultiplier(WiggleScaleDuration)); }
            set { WigglePositionDuration = value;
                WiggleRotationDuration = value;
                WiggleScaleDuration = value;
            } 
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            TargetWiggle.enabled = true;
            if (Active && (TargetWiggle != null))
            {
                if (WigglePosition)
                {
                    TargetWiggle.PositionWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
                    TargetWiggle.WigglePosition(ApplyTimeMultiplier(WigglePositionDuration));
                }
                if (WiggleRotation)
                {
                    TargetWiggle.RotationWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
                    TargetWiggle.WiggleRotation(ApplyTimeMultiplier(WiggleRotationDuration));
                }
                if (WiggleScale)
                {
                    TargetWiggle.ScaleWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
                    TargetWiggle.WiggleScale(ApplyTimeMultiplier(WiggleScaleDuration));
                }
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);

            if (Active && (TargetWiggle != null))
            {
                TargetWiggle.enabled = false;
            }
        }
    }
}
