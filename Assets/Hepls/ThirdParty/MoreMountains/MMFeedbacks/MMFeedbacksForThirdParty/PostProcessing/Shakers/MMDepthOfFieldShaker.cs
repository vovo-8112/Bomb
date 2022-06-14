using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMDepthOfFieldShaker")]
    [RequireComponent(typeof(PostProcessVolume))]
    public class MMDepthOfFieldShaker : MMShaker
    {
        public bool RelativeValues = true;

        [Header("Focus Distance")]
        [Tooltip("the curve used to animate the focus distance value on")]
        public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapFocusDistanceZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapFocusDistanceOne = 3f;

        [Header("Aperture")]
        [Tooltip("the curve used to animate the aperture value on")]
        public AnimationCurve ShakeAperture = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0.1f, 32f)]
        public float RemapApertureZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0.1f, 32f)]
        public float RemapApertureOne = 0f;

        [Header("Focal Length")]
        [Tooltip("the curve used to animate the focal length value on")]
        public AnimationCurve ShakeFocalLength = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 300f)]
        public float RemapFocalLengthZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 300f)]
        public float RemapFocalLengthOne = 0f;

        protected PostProcessVolume _volume;
        protected DepthOfField _depthOfField;
        protected float _initialFocusDistance;
        protected float _initialAperture;
        protected float _initialFocalLength;
        protected float _originalShakeDuration;
        protected bool _originalRelativeValues;
        protected AnimationCurve _originalShakeFocusDistance;
        protected float _originalRemapFocusDistanceZero;
        protected float _originalRemapFocusDistanceOne;
        protected AnimationCurve _originalShakeAperture;
        protected float _originalRemapApertureZero;
        protected float _originalRemapApertureOne;
        protected AnimationCurve _originalShakeFocalLength;
        protected float _originalRemapFocalLengthZero;
        protected float _originalRemapFocalLengthOne;
        protected override void Initialization()
        {
            base.Initialization();
            _volume = this.gameObject.GetComponent<PostProcessVolume>();
            _volume.profile.TryGetSettings(out _depthOfField);
        }
        protected override void Shake()
        {
            float newFocusDistance = ShakeFloat(ShakeFocusDistance, RemapFocusDistanceZero, RemapFocusDistanceOne, RelativeValues, _initialFocusDistance);
            _depthOfField.focusDistance.Override(newFocusDistance);

            float newAperture = ShakeFloat(ShakeAperture, RemapApertureZero, RemapApertureOne, RelativeValues, _initialAperture);
            _depthOfField.aperture.Override(newAperture);

            float newFocalLength = ShakeFloat(ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne, RelativeValues, _initialFocalLength);
            _depthOfField.focalLength.Override(newFocalLength);
        }
        protected virtual void Reset()
        {
            ShakeDuration = 2f;
        }
        protected override void GrabInitialValues()
        {
            _initialFocusDistance = _depthOfField.focusDistance;
            _initialAperture = _depthOfField.aperture;
            _initialFocalLength = _depthOfField.focalLength;
        }
        public virtual void OnDepthOfFieldShakeEvent(AnimationCurve focusDistance, float duration, float remapFocusDistanceMin, float remapFocusDistanceMax,
            AnimationCurve aperture, float remapApertureMin, float remapApertureMax,
            AnimationCurve focalLength, float remapFocalLengthMin, float remapFocalLengthMax,
            bool relativeValues = false,
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
                _originalRelativeValues = RelativeValues;

                _originalShakeFocusDistance = ShakeFocusDistance;
                _originalRemapFocusDistanceZero = RemapFocusDistanceZero;
                _originalRemapFocusDistanceOne = RemapFocusDistanceOne;

                _originalShakeAperture = ShakeAperture;
                _originalRemapApertureZero = RemapApertureZero;
                _originalRemapApertureOne = RemapApertureOne;

                _originalShakeFocalLength = ShakeFocalLength;
                _originalRemapFocalLengthZero = RemapFocalLengthZero;
                _originalRemapFocalLengthOne = RemapFocalLengthOne;
            }

            TimescaleMode = timescaleMode;
            
            ShakeDuration = duration;
            RelativeValues = relativeValues;

            ShakeFocusDistance = focusDistance;
            RemapFocusDistanceZero = remapFocusDistanceMin;
            RemapFocusDistanceOne = remapFocusDistanceMax;

            ShakeAperture = aperture;
            RemapApertureZero = remapApertureMin;
            RemapApertureOne = remapApertureMax;

            ShakeFocalLength = focalLength;
            RemapFocalLengthZero = remapFocalLengthMin;
            RemapFocalLengthOne = remapFocalLengthMax;

            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            
            _depthOfField.focusDistance.Override(_initialFocusDistance);
            _depthOfField.aperture.Override(_initialAperture);
            _depthOfField.focalLength.Override(_initialFocalLength);
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();

            ShakeDuration = _originalShakeDuration;
            RelativeValues = _originalRelativeValues;

            ShakeFocusDistance = _originalShakeFocusDistance;
            RemapFocusDistanceZero = _originalRemapFocusDistanceZero;
            RemapFocusDistanceOne = _originalRemapFocusDistanceOne;

            ShakeAperture = _originalShakeAperture;
            RemapApertureZero = _originalRemapApertureZero;
            RemapApertureOne = _originalRemapApertureOne;

            ShakeFocalLength = _originalShakeFocalLength;
            RemapFocalLengthZero = _originalRemapFocalLengthZero;
            RemapFocalLengthOne = _originalRemapFocalLengthOne;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMDepthOfFieldShakeEvent.Register(OnDepthOfFieldShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMDepthOfFieldShakeEvent.Unregister(OnDepthOfFieldShakeEvent);
        }
    }
    public struct MMDepthOfFieldShakeEvent
    {
        public delegate void Delegate(AnimationCurve focusDistance, float duration, float remapFocusDistanceMin, float remapFocusDistanceMax,
            AnimationCurve aperture, float remapApertureMin, float remapApertureMax,
            AnimationCurve focalLength, float remapFocalLengthMin, float remapFocalLengthMax,
            bool relativeValues = false,
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

        static public void Trigger(AnimationCurve focusDistance, float duration, float remapFocusDistanceMin, float remapFocusDistanceMax,
            AnimationCurve aperture, float remapApertureMin, float remapApertureMax, 
            AnimationCurve focalLength, float remapFocalLengthMin, float remapFocalLengthMax,
            bool relativeValues = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
            bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(focusDistance, duration, remapFocusDistanceMin, remapFocusDistanceMax, 
                aperture, remapApertureMin, remapApertureMax, 
                focalLength, remapFocalLengthMin, remapFocalLengthMax, relativeValues,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}
