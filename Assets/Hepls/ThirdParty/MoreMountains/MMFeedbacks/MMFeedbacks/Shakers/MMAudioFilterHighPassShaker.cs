using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterHighPassShaker")]
    [RequireComponent(typeof(AudioHighPassFilter))]
    public class MMAudioFilterHighPassShaker : MMShaker
    {
        [Header("High Pass")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeHighPass = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeHighPass = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(10f, 22000f)]
        public float RemapHighPassZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(10f, 22000f)]
        public float RemapHighPassOne = 10000f;
        protected AudioHighPassFilter _targetAudioHighPassFilter;
        protected float _initialHighPass;
        protected float _originalShakeDuration;
        protected bool _originalRelativeHighPass;
        protected AnimationCurve _originalShakeHighPass;
        protected float _originalRemapHighPassZero;
        protected float _originalRemapHighPassOne;
        protected override void Initialization()
        {
            base.Initialization();
            _targetAudioHighPassFilter = this.gameObject.GetComponent<AudioHighPassFilter>();
        }
        protected virtual void Reset()
        {
            ShakeDuration = 2f;
        }
        protected override void Shake()
        {
            float newHighPassLevel = ShakeFloat(ShakeHighPass, RemapHighPassZero, RemapHighPassOne, RelativeHighPass, _initialHighPass);
            _targetAudioHighPassFilter.cutoffFrequency = newHighPassLevel;
        }
        protected override void GrabInitialValues()
        {
            _initialHighPass = _targetAudioHighPassFilter.cutoffFrequency;
        }
        public virtual void OnMMAudioFilterHighPassShakeEvent(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
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
                _originalShakeHighPass = ShakeHighPass;
                _originalRemapHighPassZero = RemapHighPassZero;
                _originalRemapHighPassOne = RemapHighPassOne;
                _originalRelativeHighPass = RelativeHighPass;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeHighPass = highPassCurve;
            RemapHighPassZero = remapMin * feedbacksIntensity;
            RemapHighPassOne = remapMax * feedbacksIntensity;
            RelativeHighPass = relativeHighPass;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetAudioHighPassFilter.cutoffFrequency = _initialHighPass;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeHighPass = _originalShakeHighPass;
            RemapHighPassZero = _originalRemapHighPassZero;
            RemapHighPassOne = _originalRemapHighPassOne;
            RelativeHighPass = _originalRelativeHighPass;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMAudioFilterHighPassShakeEvent.Register(OnMMAudioFilterHighPassShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMAudioFilterHighPassShakeEvent.Unregister(OnMMAudioFilterHighPassShakeEvent);
        }
    }
    public struct MMAudioFilterHighPassShakeEvent
    {
        public delegate void Delegate(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
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

        static public void Trigger(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(highPassCurve, duration, remapMin, remapMax, relativeHighPass,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
