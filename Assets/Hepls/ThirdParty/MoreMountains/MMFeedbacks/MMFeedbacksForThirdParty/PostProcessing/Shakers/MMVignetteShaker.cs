using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMVignetteShaker")]
    [RequireComponent(typeof(PostProcessVolume))]
    public class MMVignetteShaker : MMShaker
    {
        [Header("Intensity")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeIntensity = true;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 1f)]
        public float RemapIntensityZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 1f)]
        public float RemapIntensityOne = 0.1f;

        protected PostProcessVolume _volume;
        protected Vignette _vignette;
        protected float _initialIntensity;
        protected float _originalShakeDuration;
        protected AnimationCurve _originalShakeIntensity;
        protected float _originalRemapIntensityZero;
        protected float _originalRemapIntensityOne;
        protected bool _originalRelativeIntensity;
        protected override void Initialization()
        {
            base.Initialization();
            _volume = this.gameObject.GetComponent<PostProcessVolume>();
            _volume.profile.TryGetSettings(out _vignette);
        }

        public virtual void SetVignette(float newValue)
        {
            _vignette.intensity.Override(newValue);
        }
        protected override void Shake()
        {
            float newValue = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, _initialIntensity);
            _vignette.intensity.Override(newValue);
        }
        protected override void GrabInitialValues()
        {
            _initialIntensity = _vignette.intensity;
        }
        public virtual void OnVignetteShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
            bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
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
                _originalRelativeIntensity = RelativeIntensity;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeIntensity = intensity;
            RemapIntensityZero = remapMin * feedbacksIntensity;
            RemapIntensityOne = remapMax * feedbacksIntensity;
            RelativeIntensity = relativeIntensity;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _vignette.intensity.Override(_initialIntensity);
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeIntensity = _originalShakeIntensity;
            RemapIntensityZero = _originalRemapIntensityZero;
            RemapIntensityOne = _originalRemapIntensityOne;
            RelativeIntensity = _originalRelativeIntensity;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMVignetteShakeEvent.Register(OnVignetteShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMVignetteShakeEvent.Unregister(OnVignetteShakeEvent);
        }
    }
    public struct MMVignetteShakeEvent
    {
        public delegate void Delegate(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
            bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false);
        static private event Delegate OnEvent;
        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }
        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }
        static public void Trigger(AnimationCurve intensity, float duration, float remapMin, float remapMax, bool relativeIntensity = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
            bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(intensity, duration, remapMin, remapMax, relativeIntensity, feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
