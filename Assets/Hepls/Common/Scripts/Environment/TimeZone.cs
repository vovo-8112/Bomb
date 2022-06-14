using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Environment/Time Zone")]
    public class TimeZone : ButtonActivated
    {
        public enum Modes { DurationBased, ExitBased }

        [Header("Time Zone")]
        [Tooltip("whether this zone will modify time on entry for a certain duration, or until it is exited")]
        public Modes Mode = Modes.DurationBased;
        [Tooltip("the new timescale to apply")]
        public float TimeScale = 0.5f;
        [Tooltip("the duration to apply the new timescale for")]
        public float Duration = 1f;
        [Tooltip("whether or not the timescale should be lerped")]
        public bool LerpTimeScale = true;
        [Tooltip("the speed at which to lerp the timescale")]
        public float LerpSpeed = 5f;
        public override void TriggerButtonAction()
        {
            if (!CheckNumberOfUses())
            {
                return;
            }
            base.TriggerButtonAction();
            ControlTime();
            ActivateZone();
        }
        public override void TriggerExitAction(GameObject collider)
        {
            if (Mode == Modes.ExitBased)
            {
                if (!CheckConditions(collider))
                {
                    return;
                }

                if (!TestForLastObject(collider))
                {
                    return;
                }

                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            }
        }
        public virtual void ControlTime()
        {
            if (Mode == Modes.ExitBased)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, true);
            }
            else
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, false);
            }
        }
    }
}