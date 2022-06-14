using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = System.Random;

namespace MoreMountains.Tools
{
    public class MMFloatingText : MonoBehaviour
    {
        [Header("Bindings")]
        [Tooltip("the part of the prefab that we'll move")]
        public Transform MovingPart;
        [Tooltip("the part of the prefab that we'll rotate to face the target camera")]
        public Transform Billboard;
        [Tooltip("the TextMesh used to display the value")]
        public TextMesh TargetTextMesh;
        
        [Header("Debug")]
        [Tooltip("the direction of this floating text, used for debug only")]
        [MMReadOnly]
        public Vector3 Direction = Vector3.up;

        protected bool _useUnscaledTime = false;
        public virtual float GetTime() { return (_useUnscaledTime) ? Time.unscaledTime : Time.time; }
        public virtual float GetDeltaTime() { return _useUnscaledTime ? Time.unscaledDeltaTime : Time.unscaledTime; }
       
        protected float _startedAt;
        protected float _lifetime;
        protected Vector3 _newPosition;
        protected Color _initialTextColor;
        protected bool _animateMovement;
        protected bool _animateX;
        protected AnimationCurve _animateXCurve;
        protected float _remapXZero;
        protected float _remapXOne;
        protected bool _animateY;
        protected AnimationCurve _animateYCurve;
        protected float _remapYZero;
        protected float _remapYOne;
        protected bool _animateZ;
        protected AnimationCurve _animateZCurve;
        protected float _remapZZero;
        protected float _remapZOne;
        protected MMFloatingTextSpawner.AlignmentModes _alignmentMode;
        protected Vector3 _fixedAlignment;
        protected Vector3 _movementDirection;
        protected Vector3 _movingPartPositionLastFrame;
        protected bool _alwaysFaceCamera;
        protected Camera _targetCamera;
        protected Quaternion _targetCameraRotation;
        protected bool _animateOpacity;
        protected AnimationCurve _animateOpacityCurve;
        protected float _remapOpacityZero;
        protected float _remapOpacityOne;
        protected bool _animateScale;
        protected AnimationCurve _animateScaleCurve;
        protected float _remapScaleZero;
        protected float _remapScaleOne;
        protected bool _animateColor;
        protected Gradient _animateColorGradient;
        protected Vector3 _newScale;
        protected Color _newColor;

        protected float _elapsedTime;
        protected float _remappedTime;
        protected virtual void OnEnable()
        {
            Initialization();
        }
        public virtual void SetUseUnscaledTime(bool status, bool resetStartedAt)
        {
            _useUnscaledTime = status;
            if (resetStartedAt)
            {
                _startedAt = GetTime();    
            }
        }
        protected virtual void Initialization()
        {
            _startedAt = GetTime();
            if (TargetTextMesh != null)
            {
                _initialTextColor = TargetTextMesh.color;
            }            
        }
        protected virtual void Update()
        {
            UpdateFloatingText();
        }
        protected virtual void UpdateFloatingText()
        {
            
            _elapsedTime = GetTime() - _startedAt;
            _remappedTime = MMMaths.Remap(_elapsedTime, 0f, _lifetime, 0f, 1f);
            if (_elapsedTime > _lifetime)
            {
                TurnOff();
            }

            HandleMovement();
            HandleColor();
            HandleOpacity();
            HandleScale();
            HandleAlignment();            
            HandleBillboard();
        }
        protected virtual void HandleMovement()
        {
            if (_animateMovement)
            {
                this.transform.up = Direction;

                _newPosition.x = _animateX ? MMMaths.Remap(_animateXCurve.Evaluate(_remappedTime), 0f, 1, _remapXZero, _remapXOne) : 0f;
                _newPosition.y = _animateY ? MMMaths.Remap(_animateYCurve.Evaluate(_remappedTime), 0f, 1, _remapYZero, _remapYOne) : 0f;
                _newPosition.z = _animateZ ? MMMaths.Remap(_animateZCurve.Evaluate(_remappedTime), 0f, 1, _remapZZero, _remapZOne) : 0f;
                MovingPart.transform.localPosition = _newPosition;
                if (Vector3.Distance(_movingPartPositionLastFrame, MovingPart.position) > 0.5f)
                {
                    _movingPartPositionLastFrame = MovingPart.position;
                }
            }
        }
        protected virtual void HandleColor()
        {
            if (_animateColor)
            {
                _newColor = _animateColorGradient.Evaluate(_remappedTime);
                SetColor(_newColor);
            }
        }
        protected virtual void HandleOpacity()
        {
            if (_animateOpacity)
            {
                float newOpacity = MMMaths.Remap(_animateOpacityCurve.Evaluate(_remappedTime), 0f, 1f, _remapOpacityZero, _remapOpacityOne);
                SetOpacity(newOpacity);
            }
        }
        protected virtual void HandleScale()
        {
            if (_animateScale)
            {
                _newScale = Vector3.one * MMMaths.Remap(_animateScaleCurve.Evaluate(_remappedTime), 0f, 1f, _remapScaleZero, _remapScaleOne);
                MovingPart.transform.localScale = _newScale;
            }
        }
        protected virtual void HandleAlignment()
        {
            if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.Fixed)
            {
                MovingPart.transform.up = _fixedAlignment;
            }
            else if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.MatchInitialDirection)
            {
                MovingPart.transform.up = this.transform.up;
            }
            else if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.MatchMovementDirection)
            {
                _movementDirection = MovingPart.position - _movingPartPositionLastFrame;
                MovingPart.transform.up = _movementDirection.normalized;
            }
        }
        protected virtual void HandleBillboard()
        {
            if (_alwaysFaceCamera)
            {
                _targetCameraRotation = _targetCamera.transform.rotation;
                Billboard.transform.LookAt(MovingPart.transform.position + _targetCameraRotation * Vector3.forward, _targetCameraRotation * MovingPart.up);
            }
        }
        public virtual void SetProperties(string value, float lifetime, Vector3 direction, bool animateMovement, 
                                            MMFloatingTextSpawner.AlignmentModes alignmentMode, Vector3 fixedAlignment,
                                            bool alwaysFaceCamera, Camera targetCamera,
                                            bool animateX, AnimationCurve animateXCurve, float remapXZero, float remapXOne,
                                            bool animateY, AnimationCurve animateYCurve, float remapYZero, float remapYOne,
                                            bool animateZ, AnimationCurve animateZCurve, float remapZZero, float remapZOne,
                                            bool animateOpacity, AnimationCurve animateOpacityCurve, float remapOpacityZero, float remapOpacityOne,
                                            bool animateScale, AnimationCurve animateScaleCurve, float remapScaleZero, float remapScaleOne,
                                            bool animateColor, Gradient animateColorGradient)
        {
            SetText(value);
            _lifetime = lifetime;
            Direction = direction;
            _animateMovement = animateMovement;
            _animateX =  animateX;
            _animateXCurve =  animateXCurve;
            _remapXZero =  remapXZero;
            _remapXOne =  remapXOne;
            _animateY =  animateY;
            _animateYCurve =  animateYCurve;
            _remapYZero =  remapYZero;
            _remapYOne =  remapYOne;
            _animateZ =  animateZ;
            _animateZCurve =  animateZCurve;
            _remapZZero =  remapZZero;
            _remapZOne =  remapZOne;
            _alignmentMode = alignmentMode;
            _fixedAlignment = fixedAlignment;
            _alwaysFaceCamera = alwaysFaceCamera;
            _targetCamera = targetCamera;
            _animateOpacity = animateOpacity;
            _animateOpacityCurve = animateOpacityCurve;
            _remapOpacityZero = remapOpacityZero;
            _remapOpacityOne = remapOpacityOne;
            _animateScale = animateScale;
            _animateScaleCurve = animateScaleCurve;
            _remapScaleZero = remapScaleZero;
            _remapScaleOne = remapScaleOne;
            _animateColor = animateColor;
            _animateColorGradient = animateColorGradient;
            UpdateFloatingText();
        }
        public virtual void ResetPosition()
        {
            if (_animateMovement)
            {
                MovingPart.transform.localPosition = Vector3.zero;    
            }
            _movingPartPositionLastFrame = MovingPart.position - Direction;
        }
        public virtual void SetText(string newValue)
        {
            TargetTextMesh.text = newValue;
        }
        public virtual void SetColor(Color newColor)
        {
            TargetTextMesh.color = newColor;
        }
        public virtual void SetOpacity(float newOpacity)
        {
            _newColor = TargetTextMesh.color;
            _newColor.a = newOpacity;
            TargetTextMesh.color = _newColor;
        }
        protected virtual void TurnOff()
        {
            this.gameObject.SetActive(false);
        }
    }
}
