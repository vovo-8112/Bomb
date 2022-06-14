using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterEchoShaker")]
    [RequireComponent(typeof(AudioEchoFilter))]
    public class MMAudioFilterEchoShaker : MMShaker
    {
        [Header("Echo")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeEcho = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeEcho = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 1f)]
        public float RemapEchoZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 1f)]
        public float RemapEchoOne = 1f;
        protected AudioEchoFilter _targetAudioEchoFilter;
        protected float _initialEcho;
        protected float _originalShakeDuration;
        protected bool _originalRelativeEcho;
        protected AnimationCurve _originalShakeEcho;
        protected float _originalRemapEchoZero;
        protected float _originalRemapEchoOne;
        protected override void Initialization()
        {
            base.Initialization();
            _targetAudioEchoFilter = this.gameObject.GetComponent<AudioEchoFilter>();
        }
        protected virtual void Reset()
        {
            ShakeDuration = 2f;
        }
        protected override void Shake()
        {
            float newEchoLevel = ShakeFloat(ShakeEcho, RemapEchoZero, RemapEchoOne, RelativeEcho, _initialEcho);
            _targetAudioEchoFilter.wetMix = newEchoLevel;
        }
        protected override void GrabInitialValues()
        {
            _initialEcho = _targetAudioEchoFilter.wetMix;
        }
        public virtual void OnMMAudioFilterEchoShakeEvent(AnimationCurve echoCurve, float duration, float remapMin, float remapMax, bool relativeEcho = false,
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
                _originalShakeEcho = ShakeEcho;
                _originalRemapEchoZero = RemapEchoZero;
                _originalRemapEchoOne = RemapEchoOne;
                _originalRelativeEcho = RelativeEcho;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeEcho = echoCurve;
            RemapEchoZero = remapMin * feedbacksIntensity;
            RemapEchoOne = remapMax * feedbacksIntensity;
            RelativeEcho = relativeEcho;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetAudioEchoFilter.wetMix = _initialEcho;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeEcho = _originalShakeEcho;
            RemapEchoZero = _originalRemapEchoZero;
            RemapEchoOne = _originalRemapEchoOne;
            RelativeEcho = _originalRelativeEcho;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMAudioFilterEchoShakeEvent.Register(OnMMAudioFilterEchoShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMAudioFilterEchoShakeEvent.Unregister(OnMMAudioFilterEchoShakeEvent);
        }
    }
    public struct MMAudioFilterEchoShakeEvent
    {
        public delegate void Delegate(AnimationCurve echoCurve, float duration, float remapMin, float remapMax, bool relativeEcho = false,
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

        static public void Trigger(AnimationCurve echoCurve, float duration, float remapMin, float remapMax, bool relativeEcho = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(echoCurve, duration, remapMin, remapMax, relativeEcho,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
