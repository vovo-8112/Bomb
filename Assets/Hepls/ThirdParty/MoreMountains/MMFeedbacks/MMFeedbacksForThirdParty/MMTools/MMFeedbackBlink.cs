using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you trigger a blink on an MMBlink object.")]
    [FeedbackPath("Renderer/MMBlink")]
    public class MMFeedbackBlink : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif
        public enum BlinkModes { Toggle, Start, Stop }
        
        [Header("Blink")]
        [Tooltip("the target object to blink")]
        public MMBlink TargetBlink;
        [Tooltip("the selected mode for this feedback")]
        public BlinkModes BlinkMode = BlinkModes.Toggle;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active &&  (TargetBlink != null))
            {
                TargetBlink.TimescaleMode = Timing.TimescaleMode;
                switch (BlinkMode)
                {
                    case BlinkModes.Toggle:
                        TargetBlink.ToggleBlinking();
                        break;
                    case BlinkModes.Start:
                        TargetBlink.StartBlinking();
                        break;
                    case BlinkModes.Stop:
                        TargetBlink.StopBlinking();
                        break;
                }
            }
        }
    }
}