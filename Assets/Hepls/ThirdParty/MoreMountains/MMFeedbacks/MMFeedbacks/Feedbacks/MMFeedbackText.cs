using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you control the contents of a target Text over time.")]
    [FeedbackPath("UI/Text")]
    public class MMFeedbackText : MMFeedback
    {
        public enum ColorModes { Instant, Gradient, Interpolate }
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
        #endif

        [Header("Target")]
        [Tooltip(" Text component to control")]
        public Text TargetText;
        [Tooltip("the new text to replace the old one with")]
        [TextArea]
        public string NewText = "Hello World";
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (TargetText == null)
            {
                return;
            }

            TargetText.text = NewText;
        }
    }
}
