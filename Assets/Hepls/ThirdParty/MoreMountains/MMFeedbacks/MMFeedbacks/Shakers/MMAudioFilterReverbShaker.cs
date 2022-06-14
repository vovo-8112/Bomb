using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterReverbShaker")]
    [RequireComponent(typeof(AudioReverbFilter))]
    public class MMAudioFilterReverbShaker : MMShaker
    {
        [Header("Reverb")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeReverb = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeReverb = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-10000f, 2000f)]
        public float RemapReverbZero = -10000f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-10000f, 2000f)]
        public float RemapReverbOne = 2000f;
        protected AudioReverbFilter _targetAudioReverbFilter;
        protected float _initialReverb;
        protected float _originalShakeDuration;
        protected bool _originalRelativeReverb;
        protected AnimationCurve _originalShakeReverb;
        protected float _originalRemapReverbZero;
        protected float _originalRemapReverbOne;
        protected override void Initialization()
        {
            base.Initialization();
            _targetAudioReverbFilter = this.gameObject.GetComponent<AudioReverbFilter>();
        }
        protected virtual void Reset()
        {
            ShakeDuration = 2f;
        }
        protected override void Shake()
        {
            float newReverbLevel = ShakeFloat(ShakeReverb, RemapReverbZero, RemapReverbOne, RelativeReverb, _initialReverb);
            _targetAudioReverbFilter.reverbLevel = newReverbLevel;
        }
        protected override void GrabInitialValues()
        {
            _initialReverb = _targetAudioReverbFilter.reverbLevel;
        }
        public virtual void OnMMAudioFilterReverbShakeEvent(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
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
                _originalShakeReverb = ShakeReverb;
                _originalRemapReverbZero = RemapReverbZero;
                _originalRemapReverbOne = RemapReverbOne;
                _originalRelativeReverb = RelativeReverb;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeReverb = reverbCurve;
            RemapReverbZero = remapMin * feedbacksIntensity;
            RemapReverbOne = remapMax * feedbacksIntensity;
            RelativeReverb = relativeReverb;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetAudioReverbFilter.reverbLevel = _initialReverb;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeReverb = _originalShakeReverb;
            RemapReverbZero = _originalRemapReverbZero;
            RemapReverbOne = _originalRemapReverbOne;
            RelativeReverb = _originalRelativeReverb;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMAudioFilterReverbShakeEvent.Register(OnMMAudioFilterReverbShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMAudioFilterReverbShakeEvent.Unregister(OnMMAudioFilterReverbShakeEvent);
        }
    }
    public struct MMAudioFilterReverbShakeEvent
    {
        public delegate void Delegate(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
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

        static public void Trigger(AnimationCurve reverbCurve, float duration, float remapMin, float remapMax, bool relativeReverb = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(reverbCurve, duration, remapMin, remapMax, relativeReverb,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
