using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools 
{
    [AddComponentMenu("More Mountains/Tools/Camera/MMGhostCamera")]
    public class MMGhostCamera : MonoBehaviour
    {
        [Header("Speed")]
        public float MovementSpeed = 10f;
        public float RunFactor = 4f;
        public float Acceleration = 5f;
        public float Deceleration = 5f;
        public float RotationSpeed = 40f;

        [Header("Controls")]
        public KeyCode ActivateButton = KeyCode.RightShift;
        public string HorizontalAxisName = "Horizontal";
        public string VerticalAxisName = "Vertical";
        public KeyCode UpButton = KeyCode.Space;
        public KeyCode DownButton = KeyCode.C;
        public KeyCode ControlsModeSwitch = KeyCode.M;
        public KeyCode TimescaleModificationButton = KeyCode.F;
        public KeyCode RunButton = KeyCode.LeftShift;
        public float MouseSensitivity = 0.02f;
        public float MobileStickSensitivity = 2f;

        [Header("Timescale Modification")]
        public float TimescaleModifier = 0.5f;


        [Header("Settings")]
        public bool AutoActivation = true;
        public bool MovementEnabled = true;
        public bool RotationEnabled = true;
        [MMReadOnly]
        public bool Active = false;
        [MMReadOnly]
        public bool TimeAltered = false;

        [Header("Virtual Joysticks")]
        public bool UseMobileControls;
        [MMCondition("UseMobileControls", true)]
        public GameObject LeftStickContainer;
        [MMCondition("UseMobileControls", true)]
        public GameObject RightStickContainer;
        [MMCondition("UseMobileControls", true)]
        public MMTouchJoystick LeftStick;
        [MMCondition("UseMobileControls", true)]
        public MMTouchJoystick RightStick;

        protected Vector3 _currentInput;
        protected Vector3 _lerpedInput;
        protected Vector3 _normalizedInput;
        protected float _acceleration;
        protected float _deceleration;
        protected Vector3 _movementVector = Vector3.zero;
        protected float _speedMultiplier;
        protected Vector3 _newEulerAngles;
        protected virtual void Start()
        {
            if (AutoActivation)
            {
                ToggleFreeCamera();
            }
        }
        protected virtual void Update()
        {
            if (Input.GetKeyDown(ActivateButton))
            {
                ToggleFreeCamera();
            }

            if (!Active)
            {
                return;
            }

            GetInput();
            Translate();
            Rotate();
            Move();

            HandleMobileControls();
        }
        protected virtual void GetInput()
        {
            if (!UseMobileControls || (LeftStick == null))
            {
                _currentInput.x = Input.GetAxis("Horizontal");
                _currentInput.y = 0f;
                _currentInput.z = Input.GetAxis("Vertical");
            }
            else
            {
                _currentInput.x = LeftStick._joystickValue.x;
                _currentInput.y = 0f;
                _currentInput.z = LeftStick._joystickValue.y;
            }

            if (Input.GetKey(UpButton))
            {
                _currentInput.y = 1f; 
            }
            if (Input.GetKey(DownButton))
            {
                _currentInput.y = -1f;
            }

            _speedMultiplier = Input.GetKey(RunButton) ? RunFactor : 1f;
            _normalizedInput = _currentInput.normalized;
            
            if (Input.GetKeyDown(TimescaleModificationButton))
            {
                ToggleSlowMotion();
            }
        }
        protected virtual void HandleMobileControls()
        {
            if (Input.GetKeyDown(ControlsModeSwitch))
            {
                UseMobileControls = !UseMobileControls;
            }
            if (UseMobileControls)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Active)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (LeftStickContainer != null)
            {
                LeftStickContainer?.SetActive(UseMobileControls);
            }
            if (RightStickContainer != null)
            {
                RightStickContainer?.SetActive(UseMobileControls);
            }
        }
        protected virtual void Translate()
        {
            if (!MovementEnabled)
            {
                return;
            }

            if ((Acceleration == 0) || (Deceleration == 0))
            {
                _lerpedInput = _currentInput;
            }
            else
            {
                if (_normalizedInput.magnitude == 0)
                {
                    _acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * Time.deltaTime);
                    _lerpedInput = Vector3.Lerp(_lerpedInput, _lerpedInput * _acceleration, Time.deltaTime * Deceleration);
                }
                else
                {
                    _acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * Time.deltaTime);
                    _lerpedInput = Vector3.ClampMagnitude(_normalizedInput, _acceleration);
                }
            }

            _movementVector = _lerpedInput;
            _movementVector *= MovementSpeed * _speedMultiplier;

            if (_movementVector.magnitude > MovementSpeed * _speedMultiplier)
            {
                _movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed * _speedMultiplier);
            }
        }
        protected virtual void Rotate()
        {
            if (!RotationEnabled)
            {
                return;
            }
            _newEulerAngles = this.transform.eulerAngles;

            if (!UseMobileControls || (LeftStick == null))
            {
                _newEulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * MouseSensitivity;
                _newEulerAngles.y += Input.GetAxis("Mouse X") * 359f * MouseSensitivity;
            }
            else
            {
                _newEulerAngles.x += -RightStick._joystickValue.y * MobileStickSensitivity;
                _newEulerAngles.y += RightStick._joystickValue.x * MobileStickSensitivity;
            }                

            _newEulerAngles = Vector3.Lerp(this.transform.eulerAngles, _newEulerAngles, Time.deltaTime * RotationSpeed);
        }
        protected virtual void Move()
        {
            transform.eulerAngles = _newEulerAngles;
            transform.position += transform.rotation * _movementVector * Time.deltaTime;
        }
        protected virtual void ToggleSlowMotion()
        {
            TimeAltered = !TimeAltered;
            if (TimeAltered)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimescaleModifier, 1f, true, 5f, true);
            }
            else
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);

            }
        }
        protected virtual void ToggleFreeCamera()
        {
            Active = !Active;
            Cursor.lockState = Active ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !Active;
        }
    }
}