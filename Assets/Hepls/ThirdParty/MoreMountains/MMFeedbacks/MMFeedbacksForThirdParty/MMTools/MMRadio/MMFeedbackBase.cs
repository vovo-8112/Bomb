using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    public class MMFeedbackBaseTarget
    {
        public MMPropertyReceiver Target;
        public MMTweenType LevelCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        public float RemapLevelZero = 0f;
        public float RemapLevelOne = 1f;
        public float InstantLevel;
        public float InitialLevel;
    }

    public abstract class MMFeedbackBase : MMFeedback
    {
        public enum Modes { OverTime, Instant } 
        
        [Header("Mode")]
        [Tooltip("whether the feedback should affect the target property instantly or over a period of time")]
        public Modes Mode = Modes.OverTime;
        [Tooltip("how long the target property should change over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime)]
        public float Duration = 0.2f;
        [Tooltip("whether or not that target property should be turned off on start")]
        public bool StartsOff = false;
        [Tooltip("whether or not the values should be relative or not")]
        public bool RelativeValues = true;
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;
        [Tooltip("if this is true, the target object will be disabled on stop")]
        public bool DisableOnStop = false;
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { if (Mode != Modes.Instant) { Duration = value; } } }

        protected List<MMFeedbackBaseTarget> _targets;
        protected Coroutine _coroutine = null;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);

            PrepareTargets();

            if (Active)
            {
                if (StartsOff)
                {
                    Turn(false);
                }
            }
        }
        protected virtual void PrepareTargets()
        {
            _targets = new List<MMFeedbackBaseTarget>();
            FillTargets();
            InitializeTargets();
        }
        protected virtual void OnValidate()
        {
            PrepareTargets();
        }
        protected abstract void FillTargets();
        protected virtual void InitializeTargets()
        {
            if (_targets.Count == 0)
            {
                return;
            }

            foreach(MMFeedbackBaseTarget target in _targets)
            {
                target.Target.Initialization(this.gameObject);
                target.InitialLevel = target.Target.Level;
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                Turn(true);
                switch (Mode)
                {
                    case Modes.Instant:
                        Instant();
                        break;
                    case Modes.OverTime:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(UpdateValueSequence(feedbacksIntensity));
                        break;
                }
            }
        }
        protected virtual void Instant()
        {
            if (_targets.Count == 0)
            {
                return;
            }

            foreach (MMFeedbackBaseTarget target in _targets)
            {
                target.Target.SetLevel(target.InstantLevel);
            }
        }
        protected virtual IEnumerator UpdateValueSequence(float feedbacksIntensity)
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;

            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);
                SetValues(remappedTime, feedbacksIntensity);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetValues(FinalNormalizedTime, feedbacksIntensity);
            if (StartsOff)
            {
                Turn(false);
            }

            _coroutine = null;
            yield return null;
        }
        protected virtual void SetValues(float time, float feedbacksIntensity)
        {
            if (_targets.Count == 0)
            {
                return;
            }
            
            float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
            
            foreach (MMFeedbackBaseTarget target in _targets)
            {
                float intensity = MMTween.Tween(time, 0f, 1f, target.RemapLevelZero, target.RemapLevelOne, target.LevelCurve);
                if (RelativeValues)
                {
                    intensity += target.InitialLevel;
                }
                target.Target.SetLevel(intensity * intensityMultiplier);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                    _coroutine = null;
                }

                if (DisableOnStop)
                {
                    Turn(false);    
                }
            }
        }
        protected virtual void Turn(bool status)
        {
            if (_targets.Count == 0)
            {
                return;
            }
            foreach (MMFeedbackBaseTarget target in _targets)
            {
                if (target.Target.TargetComponent.gameObject != null)
                {
                    target.Target.TargetComponent.gameObject.SetActive(status);
                }
            }
        }
    }
}
