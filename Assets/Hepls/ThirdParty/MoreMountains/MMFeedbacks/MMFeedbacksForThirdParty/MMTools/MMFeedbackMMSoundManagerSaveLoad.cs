using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/MMSoundManager Save and Load")]
    [FeedbackHelp("This feedback will let you trigger save, load, and reset on MMSoundManager settings. You will need a MMSoundManager in your scene for this to work.")]
    public class MMFeedbackMMSoundManagerSaveLoad : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public enum Modes { Save, Load, Reset }

        [Header("MMSoundManager Save and Load")]
        [Tooltip("the selected mode to interact with save settings on the MMSoundManager")]
        public Modes Mode = Modes.Save;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                switch (Mode)
                {
                    case Modes.Save:
                        MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.SaveSettings);
                        break;
                    case Modes.Load:
                        MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.LoadSettings);
                        break;
                    case Modes.Reset:
                        MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.ResetSettings);
                        break;
                }
            }
        }
    }
}
