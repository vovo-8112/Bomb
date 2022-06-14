using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will 'hold', or wait, until all previous feedbacks have been executed, and will then pause the execution of your MMFeedbacks sequence, for the specified duration.")]
    [FeedbackPath("Pause/Holding Pause")]
    public class MMFeedbackHoldingPause : MMFeedbackPause
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HoldingPauseColor; } }
        #endif
        public override bool HoldingPause { get { return true; } }
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                StartCoroutine(PlayPause());
            }
        }
    }
}
