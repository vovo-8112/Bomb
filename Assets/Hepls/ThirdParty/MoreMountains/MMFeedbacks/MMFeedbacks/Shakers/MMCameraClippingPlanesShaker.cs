using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MMCameraClippingPlanesShaker")]
    [RequireComponent(typeof(Camera))]
    public class MMCameraClippingPlanesShaker : MMShaker
    {
        [Header("Clipping Planes")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeClippingPlanes = false;
        
        [Header("Near Plane")]
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeNear = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapNearZero = 0.3f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapNearOne = 100f;

        [Header("Far Plane")]
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeFar = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapFarZero = 1000f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapFarOne = 1000f;
        
        protected Camera _targetCamera;
        protected float _initialNear;
        protected float _initialFar;

        protected float _originalShakeDuration;
        protected bool _originalRelativeClippingPlanes;

        protected AnimationCurve _originalShakeNear;
        protected float _originalRemapNearZero;
        protected float _originalRemapNearOne;

        protected AnimationCurve _originalShakeFar;
        protected float _originalRemapFarZero;
        protected float _originalRemapFarOne;
        protected override void Initialization()
        {
            base.Initialization();
            _targetCamera = this.gameObject.GetComponent<Camera>();
        }
        protected virtual void Reset()
        {
            ShakeDuration = 0.5f;
        }
        protected override void Shake()
        {
            float newNear = ShakeFloat(ShakeNear, RemapNearZero, RemapNearOne, RelativeClippingPlanes, _initialNear);
            _targetCamera.nearClipPlane = newNear;
            float newFar = ShakeFloat(ShakeFar, RemapFarZero, RemapFarOne, RelativeClippingPlanes, _initialFar);
            _targetCamera.farClipPlane = newFar;
        }
        protected override void GrabInitialValues()
        {
            _initialNear = _targetCamera.nearClipPlane;
            _initialFar = _targetCamera.farClipPlane;
        }
        public virtual void OnMMCameraClippingPlanesShakeEvent(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValues = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            if (!CheckEventAllowed(channel))
            {
                return;
            }
            
            if (stop)
            {
                Stop();
                return;
            }
            
            if (!Interruptible && Shaking)
            {
                return;
            }
            
            _resetShakerValuesAfterShake = resetShakerValuesAfterShake;
            _resetTargetValuesAfterShake = resetTargetValuesAfterShake;

            if (resetShakerValuesAfterShake)
            {
                _originalShakeDuration = ShakeDuration;
                _originalShakeNear = ShakeNear;
                _originalShakeFar = ShakeFar;
                _originalRemapNearZero = RemapNearZero;
                _originalRemapNearOne = RemapNearOne;
                _originalRemapFarZero = RemapFarZero;
                _originalRemapFarOne = RemapFarOne;
                _originalRelativeClippingPlanes = RelativeClippingPlanes;
            }

            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeNear = animNearCurve;
            RemapNearZero = remapNearMin * feedbacksIntensity;
            RemapNearOne = remapNearMax * feedbacksIntensity;
            ShakeFar = animFarCurve;
            RemapFarZero = remapFarMin * feedbacksIntensity;
            RemapFarOne = remapFarMax * feedbacksIntensity;
            RelativeClippingPlanes = relativeValues;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetCamera.nearClipPlane = _initialNear;
            _targetCamera.farClipPlane = _initialFar;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeNear = _originalShakeNear;
            ShakeFar = _originalShakeFar;
            RemapNearZero = _originalRemapNearZero;
            RemapNearOne = _originalRemapNearOne;
            RemapFarZero = _originalRemapFarZero;
            RemapFarOne = _originalRemapFarOne;
            RelativeClippingPlanes = _originalRelativeClippingPlanes;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMCameraClippingPlanesShakeEvent.Register(OnMMCameraClippingPlanesShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMCameraClippingPlanesShakeEvent.Unregister(OnMMCameraClippingPlanesShakeEvent);
        }
    }
    public struct MMCameraClippingPlanesShakeEvent
    {
        public delegate void Delegate(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValue = false,
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

        static public void Trigger(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValue = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
        {
            OnEvent?.Invoke(animNearCurve, duration, remapNearMin, remapNearMax, animFarCurve, remapFarMin, remapFarMax, relativeValue,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
        }
    }
}

