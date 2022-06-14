﻿using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you control the opacity of a canvas group over time.")]
    [FeedbackPath("UI/CanvasGroup")]
    public class MMFeedbackCanvasGroup : MMFeedbackBase
    {
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
#endif

        [Header("Target")]
        [Tooltip("the receiver to write the level to")]
        public CanvasGroup TargetCanvasGroup;
        
        [Header("Level")]
        [Tooltip("the curve to tween the opacity on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public MMTweenType AlphaCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        [Tooltip("the value to remap the opacity curve's 0 to")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public float RemapZero = 0f;
        [Tooltip("the value to remap the opacity curve's 1 to")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public float RemapOne = 1f;
        [Tooltip("the value to move the opacity to in instant mode")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
        public float InstantAlpha;
        
        protected override void FillTargets()
        {
            if (TargetCanvasGroup == null)
            {
                return;
            }

            MMFeedbackBaseTarget target = new MMFeedbackBaseTarget();
            MMPropertyReceiver receiver = new MMPropertyReceiver();
            receiver.TargetObject = TargetCanvasGroup.gameObject;
            receiver.TargetComponent = TargetCanvasGroup;
            receiver.TargetPropertyName = "alpha";
            receiver.RelativeValue = RelativeValues;
            target.Target = receiver;
            target.LevelCurve = AlphaCurve;
            target.RemapLevelZero = RemapZero;
            target.RemapLevelOne = RemapOne;
            target.InstantLevel = InstantAlpha;

            _targets.Add(target);
        }

    }
}
