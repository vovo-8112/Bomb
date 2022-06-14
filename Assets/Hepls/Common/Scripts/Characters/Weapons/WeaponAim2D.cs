using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Weapon))]
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Aim 2D")]
    public class WeaponAim2D : WeaponAim
    {
        protected Vector2 _inputMovement;
        protected Camera _mainCamera;
        protected override void Initialization()
        {
            base.Initialization();
            _mainCamera = Camera.main;
            if (_weapon.Owner?.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterOrientation2D>() != null)
            {
                switch (_weapon.Owner?.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterOrientation2D>().CurrentFacingDirection)
                {
                    case Character.FacingDirections.East:
                        _lastNonNullMovement = Vector2.right;
                        break;
                    case Character.FacingDirections.North:
                        _lastNonNullMovement = Vector2.up;
                        break;
                    case Character.FacingDirections.West:
                        _lastNonNullMovement = Vector2.left;
                        break;
                    case Character.FacingDirections.South:
                        _lastNonNullMovement = Vector2.down;
                        break;
                }
            }
        }
        public override float CurrentAngleRelative
        {
            get
            {
                if (_weapon != null)
                {
                    if (_weapon.Owner != null)
                    {
                        if (_weapon.Owner.Orientation2D != null)
                        {
                            if (_weapon.Owner.Orientation2D.IsFacingRight)
                            {
                                return CurrentAngle;
                            }
                            else
                            {
                                return -CurrentAngle;
                            }                        
                        }
                    }
                }
                return 0;
            }
        }
        protected override void GetCurrentAim()
        {
            if (_weapon.Owner == null)
            {
                return;
            }

            if ((_weapon.Owner.LinkedInputManager == null) && (_weapon.Owner.CharacterType == Character.CharacterTypes.Player))
            {
                return;
            }

            AutoDetectWeaponMode();

            switch (AimControl)
            {
                case AimControls.Off:
                    if (_weapon.Owner == null) { return; }
                    GetOffAim();
                    break;

                case AimControls.Script:
                    GetScriptAim();
                    break;

                case AimControls.PrimaryMovement:
                    if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
                    {
                        return;
                    }
                    GetPrimaryMovementAim();                    
                    break;

                case AimControls.SecondaryMovement:
                    if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
                    {
                        return;
                    }
                    GetSecondaryMovementAim();                    
                    break;                    

                case AimControls.PrimaryThenSecondaryMovement:
                    if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
                    {
                        return;
                    }

                    if (_weapon.Owner.LinkedInputManager.PrimaryMovement.magnitude > MinimumMagnitude)
                    {
                        GetPrimaryMovementAim();
                    }
                    else
                    {
                        GetSecondaryMovementAim();
                    }
                    break;

                case AimControls.SecondaryThenPrimaryMovement:
                    if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
                    {
                        return;
                    }

                    if (_weapon.Owner.LinkedInputManager.SecondaryMovement.magnitude > MinimumMagnitude)
                    {
                        GetSecondaryMovementAim();
                    }
                    else
                    {
                        GetPrimaryMovementAim();
                    }

                    break;

                case AimControls.Mouse:
                    if (_weapon.Owner == null)
                    {
                        return;
                    }
                    GetMouseAim();                    
                    break;

                case AimControls.CharacterRotateCameraDirection:
                    if (_weapon.Owner == null)
                    {
                        return;
                    }
                    _currentAim = _weapon.Owner.CameraDirection;
                    _currentAimAbsolute = _weapon.Owner.CameraDirection;
                    _currentAim = (_weapon.Owner.Orientation2D.IsFacingRight) ? _currentAim : -_currentAim;
                    _direction = -(transform.position - _currentAim);
                    break;
            }
        }
        public virtual void GetOffAim()
        {
            _currentAim = Vector2.right;
            _currentAimAbsolute = Vector2.right;
            _direction = Vector2.right;
        }
        public virtual void GetScriptAim()
        {
            _currentAimAbsolute = _currentAim;
            _currentAim = (_weapon.Owner.Orientation2D.IsFacingRight) ? _currentAim : -_currentAim;
            _direction = -(transform.position - _currentAim);
        }
        public virtual void GetPrimaryMovementAim()
        {
            if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
            {
                return;
            }

            if (_lastNonNullMovement == Vector2.zero)
            {
                _lastNonNullMovement = _weapon.Owner.LinkedInputManager.LastNonNullPrimaryMovement;
            }

            _inputMovement = _weapon.Owner.LinkedInputManager.PrimaryMovement;

            TestLastMovement();


            _currentAimAbsolute = _inputMovement;

            if (_weapon.Owner.Orientation2D.IsFacingRight)
            {
                _currentAim = _inputMovement;
                _direction = transform.position + _currentAim;
            }
            else
            {
                _currentAim = -_inputMovement;
                _direction = -(transform.position - _currentAim);
            }

            StoreLastMovement();
        }
        public virtual void GetSecondaryMovementAim()
        {
            if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
            {
                return;
            }

            if (_lastNonNullMovement == Vector2.zero)
            {
                _lastNonNullMovement = _weapon.Owner.LinkedInputManager.LastNonNullSecondaryMovement;
            }

            _inputMovement = _weapon.Owner.LinkedInputManager.SecondaryMovement;
            TestLastMovement();

            _currentAimAbsolute = _inputMovement;

            if (_weapon.Owner.Orientation2D.IsFacingRight)
            {
                _currentAim = _inputMovement;
                _direction = transform.position + _currentAim;
            }
            else
            {
                _currentAim = -_inputMovement;
                _direction = -(transform.position - _currentAim);
            }
            StoreLastMovement();
        }
        public virtual void GetMouseAim()
        {
            _mousePosition = Input.mousePosition;
            _mousePosition.z = 10;

            _direction = _mainCamera.ScreenToWorldPoint(_mousePosition);
            _direction.z = _weapon.Owner.transform.position.z;

            _reticlePosition = _direction;

            _currentAimAbsolute = _direction - _weapon.Owner.transform.position;

            _currentAim = _direction - _weapon.Owner.transform.position;
            if (_weapon.Owner.Orientation2D != null)
            {
                if (_weapon.Owner.Orientation2D.IsFacingRight)
                {
                    _currentAim = _direction - _weapon.Owner.transform.position;
                    _currentAimAbsolute = _currentAim;
                }
                else
                {
                    _currentAim = _weapon.Owner.transform.position - _direction;
                }
            }            
        }
        protected virtual void TestLastMovement()
        {
            if (RotationMode == RotationModes.Strict2Directions)
            {
                _inputMovement.x = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullMovement.x;
                _inputMovement.y = Mathf.Abs(_inputMovement.y) > 0 ? _inputMovement.y : _lastNonNullMovement.y;
            }            
            else
            {
                _inputMovement = _inputMovement.magnitude > 0 ? _inputMovement : _lastNonNullMovement;
            }
        }
        protected virtual void StoreLastMovement()
        {
            if (RotationMode == RotationModes.Strict2Directions)
            {
                _lastNonNullMovement.x = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullMovement.x;
                _lastNonNullMovement.y = Mathf.Abs(_inputMovement.y) > 0 ? _inputMovement.y : _lastNonNullMovement.y;
            }
            else
            {
                _lastNonNullMovement = _inputMovement.magnitude > 0 ? _inputMovement : _lastNonNullMovement;
            }
        }
        protected override void Update()
        {
            HideMousePointer();
            HideReticle();
            if (GameManager.Instance.Paused)
            {
                return;
            }
            GetCurrentAim();
            DetermineWeaponRotation();
        }
        protected virtual void FixedUpdate()
        {
            if (GameManager.Instance.Paused)
            {
                return;
            }
            MoveTarget();
            MoveReticle();
        }
        protected override void DetermineWeaponRotation()
        {
            if (_currentAim != Vector3.zero)
            {
                if (_direction != Vector3.zero)
                {
                    CurrentAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;
                    CurrentAngleAbsolute = Mathf.Atan2(_currentAimAbsolute.y, _currentAimAbsolute.x) * Mathf.Rad2Deg;
                    if (RotationMode == RotationModes.Strict4Directions || RotationMode == RotationModes.Strict8Directions)
                    {
                        CurrentAngle = MMMaths.RoundToClosest(CurrentAngle, _possibleAngleValues);
                    }
                    if (RotationMode == RotationModes.Strict2Directions)
                    {
                        CurrentAngle = 0f;
                    }
                    CurrentAngle += _additionalAngle;
                    if (_weapon.Owner.Orientation2D != null)
                    {
                        if (_weapon.Owner.Orientation2D.IsFacingRight)
                        {
                            CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
                        }
                        else
                        {
                            CurrentAngle = Mathf.Clamp(CurrentAngle, -MaximumAngle, -MinimumAngle);
                        }
                    }
                    else
                    {
                        CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
                    }
                    
                    _lookRotation = Quaternion.Euler(CurrentAngle * Vector3.forward);
                    RotateWeapon(_lookRotation);
                }
            }
            else
            {
                CurrentAngle = 0f;
                RotateWeapon(_initialRotation);
            }
            MMDebug.DebugDrawArrow(this.transform.position, _currentAimAbsolute.normalized, Color.green);
        }
        protected override void InitializeReticle()
        {
            if (_weapon.Owner == null) { return; }
            if (Reticle == null) { return; }
            if (ReticleType == ReticleTypes.None) { return; }

            if (ReticleType == ReticleTypes.Scene)
            {
                _reticle = (GameObject)Instantiate(Reticle);

                if (!ReticleAtMousePosition)
                {
                    if (_weapon.Owner != null)
                    {
                        _reticle.transform.SetParent(_weapon.transform);
                        _reticle.transform.localPosition = ReticleDistance * Vector3.right;
                    }
                }                
            }

            if (ReticleType == ReticleTypes.UI)
            {
                _reticle = (GameObject)Instantiate(Reticle);
                _reticle.transform.SetParent(GUIManager.Instance.MainCanvas.transform);
                _reticle.transform.localScale = Vector3.one;
                if (_reticle.gameObject.MMGetComponentNoAlloc<MMUIFollowMouse>() != null)
                {
                    _reticle.gameObject.MMGetComponentNoAlloc<MMUIFollowMouse>().TargetCanvas = GUIManager.Instance.MainCanvas;
                }
            }
        }
        protected override void MoveReticle()
        {
            if (ReticleType == ReticleTypes.None) { return; }
            if (_reticle == null) { return; }
            if (_weapon.Owner.ConditionState.CurrentState == CharacterStates.CharacterConditions.Paused) { return; }

            if (ReticleType == ReticleTypes.Scene)
            {
                if (!RotateReticle)
                {
                    _reticle.transform.rotation = Quaternion.identity;
                }
                else
                {
                    if (ReticleAtMousePosition)
                    {
                        _reticle.transform.rotation = _lookRotation;
                    }
                }
                if (ReticleAtMousePosition && AimControl == AimControls.Mouse)
                {
                    _reticle.transform.position = _reticlePosition;
                }                
            }
        }
        protected override void MoveTarget()
        {
            if (_weapon.Owner == null)
            {
                return;
            }
            
            if (MoveCameraTargetTowardsReticle)
            {
                if (ReticleType != ReticleTypes.None)
                {
                    _newCamTargetPosition = _reticlePosition;
                    _newCamTargetDirection = _newCamTargetPosition - this.transform.position;
                    if (_newCamTargetDirection.magnitude > CameraTargetMaxDistance)
                    {
                        _newCamTargetDirection = _newCamTargetDirection.normalized * CameraTargetMaxDistance;
                    }

                    _newCamTargetPosition = this.transform.position + _newCamTargetDirection;

                    _newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position,
                        Vector3.Lerp(this.transform.position, _newCamTargetPosition, CameraTargetOffset),
                        Time.deltaTime * CameraTargetSpeed);

                    _weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
                }
                else
                {
                    _newCamTargetPosition = this.transform.position + _currentAimAbsolute.normalized * CameraTargetMaxDistance;
                    _newCamTargetDirection = _newCamTargetPosition - this.transform.position;
		            
                    _newCamTargetPosition = this.transform.position + _newCamTargetDirection;

                    _newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position, Vector3.Lerp(this.transform.position, _newCamTargetPosition, CameraTargetOffset), Time.deltaTime * CameraTargetSpeed);

                    _weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
                    
                }
            }
        }
    }
}