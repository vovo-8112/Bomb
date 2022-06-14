using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterDistortionShaker")]
    [RequireComponent(typeof(AudioDistortionFilter))]
    public class MMAudioFilterDistortionShaker : MMShaker
    {
        [Header("Distortion")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeDistortion = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeDistortion = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 1f)]
        public float RemapDistortionZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 1f)]
        public float RemapDistortionOne = 1f;
        protected AudioDistortionFilter _targetAudioDistortionFilter;
        protected float _initialDistortion;
        protected float _originalShakeDuration;
        protected bool _originalRelativeDistortion;
        protected AnimationCurve _originalShakeDistortion;
        protected float _originalRemapDistortionZero;
        protected float _originalRemapDistortionOne;
        protected override void Initialization()
        {
            base.Initialization();
            _targetAudioDistortionFilter = this.gameObject.GetComponent<AudioDistortionFilter>();
        }
        protected virtual void Reset()
        {
            ShakeDuration = 2f;
        }
        protected override void Shake()
        {
            float newDistortionLevel = ShakeFloat(ShakeDistortion, RemapDistortionZero, RemapDistortionOne, RelativeDistortion, _initialDistortion);
            _targetAudioDistortionFilter.distortionLevel = newDistortionLevel;
        }
        protected override void GrabInitialValues()
        {
            _initialDistortion = _targetAudioDistortionFilter.distortionLevel;
        }
        public virtual void OnMMAudioFilterDistortionShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
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
                _originalShakeDistortion = ShakeDistortion;
                _originalRemapDistortionZero = RemapDistortionZero;
                _originalRemapDistortionOne = RemapDistortionOne;
                _originalRelativeDistortion = RelativeDistortion;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeDistortion = distortionCurve;
            RemapDistortionZero = remapMin * feedbacksIntensity;
            RemapDistortionOne = remapMax * feedbacksIntensity;
            RelativeDistortion = relativeDistortion;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetAudioDistortionFilter.distortionLevel = _initialDistortion;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeDistortion = _originalShakeDistortion;
            RemapDistortionZero = _originalRemapDistortionZero;
            RemapDistortionOne = _originalRemapDistortionOne;
            RelativeDistortion = _originalRelativeDistortion;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMAudioFilterDistortionShakeEvent.Register(OnMMAudioFilterDistortionShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMAudioFilterDistortionShakeEvent.Unregister(OnMMAudioFilterDistortionShakeEvent);
        }
    }
    public struct MMAudioFilterDistortionShakeEvent
    {
        public delegate void Delegate(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
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

        static public void Trigger(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(distortionCurve, duration, remapMin, remapMax, relativeDistortion,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
