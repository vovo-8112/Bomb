using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMBloomShaker")]
    [RequireComponent(typeof(PostProcessVolume))]
    public class MMBloomShaker : MMShaker
    {
        public bool RelativeValues = true;

        [Header("Intensity")]
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapIntensityZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapIntensityOne = 10f;

        [Header("Threshold")]
        [Tooltip("the curve used to animate the threshold value on")]
        public AnimationCurve ShakeThreshold = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapThresholdZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapThresholdOne = 0f;

        protected PostProcessVolume _volume;
        protected Bloom _bloom;
        protected float _initialIntensity;
        protected float _initialThreshold;
        protected float _originalShakeDuration;
        protected bool _originalRelativeIntensity;
        protected AnimationCurve _originalShakeIntensity;
        protected float _originalRemapIntensityZero;
        protected float _originalRemapIntensityOne;
        protected AnimationCurve _originalShakeThreshold;
        protected float _originalRemapThresholdZero;
        protected float _originalRemapThresholdOne;
        protected override void Initialization()
        {
            base.Initialization();
            _volume = this.gameObject.GetComponent<PostProcessVolume>();
            _volume.profile.TryGetSettings(out _bloom);
        }
        protected override void Shake()
        {
            float newIntensity = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeValues, _initialIntensity);
            _bloom.intensity.Override(newIntensity);
            float newThreshold = ShakeFloat(ShakeThreshold, RemapThresholdZero, RemapThresholdOne, RelativeValues, _initialThreshold);
            _bloom.threshold.Override(newThreshold);
        }
        protected override void GrabInitialValues()
        {
            _initialIntensity = _bloom.intensity;
            _initialThreshold = _bloom.threshold;
        }
        public virtual void OnBloomShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax,
            AnimationCurve threshold, float remapThresholdMin, float remapThresholdMax, bool relativeIntensity = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            if (!CheckEventAllowed(channel) || (!Interruptible && Shaking))
            {
                return;
            }
            
            if (stop)
            {
                Stop();
                return;
            }

            _resetShakerValuesAfterShake = resetShakerValuesAfterShake;
            _resetTargetValuesAfterShake = resetTargetValuesAfterShake;

            if (resetShakerValuesAfterShake)
            {
                _originalShakeDuration = ShakeDuration;
                _originalShakeIntensity = ShakeIntensity;
                _originalRemapIntensityZero = RemapIntensityZero;
                _originalRemapIntensityOne = RemapIntensityOne;
                _originalRelativeIntensity = RelativeValues;
                _originalShakeThreshold = ShakeThreshold;
                _originalRemapThresholdZero = RemapThresholdZero;
                _originalRemapThresholdOne = RemapThresholdOne;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeIntensity = intensity;
            RemapIntensityZero = remapMin * feedbacksIntensity;
            RemapIntensityOne = remapMax * feedbacksIntensity;
            RelativeValues = relativeIntensity;
            ShakeThreshold = threshold;
            RemapThresholdZero = remapThresholdMin;
            RemapThresholdOne = remapThresholdMax;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _bloom.intensity.Override(_initialIntensity);
            _bloom.threshold.Override(_initialThreshold);
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeIntensity = _originalShakeIntensity;
            RemapIntensityZero = _originalRemapIntensityZero;
            RemapIntensityOne = _originalRemapIntensityOne;
            RelativeValues = _originalRelativeIntensity;
            ShakeThreshold = _originalShakeThreshold;
            RemapThresholdZero = _originalRemapThresholdZero;
            RemapThresholdOne = _originalRemapThresholdOne;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMBloomShakeEvent.Register(OnBloomShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMBloomShakeEvent.Unregister(OnBloomShakeEvent);
        }
    }
    public struct MMBloomShakeEvent
    {
        public delegate void Delegate(AnimationCurve intensity, float duration, float remapMin, float remapMax,
            AnimationCurve threshold, float remapThresholdMin, float remapThresholdMax, bool relativeIntensity = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(AnimationCurve intensity, float duration, float remapMin, float remapMax,
            AnimationCurve threshold, float remapThresholdMin, float remapThresholdMax, bool relativeIntensity = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(intensity, duration, remapMin, remapMax, threshold, remapThresholdMin, remapThresholdMax, relativeIntensity,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
