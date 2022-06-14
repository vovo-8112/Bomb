using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMAim
    {
        public enum AimControls { Off, PrimaryMovement, SecondaryMovement, Mouse, Script }
        public enum RotationModes { Free, Strict4Directions, Strict8Directions }

        [Header("Control Mode")]
        [MMInformation("Pick a control mode : mouse (aims towards the pointer), primary movement (you'll aim towards the current input direction), or secondary movement (aims " +
            "towards a second input axis, think twin stick shooters), and set minimum and maximum angles.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        public AimControls AimControl = AimControls.SecondaryMovement;
		public RotationModes RotationMode = RotationModes.Free;

        [Header("Limits")]
        [Range(-180, 180)]
        public float MinimumAngle = -180f;
        [Range(-180, 180)]
        public float MaximumAngle = 180f;
        [MMReadOnly]
        public float CurrentAngle;

        public Vector3 CurrentPosition { get; set; }
        public Vector2 PrimaryMovement { get; set; }
        public Vector2 SecondaryMovement { get; set; }

        protected float[] _possibleAngleValues;
        protected Vector3 _currentAim = Vector3.zero;
        protected Vector3 _direction;
        protected Vector3 _mousePosition;

        protected Camera _mainCamera;
		public virtual void Initialization()
        {
            if (RotationMode == RotationModes.Strict4Directions)
            {
                _possibleAngleValues = new float[5];
                _possibleAngleValues[0] = -180f;
                _possibleAngleValues[1] = -90f;
                _possibleAngleValues[2] = 0f;
                _possibleAngleValues[3] = 90f;
                _possibleAngleValues[4] = 180f;
            }
            if (RotationMode == RotationModes.Strict8Directions)
            {
                _possibleAngleValues = new float[9];
                _possibleAngleValues[0] = -180f;
                _possibleAngleValues[1] = -135f;
                _possibleAngleValues[2] = -90f;
                _possibleAngleValues[3] = -45f;
                _possibleAngleValues[4] = 0f;
                _possibleAngleValues[5] = 45f;
                _possibleAngleValues[6] = 90f;
                _possibleAngleValues[7] = 135f;
                _possibleAngleValues[8] = 180f;
            }

            _mainCamera = Camera.main;
        }
		public virtual Vector2 GetCurrentAim()
        {
            switch (AimControl)
            {
                case AimControls.Off:
                    _currentAim = Vector2.zero;
                    break;

                case AimControls.Script:
                    break;

                case AimControls.PrimaryMovement:
                    _currentAim = PrimaryMovement;
                    break;

                case AimControls.SecondaryMovement:
                    _currentAim = SecondaryMovement;
                    break;

                case AimControls.Mouse:
                    _mousePosition = Input.mousePosition;
                    _mousePosition.z = 10;
                    _direction = _mainCamera.ScreenToWorldPoint(_mousePosition);
                    _direction.z = CurrentPosition.z;
                    _currentAim = _direction - CurrentPosition;
                    break;

                default:
                    _currentAim = Vector2.zero;
                    break;
            }
            CurrentAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;
            if ((CurrentAngle < MinimumAngle) || (CurrentAngle > MaximumAngle))
            {
                float minAngleDifference = Mathf.DeltaAngle(CurrentAngle, MinimumAngle);
                float maxAngleDifference = Mathf.DeltaAngle(CurrentAngle, MaximumAngle);
                CurrentAngle = (Mathf.Abs(minAngleDifference) < Mathf.Abs(maxAngleDifference)) ? MinimumAngle : MaximumAngle;
            }
            if (RotationMode == RotationModes.Strict4Directions || RotationMode == RotationModes.Strict8Directions)
            {
                CurrentAngle = MMMaths.RoundToClosest(CurrentAngle, _possibleAngleValues);
            }
            CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
            _currentAim = (_currentAim.magnitude == 0f) ? Vector2.zero : MMMaths.RotateVector2(Vector2.right, CurrentAngle);
            return _currentAim;
        }
        public virtual void SetAim(Vector2 newAim)
        {
            _currentAim = newAim;
        }
    }
}
