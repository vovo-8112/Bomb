using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Rotate Camera")]
    public class CharacterRotateCamera : CharacterAbility
    {
		public override string HelpBoxText() { return "An ability that will let the Character rotate its associated camera, using the PlayerID_CameraRotationAxis input axis"; }

        [Header("Rotation axis")]
        [Tooltip("the space in which to rotate the camera (usually world)")]
        public Space RotationSpace = Space.World;
        [Tooltip("the camera's forward vector, usually 0,0,1")]
        public Vector3 RotationForward = Vector3.forward;
        [Tooltip("the axis on which to rotate the camera (usually 0,1,0 in 3D, 0,0,1 in 2D)")]
        public Vector3 RotationAxis = Vector3.up;

        [Header("Camera Speed")]
        [Tooltip("the speed at which the camera should rotate")]
        public float CameraRotationSpeed = 3f;
        [Tooltip("the speed at which the camera should interpolate towards its target position")]
        public float CameraInterpolationSpeed = 0.2f;

        [Header("Input Manager")]
        [Tooltip("if this is false, this ability won't read input")]
        public bool InputAuthorized = true;
        [Tooltip("whether or not this ability should make changes on the InputManager to set it in camera driven input mode")]
        public bool AutoSetupInputManager = true;

        protected float _requestedCameraAngle = 0f;
        protected Camera _mainCamera;
        protected CinemachineBrain _brain;
        protected CinemachineVirtualCamera _virtualCamera;
        protected float _targetRotationAngle;
        protected Vector3 _cameraDirection;
        protected float _cameraDirectionAngle;
        protected override void Initialization()
        {
            base.Initialization();
            GetCurrentCamera();
            if (AutoSetupInputManager)
            {
                _inputManager.RotateInputBasedOnCameraDirection = true;
                bool camera3D = (_character.CharacterDimension == Character.CharacterDimensions.Type3D);
                _inputManager.SetCamera(_mainCamera, camera3D);
            }
        }
        protected virtual void GetCurrentCamera()
        {
            _mainCamera = Camera.main;
            _brain = _mainCamera.GetComponent<CinemachineBrain>();
            if (_brain != null)
            {
                _virtualCamera = _brain.ActiveVirtualCamera as CinemachineVirtualCamera;
            }
        }
        public virtual void SetCameraAngle(float newAngle)
        {
            _requestedCameraAngle = newAngle;
        }
        protected override void HandleInput()
        {
            base.HandleInput();
            if (!InputAuthorized)
            {
                return;
            }
            _requestedCameraAngle = _inputManager.CameraRotationInput * CameraRotationSpeed;
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            if (!AbilityAuthorized)
            {
                return;
            }
            RotateCamera();
        }
        protected virtual void RotateCamera()
        {
            _targetRotationAngle = MMMaths.Lerp(_targetRotationAngle, _requestedCameraAngle, CameraInterpolationSpeed, Time.deltaTime);

            if (_virtualCamera != null)
            {
                _virtualCamera.transform.Rotate(RotationAxis, _targetRotationAngle, RotationSpace);
                _cameraDirectionAngle = (_character.CharacterDimension == Character.CharacterDimensions.Type3D) ? _virtualCamera.transform.localEulerAngles.y : _virtualCamera.transform.localEulerAngles.z;

            }
            else  if (_mainCamera != null)
            {
                _mainCamera.transform.Rotate(RotationAxis, _targetRotationAngle, RotationSpace);
                _cameraDirectionAngle = (_character.CharacterDimension == Character.CharacterDimensions.Type3D) ? _mainCamera.transform.localEulerAngles.y : _mainCamera.transform.localEulerAngles.z;
            }
            
            _cameraDirection = Quaternion.AngleAxis(_cameraDirectionAngle, RotationAxis) * RotationForward;
            _character.SetCameraDirection(_cameraDirection);
        }
    }
}

