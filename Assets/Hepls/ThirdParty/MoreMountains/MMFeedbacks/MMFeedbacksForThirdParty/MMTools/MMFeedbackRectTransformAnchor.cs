using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you control the min and max anchors of a RectTransform over time. That's the normalized position in the parent RectTransform that the lower left and upper right corners are anchored to.")]
    [FeedbackPath("UI/RectTransform Anchor")]
    public class MMFeedbackRectTransformAnchor : MMFeedbackBase
    {
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
#endif

        [Header("Target")]
        [Tooltip("the target RectTransform to control")]
        public RectTransform TargetRectTransform;

        [Header("Anchor Min")]
        [Tooltip("whether or not to modify the min anchor")]
        public bool ModifyAnchorMin = true;
        [Tooltip("the curve to animate the min anchor on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public MMTweenType AnchorMinCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
        [Tooltip("the value to remap the min anchor curve's 0 on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public Vector2 AnchorMinRemapZero = Vector2.zero;
        [Tooltip("the value to remap the min anchor curve's 1 on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
        public Vector2 AnchorMinRemapOne = Vector2.one;
        
        [Header("Anchor Max")]
        [Tooltip("whether or not to modify the max anchor")]
        public bool ModifyAnchorMax = true;
        [Tooltip("the curve to animate the max anchor on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public MMTweenType AnchorMaxCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
        [Tooltip("the value to remap the max anchor curve's 0 on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public Vector2 AnchorMaxRemapZero = Vector2.zero;
        [Tooltip("the value to remap the max anchor curve's 1 on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
        public Vector2 AnchorMaxRemapOne = Vector2.one;
        
        protected override void FillTargets()
        {
            if (TargetRectTransform == null)
            {
                return;
            }
            
            MMFeedbackBaseTarget targetMin = new MMFeedbackBaseTarget();
            MMPropertyReceiver receiverMin = new MMPropertyReceiver();
            receiverMin.TargetObject = TargetRectTransform.gameObject;
            receiverMin.TargetComponent = TargetRectTransform;
            receiverMin.TargetPropertyName = "anchorMin";
            receiverMin.RelativeValue = RelativeValues;
            receiverMin.Vector2RemapZero = AnchorMinRemapZero;
            receiverMin.Vector2RemapOne = AnchorMinRemapOne;
            receiverMin.ShouldModifyValue = ModifyAnchorMin;
            targetMin.Target = receiverMin;
            targetMin.LevelCurve = AnchorMinCurve;
            targetMin.RemapLevelZero = 0f;
            targetMin.RemapLevelOne = 1f;
            targetMin.InstantLevel = 1f;

            _targets.Add(targetMin);
            
            MMFeedbackBaseTarget targetMax = new MMFeedbackBaseTarget();
            MMPropertyReceiver receiverMax = new MMPropertyReceiver();
            receiverMax.TargetObject = TargetRectTransform.gameObject;
            receiverMax.TargetComponent = TargetRectTransform;
            receiverMax.TargetPropertyName = "anchorMax";
            receiverMax.RelativeValue = RelativeValues;
            receiverMax.Vector2RemapZero = AnchorMaxRemapZero;
            receiverMax.Vector2RemapOne = AnchorMaxRemapOne;
            receiverMax.ShouldModifyValue = ModifyAnchorMax;
            targetMax.Target = receiverMax;
            targetMax.LevelCurve = AnchorMaxCurve;
            targetMax.RemapLevelZero = 0f;
            targetMax.RemapLevelOne = 1f;
            targetMax.InstantLevel = 1f;

            _targets.Add(targetMax);
        }

    }
}
