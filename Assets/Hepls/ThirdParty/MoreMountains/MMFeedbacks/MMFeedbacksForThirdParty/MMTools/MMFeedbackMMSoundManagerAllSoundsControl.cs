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
    [FeedbackPath("Audio/MMSoundManager All Sounds Control")]
    [FeedbackHelp("A feedback used to control all sounds playing on the MMSoundManager at once. It'll let you pause, play, stop and free (stop and returns the audiosource to the pool) sounds. You will need a MMSoundManager in your scene for this to work.")]
    public class MMFeedbackMMSoundManagerAllSoundsControl : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        
        [Header("MMSoundManager All Sounds Control")]
        [Tooltip("The selected control mode")]
        public MMSoundManagerAllSoundsControlEventTypes ControlMode = MMSoundManagerAllSoundsControlEventTypes.Pause;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                switch (ControlMode)
                {
                    case MMSoundManagerAllSoundsControlEventTypes.Pause:
                        MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Pause);
                        break;
                    case MMSoundManagerAllSoundsControlEventTypes.Play:
                        MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Play);
                        break;
                    case MMSoundManagerAllSoundsControlEventTypes.Stop:
                        MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Stop);
                        break;
                    case MMSoundManagerAllSoundsControlEventTypes.Free:
                        MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Free);
                        break;
                    case MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent:
                        MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent);
                        break;
                    case MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping:
                        MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping);
                        break;
                }
            }
        }
    }
}
