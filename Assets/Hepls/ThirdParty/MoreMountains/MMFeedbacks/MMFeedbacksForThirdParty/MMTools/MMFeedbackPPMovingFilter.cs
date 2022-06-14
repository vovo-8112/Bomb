using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will trigger a post processing moving filter event, meant to be caught by a MMPostProcessingMovableFilter object")]
    [FeedbackPath("PostProcess/PPMovingFilter")]
    public class MMFeedbackPPMovingFilter : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
        #endif
        public enum Modes { Toggle, On, Off }

        [Header("PostProcessing Profile Moving Filter")]
        [Tooltip("the selected mode for this feedback")]
        public Modes Mode = Modes.Toggle;
        [Tooltip("the channel to target")]
        public int Channel = 0;
        [Tooltip("the duration of the transition")]
        public float TransitionDuration = 1f;
        [Tooltip("the curve to move along to")]
        public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(TransitionDuration); } set { TransitionDuration = value;  } }

        protected bool _active = false;
        protected bool _toggle = false;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                _active = (Mode == Modes.On);
                _toggle = (Mode == Modes.Toggle);

                MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, Channel);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, stop:true);
            }
        }
    }
}
