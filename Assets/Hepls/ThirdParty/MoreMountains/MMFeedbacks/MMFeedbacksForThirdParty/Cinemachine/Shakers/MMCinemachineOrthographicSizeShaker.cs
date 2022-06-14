﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachineOrthographicSizeShaker")]
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class MMCinemachineOrthographicSizeShaker : MMShaker
    {
        [Header("Orthographic Size")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeOrthographicSize = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeOrthographicSize = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapOrthographicSizeZero = 5f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapOrthographicSizeOne = 10f;

        protected CinemachineVirtualCamera _targetCamera;
        protected float _initialOrthographicSize;
        protected float _originalShakeDuration;
        protected bool _originalRelativeOrthographicSize;
        protected AnimationCurve _originalShakeOrthographicSize;
        protected float _originalRemapOrthographicSizeZero;
        protected float _originalRemapOrthographicSizeOne;
        protected override void Initialization()
        {
            base.Initialization();
            _targetCamera = this.gameObject.GetComponent<CinemachineVirtualCamera>();
        }
        protected virtual void Reset()
        {
            ShakeDuration = 0.5f;
        }
        protected override void Shake()
        {
            float newOrthographicSize = ShakeFloat(ShakeOrthographicSize, RemapOrthographicSizeZero, RemapOrthographicSizeOne, RelativeOrthographicSize, _initialOrthographicSize);
            _targetCamera.m_Lens.OrthographicSize = newOrthographicSize;
        }
        protected override void GrabInitialValues()
        {
            _initialOrthographicSize = _targetCamera.m_Lens.OrthographicSize;
        }
        public virtual void OnMMCameraOrthographicSizeShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, bool stop = false)
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
                _originalShakeOrthographicSize = ShakeOrthographicSize;
                _originalRemapOrthographicSizeZero = RemapOrthographicSizeZero;
                _originalRemapOrthographicSizeOne = RemapOrthographicSizeOne;
                _originalRelativeOrthographicSize = RelativeOrthographicSize;
            }

            ShakeDuration = duration;
            ShakeOrthographicSize = distortionCurve;
            RemapOrthographicSizeZero = remapMin * feedbacksIntensity;
            RemapOrthographicSizeOne = remapMax * feedbacksIntensity;
            RelativeOrthographicSize = relativeDistortion;
            ForwardDirection = forwardDirection;

            Play();
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetCamera.m_Lens.OrthographicSize = _initialOrthographicSize;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakeOrthographicSize = _originalShakeOrthographicSize;
            RemapOrthographicSizeZero = _originalRemapOrthographicSizeZero;
            RemapOrthographicSizeOne = _originalRemapOrthographicSizeOne;
            RelativeOrthographicSize = _originalRelativeOrthographicSize;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMCameraOrthographicSizeShakeEvent.Register(OnMMCameraOrthographicSizeShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMCameraOrthographicSizeShakeEvent.Unregister(OnMMCameraOrthographicSizeShakeEvent);
        }
    }
}

