using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you control the font size of a target Text over time.")]
    [FeedbackPath("UI/Text Font Size")]
    public class MMFeedbackTextFontSize : MMFeedbackBase
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
        #endif

        [Header("Target")]
        [Tooltip("the TMP_Text component to control")]
        public Text TargetText;

        [Header("Font Size")]
        [Tooltip("the curve to tween on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public MMTweenType FontSizeCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        [Tooltip("the value to remap the curve's 0 to")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public float RemapZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public float RemapOne = 1f;
        [Tooltip("the value to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
        public float InstantFontSize;
        
        protected override void FillTargets()
        {
            if (TargetText == null)
            {
                return;
            }

            MMFeedbackBaseTarget target = new MMFeedbackBaseTarget();
            MMPropertyReceiver receiver = new MMPropertyReceiver();
            receiver.TargetObject = TargetText.gameObject;
            receiver.TargetComponent = TargetText;
            receiver.TargetPropertyName = "fontSize";
            receiver.RelativeValue = RelativeValues;
            target.Target = receiver;
            target.LevelCurve = FontSizeCurve;
            target.RemapLevelZero = RemapZero;
            target.RemapLevelOne = RemapOne;
            target.InstantLevel = InstantFontSize;

            _targets.Add(target);
        }

    }
}
