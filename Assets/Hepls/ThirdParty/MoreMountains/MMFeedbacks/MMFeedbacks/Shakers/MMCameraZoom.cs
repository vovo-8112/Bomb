using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MMCameraZoom")]
    public class MMCameraZoom : MonoBehaviour
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
        
        public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
        public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

        public TimescaleModes TimescaleMode { get; set; }
        
        protected Camera _camera;
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
            _camera = this.gameObject.GetComponent<Camera>();
            _initialFieldOfView = _camera.fieldOfView;
        }
        protected virtual void Update()
        {
            if (!_zooming)
            {
                return;
            }

            if (_camera.fieldOfView != _targetFieldOfView)
            {
                _delta += GetDeltaTime() / _transitionDuration;
                _camera.fieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomCurve.Evaluate(_delta));
            }
            else
            {
                if (!_destinationReached)
                {
                    _reachedDestinationTimestamp = GetTime();
                    _destinationReached = true;
                }

                if ((_mode == MMCameraZoomModes.For) && (_direction == 1))
                {
                    if (GetTime() - _reachedDestinationTimestamp > _duration)
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
        public virtual void Zoom(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, bool useUnscaledTime, bool relative = false)
        {
            if (_zooming)
            {
                return;
            }

            _zooming = true;
            _delta = 0f;
            _mode = mode;

            TimescaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
            _startFieldOfView = _camera.fieldOfView;
            _transitionDuration = transitionDuration;
            _duration = duration;
            _transitionDuration = transitionDuration;
            _direction = 1;
            _destinationReached = false;
            _initialFieldOfView = _camera.fieldOfView;

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
            Zoom(TestMode, TestFieldOfView, TestTransitionDuration, TestDuration, false);
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
            this.Zoom(mode, newFieldOfView, transitionDuration, duration, useUnscaledTime, relative);
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
