using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachineFreeLookZoom")]
    [RequireComponent(typeof(Cinemachine.CinemachineFreeLook))]
    public class MMCinemachineFreeLookZoom : MonoBehaviour
    {
        public int Channel = 0;

        [Header("Transition Speed")]
        [Tooltip("the animation curve to apply to the zoom transition")]
        public AnimationCurve ZoomCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        [Header("Test Zoom")]
        [Tooltip("the mode to apply the zoom in when using the test button in the inspector")]
        public MMCameraZoomModes TestMode;
        [Tooltip("the target field of view to apply the zoom in when using the test button in the inspector")]
        public float TestFieldOfView = 30f;
        [Tooltip("the transition duration to apply the zoom in when using the test button in the inspector")]
        public float TestTransitionDuration = 0.1f;
        [Tooltip("the duration to apply the zoom in when using the test button in the inspector")]
        public float TestDuration = 0.05f;

        [MMFInspectorButton("TestZoom")]
        public bool TestZoomButton;

        protected Cinemachine.CinemachineFreeLook _freeLookCamera;
        protected float _initialFieldOfView;
        protected MMCameraZoomModes _mode;
        protected bool _zooming = false;
        protected float _startFieldOfView;
        protected float _transitionDuration;
        protected float _duration;
        protected float _targetFieldOfView;
        protected float _delta = 0f;
        protected int _direction = 1;
        protected float _reachedDestinationTimestamp;
        protected bool _destinationReached = false;
        protected virtual void Awake()
        {
            _freeLookCamera = this.gameObject.GetComponent<Cinemachine.CinemachineFreeLook>();
            _initialFieldOfView = _freeLookCamera.m_Lens.FieldOfView;
        }
        protected virtual void Update()
        {
            if (!_zooming)
            {
                return;
            }

            if (_freeLookCamera.m_Lens.FieldOfView != _targetFieldOfView)
            {
                _delta += Time.deltaTime / _transitionDuration;
                _freeLookCamera.m_Lens.FieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomCurve.Evaluate(_delta));
            }
            else
            {
                if (!_destinationReached)
                {
                    _reachedDestinationTimestamp = Time.time;
                    _destinationReached = true;
                }

                if ((_mode == MMCameraZoomModes.For) && (_direction == 1))
                {
                    if (Time.time - _reachedDestinationTimestamp > _duration)
                    {
                        _direction = -1;
                        _startFieldOfView = _targetFieldOfView;
                        _targetFieldOfView = _initialFieldOfView;
                        _delta = 0f;
                    }                    
                }
                else
                {
                    _zooming = false;
                }                
            }
        }
        public virtual void Zoom(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, bool relative = false)
        {
            if (_zooming)
            {
                return;
            }

            _zooming = true;
            _delta = 0f;
            _mode = mode;

            _startFieldOfView = _freeLookCamera.m_Lens.FieldOfView;
            _transitionDuration = transitionDuration;
            _duration = duration;
            _transitionDuration = transitionDuration;
            _direction = 1;
            _destinationReached = false;

            switch (mode)
            {
                case MMCameraZoomModes.For:
                    _targetFieldOfView = newFieldOfView;
                    break;

                case MMCameraZoomModes.Set:
                    _targetFieldOfView = newFieldOfView;
                    break;

                case MMCameraZoomModes.Reset:
                    _targetFieldOfView = _initialFieldOfView;
                    break;
            }

            if (relative)
            {
                _targetFieldOfView += _initialFieldOfView;
            }

        }
        protected virtual void TestZoom()
        {
            Zoom(TestMode, TestFieldOfView, TestTransitionDuration, TestDuration);
        }
        public virtual void OnCameraZoomEvent(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, int channel, bool useUnscaledTime, bool stop = false, bool relative = false)
        {
            if ((channel != Channel) && (channel != -1) && (Channel != -1))
            {
                return;
            }
            if (stop)
            {
                _zooming = false;
                return;
            }
            this.Zoom(mode, newFieldOfView, transitionDuration, duration, relative);
        }
        protected virtual void OnEnable()
        {
            MMCameraZoomEvent.Register(OnCameraZoomEvent);
        }
        protected virtual void OnDisable()
        {
            MMCameraZoomEvent.Unregister(OnCameraZoomEvent);
        }
    }
}
