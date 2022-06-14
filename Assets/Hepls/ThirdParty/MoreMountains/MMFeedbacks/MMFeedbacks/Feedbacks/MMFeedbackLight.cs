using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you control the color and intensity of a Light in your scene for a certain duration (or instantly).")]
    [FeedbackPath("Light")]
    public class MMFeedbackLight : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LightColor; } }
        #endif
        public enum Modes { OverTime, Instant, ShakerEvent }

        [Header("Light")]
        [Tooltip("the light to affect when playing the feedback")]
        public Light BoundLight;
        [Tooltip("whether the feedback should affect the light instantly or over a period of time")]
        public Modes Mode = Modes.OverTime;
        [Tooltip("how long the light should change over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float Duration = 0.2f;
        [Tooltip("whether or not that light should be turned off on start")]
        public bool StartsOff = true;
        [Tooltip("whether or not the values should be relative or not")]
        public bool RelativeValues = true;
        [Tooltip("the channel to broadcast on")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public int Channel = 0;
        [Tooltip("whether or not to reset shaker values after shake")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool ResetTargetValuesAfterShake = true;
        [Tooltip("whether or not to broadcast a range to only affect certain shakers")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool UseRange = false;
        [Tooltip("the range of the event, in units")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public float EventRange = 100f;
        [Tooltip("the transform to use to broadcast the event as origin point")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public Transform EventOriginTransform;
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;
        [Tooltip("if this is true, the light will be disabled when this feedbacks is stopped")] 
        public bool DisableOnStop = true;

        [Header("Color")]
        [Tooltip("whether or not to modify the color of the light")]
        public bool ModifyColor = true;
        [Tooltip("the colors to apply to the light over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public Gradient ColorOverTime;
        [Tooltip("the color to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
        public Color InstantColor;

        [Header("Intensity")]
        [Tooltip("the curve to tween the intensity on")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the value to remap the intensity curve's 0 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapIntensityZero = 0f;
        [Tooltip("the value to remap the intensity curve's 1 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapIntensityOne = 1f;
        [Tooltip("the value to move the intensity to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public float InstantIntensity;

        [Header("Range")]
        [Tooltip("the range to apply to the light over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public AnimationCurve RangeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the value to remap the range curve's 0 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapRangeZero = 0f;
        [Tooltip("the value to remap the range curve's 0 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapRangeOne = 10f;
        [Tooltip("the value to move the intensity to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public float InstantRange;

        [Header("Shadow Strength")]
        [Tooltip("the range to apply to the light over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public AnimationCurve ShadowStrengthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the value to remap the shadow strength's curve's 0 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapShadowStrengthZero = 0f;
        [Tooltip("the value to remap the shadow strength's curve's 1 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapShadowStrengthOne = 1f;
        [Tooltip("the value to move the shadow strength to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public float InstantShadowStrength;

        protected float _initialRange;
        protected float _initialShadowStrength;
        protected float _initialIntensity;
        protected Coroutine _coroutine;
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);

            _initialRange = BoundLight.range;
            _initialShadowStrength = BoundLight.shadowStrength;
            _initialIntensity = BoundLight.intensity;

            if (EventOriginTransform == null)
            {
                EventOriginTransform = this.transform;
            }

            if (Active)
            {
                if (StartsOff)
                {
                    Turn(false);
                }
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                Turn(true);
                switch (Mode)
                {
                    case Modes.Instant:
                        BoundLight.intensity = InstantIntensity * intensityMultiplier;
                        BoundLight.shadowStrength = InstantShadowStrength;
                        BoundLight.range = InstantRange;
                        if (ModifyColor)
                        {
                            BoundLight.color = InstantColor;
                        }                        
                        break;
                    case Modes.OverTime:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(LightSequence(intensityMultiplier));

                        break;
                    case Modes.ShakerEvent:
                        MMLightShakeEvent.Trigger(FeedbackDuration, RelativeValues, ModifyColor, ColorOverTime, IntensityCurve,
                            RemapIntensityZero, RemapIntensityOne, RangeCurve, RemapRangeZero * intensityMultiplier, RemapRangeOne * intensityMultiplier,
                            ShadowStrengthCurve, RemapShadowStrengthZero, RemapShadowStrengthOne, feedbacksIntensity,
                            Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
                            UseRange, EventRange, EventOriginTransform.position);
                        break;
                }
            }
        }
        protected virtual IEnumerator LightSequence(float intensityMultiplier)
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                SetLightValues(remappedTime, intensityMultiplier);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetLightValues(FinalNormalizedTime, intensityMultiplier);
            if (StartsOff)
            {
                Turn(false);
            }            
            _coroutine = null;
            yield return null;
        }
        protected virtual void SetLightValues(float time, float intensityMultiplier)
        {
            float intensity = MMFeedbacksHelpers.Remap(IntensityCurve.Evaluate(time), 0f, 1f, RemapIntensityZero, RemapIntensityOne);
            float range = MMFeedbacksHelpers.Remap(RangeCurve.Evaluate(time), 0f, 1f, RemapRangeZero, RemapRangeOne);
            float shadowStrength = MMFeedbacksHelpers.Remap(ShadowStrengthCurve.Evaluate(time), 0f, 1f, RemapShadowStrengthZero, RemapShadowStrengthOne);        

            if (RelativeValues)
            {
                intensity += _initialIntensity;
                shadowStrength += _initialShadowStrength;
                range += _initialRange;
            }

            BoundLight.intensity = intensity * intensityMultiplier;
            BoundLight.range = range;
            BoundLight.shadowStrength = Mathf.Clamp01(shadowStrength);
            if (ModifyColor)
            {
                BoundLight.color = ColorOverTime.Evaluate(time);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active && (_coroutine != null))
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            if (Active && DisableOnStop)
            {
                Turn(false);
            }
        }
        protected virtual void Turn(bool status)
        {
            BoundLight.gameObject.SetActive(status);
            BoundLight.enabled = status;
        }
    }
}
