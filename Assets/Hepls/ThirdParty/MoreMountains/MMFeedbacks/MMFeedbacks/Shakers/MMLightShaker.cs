using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Lights/MMLightShaker")]
    [RequireComponent(typeof(Light))]
    public class MMLightShaker : MMShaker
    {
        [Header("Light")]
        [Tooltip("the light to affect when playing the feedback")]
        public Light BoundLight;
        [Tooltip("whether or not that light should be turned off on start")]
        public bool StartsOff = true;
        [Tooltip("whether or not the values should be relative or not")]
        public bool RelativeValues = true;

        [Header("Color")]
        [Tooltip("whether or not this shaker should modify color")]
        public bool ModifyColor = true;
        [Tooltip("the colors to apply to the light over time")]
        public Gradient ColorOverTime;

        [Header("Intensity")]
        [Tooltip("the intensity to apply to the light over time")]
        public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the value to remap the intensity curve's 0 to")]
        public float RemapIntensityZero = 0f;
        [Tooltip("the value to remap the intensity curve's 1 to")]
        public float RemapIntensityOne = 1f;

        [Header("Range")]
        [Tooltip("the range to apply to the light over time")]
        public AnimationCurve RangeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the value to remap the range curve's 0 to")]
        public float RemapRangeZero = 0f;
        [Tooltip("the value to remap the range curve's 0 to")]
        public float RemapRangeOne = 10f;

        [Header("Shadow Strength")]
        [Tooltip("the range to apply to the light over time")]
        public AnimationCurve ShadowStrengthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the value to remap the shadow strength's curve's 0 to")]
        public float RemapShadowStrengthZero = 0f;
        [Tooltip("the value to remap the shadow strength's curve's 1 to")]
        public float RemapShadowStrengthOne = 1f;

        protected Color _initialColor;
        protected float _initialRange;
        protected float _initialIntensity;
        protected float _initialShadowStrength;

        protected bool _originalRelativeValues;
        protected bool _originalModifyColor;
        protected float _originalShakeDuration;
        protected Gradient _originalColorOverTime;
        protected AnimationCurve _originalIntensityCurve;
        protected float _originalRemapIntensityZero;
        protected float _originalRemapIntensityOne;
        protected AnimationCurve _originalRangeCurve;
        protected float _originalRemapRangeZero;
        protected float _originalRemapRangeOne;
        protected AnimationCurve _originalShadowStrengthCurve;
        protected float _originalRemapShadowStrengthZero;
        protected float _originalRemapShadowStrengthOne;
        protected override void Initialization()
        {
            base.Initialization();
            if (BoundLight == null)
            {
                BoundLight = this.gameObject.GetComponent<Light>();
            }
        }
        protected virtual void Reset()
        {
            ShakeDuration = 1f;
        }
        protected override void Shake()
        {
            float newRange = ShakeFloat(RangeCurve, RemapRangeZero, RemapRangeOne, RelativeValues, _initialRange);
            BoundLight.range = newRange;
            float newIntensity = ShakeFloat(IntensityCurve, RemapIntensityZero, RemapIntensityOne, RelativeValues, _initialIntensity);
            BoundLight.intensity = newIntensity;
            float newShadowStrength = ShakeFloat(ShadowStrengthCurve, RemapShadowStrengthZero, RemapShadowStrengthOne, RelativeValues, _initialShadowStrength);
            BoundLight.shadowStrength = Mathf.Clamp01(newShadowStrength);
            if (ModifyColor)
            {
                BoundLight.color = ColorOverTime.Evaluate(_remappedTimeSinceStart);
            }            
        }
        protected override void GrabInitialValues()
        {
            _initialColor = BoundLight.color;
            _initialRange = BoundLight.range;
            _initialIntensity = BoundLight.intensity;
            _initialShadowStrength = BoundLight.shadowStrength;
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            BoundLight.color = _initialColor;
            BoundLight.range = _initialRange;
            BoundLight.intensity = _initialIntensity;
            BoundLight.shadowStrength = _initialShadowStrength;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ModifyColor = _originalModifyColor;
            RelativeValues = _originalRelativeValues;
            ShakeDuration = _originalShakeDuration;
            ColorOverTime = _originalColorOverTime;
            IntensityCurve = _originalIntensityCurve;
            RemapIntensityZero = _originalRemapIntensityZero;
            RemapIntensityOne = _originalRemapIntensityOne;
            RangeCurve = _originalRangeCurve;
            RemapRangeZero  =_originalRemapRangeZero;
            RemapRangeOne = _originalRemapRangeOne;
            ShadowStrengthCurve = _originalShadowStrengthCurve;
            RemapShadowStrengthZero = _originalRemapShadowStrengthZero;
            RemapShadowStrengthOne = _originalRemapShadowStrengthOne;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMLightShakeEvent.Register(OnMMLightShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMLightShakeEvent.Unregister(OnMMLightShakeEvent);
        }

        public virtual void OnMMLightShakeEvent(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime,
            AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
            AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne,
            AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
            bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
        {
            if (!CheckEventAllowed(channel, useRange, eventRange, eventOriginPosition) || (!Interruptible && Shaking))
            {
                return;
            }

            _resetShakerValuesAfterShake = resetShakerValuesAfterShake;
            _resetTargetValuesAfterShake = resetTargetValuesAfterShake;

            if (resetShakerValuesAfterShake)
            {
                _originalModifyColor = ModifyColor;
                _originalRelativeValues = RelativeValues;
                _originalShakeDuration = ShakeDuration;
                _originalColorOverTime = ColorOverTime;
                _originalIntensityCurve = IntensityCurve;
                _originalRemapIntensityZero = RemapIntensityZero;
                _originalRemapIntensityOne = RemapIntensityOne;
                _originalRangeCurve = RangeCurve;
                _originalRemapRangeZero = RemapRangeZero;
                _originalRemapRangeOne = RemapRangeOne;
                _originalShadowStrengthCurve = ShadowStrengthCurve;
                _originalRemapShadowStrengthZero = RemapShadowStrengthZero;
                _originalRemapShadowStrengthOne = RemapShadowStrengthOne;
            }

            ModifyColor = modifyColor;
            RelativeValues = relativeValues;
            ShakeDuration = shakeDuration;
            ColorOverTime = colorOverTime;
            IntensityCurve = intensityCurve;
            RemapIntensityZero = remapIntensityZero;
            RemapIntensityOne = remapIntensityOne;
            RangeCurve = rangeCurve;
            RemapRangeZero = remapRangeZero;
            RemapRangeOne = remapRangeOne;
            ShadowStrengthCurve = shadowStrengthCurve;
            RemapShadowStrengthZero = remapShadowStrengthZero;
            RemapShadowStrengthOne = remapShadowStrengthOne;

            Play();
        }
    }
           
    public struct MMLightShakeEvent
    {
        public delegate void Delegate(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime, 
            AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
        AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne, 
        AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
            bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime,
            AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
            AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne,
            AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
            bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
        {
            OnEvent?.Invoke(shakeDuration, relativeValues, modifyColor, colorOverTime,
                intensityCurve, remapIntensityZero, remapIntensityOne,
                rangeCurve, remapRangeZero, remapRangeOne,
                shadowStrengthCurve, remapShadowStrengthZero, remapShadowStrengthOne, 
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, 
                useRange, eventRange, eventOriginPosition);
        }
    }
}
