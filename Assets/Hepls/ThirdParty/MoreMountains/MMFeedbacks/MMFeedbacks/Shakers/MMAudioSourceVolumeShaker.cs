using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioSourceVolumeShaker")]
    [RequireComponent(typeof(AudioSource))]
    public class MMAudioSourceVolumeShaker : MMShaker
    {
        [Header("Volume")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeVolume = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeVolume = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-1f, 1f)]
        public float RemapVolumeZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-1f, 1f)]
        public float RemapVolumeOne = 1f;
        protected AudioSource _targetAudioSource;
        protected float _initialVolume;
        protected float _originalShakeDuration;
        protected bool _originalRelativeValues;
        protected AnimationCurve _originalShakeVolume;
        protected float _originalRemapVolumeZero;
        protected float _originalRemapVolumeOne;
        protected override void Initialization()
        {
            base.Initialization();
            _targetAudioSource = this.gameObject.GetComponent<AudioSource>();
        }
        protected virtual void Reset()
        {
            ShakeDuration = 2f;
        }
        protected override void Shake()
        {
            float newVolume = ShakeFloat(ShakeVolume, RemapVolumeZero, RemapVolumeOne, RelativeVolume, _initialVolume);
            _targetAudioSource.volume = newVolume;
        }
        protected override void GrabInitialValues()
        {
            _initialVolume = _targetAudioSource.volume;
        }
        public virtual void OnMMAudioSourceVolumeShakeEvent(AnimationCurve volumeCurve, float duration, float remapMin, float remapMax, bool relativeVolume = false,
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
                _originalShakeVolume = ShakeVolume;
                _originalRemapVolumeZero = RemapVolumeZero;
                _originalRemapVolumeOne = RemapVolumeOne;
                _originalRelativeValues = RelativeVolume;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeVolume = volumeCurve;
            RemapVolumeZero = remapMin * feedbacksIntensity;
            RemapVolumeOne = remapMax * feedbacksIntensity;
            RelativeVolume = relativeVolume;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetAudioSource.volume = _initialVolume;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeVolume = _originalShakeVolume;
            RemapVolumeZero = _originalRemapVolumeZero;
            RemapVolumeOne = _originalRemapVolumeOne;
            RelativeVolume = _originalRelativeValues;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMAudioSourceVolumeShakeEvent.Register(OnMMAudioSourceVolumeShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMAudioSourceVolumeShakeEvent.Unregister(OnMMAudioSourceVolumeShakeEvent);
        }
    }
    public struct MMAudioSourceVolumeShakeEvent
    {
        public delegate void Delegate(AnimationCurve volumeCurve, float duration, float remapMin, float remapMax, bool relativeVolume = false,
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

        static public void Trigger(AnimationCurve volumeCurve, float duration, float remapMin, float remapMax, bool relativeVolume = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(volumeCurve, duration, remapMin, remapMax, relativeVolume,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
