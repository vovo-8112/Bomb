using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Weapon))]
    public abstract class WeaponAim : MonoBehaviour
    {
        public enum AimControls { Off, PrimaryMovement, SecondaryMovement, Mouse, Script, SecondaryThenPrimaryMovement, PrimaryThenSecondaryMovement, CharacterRotateCameraDirection }
        public enum RotationModes { Free, Strict2Directions, Strict4Directions, Strict8Directions }
        public enum ReticleTypes { None, Scene, UI }

        [Header("Control Mode")]
        [MMInformation("Add this component to a Weapon and you'll be able to aim (rotate) it. It supports three different control modes : mouse (the weapon aims towards the pointer), primary movement (you'll aim towards the current input direction), or secondary movement (aims towards a second input axis, think twin stick shooters).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the selected aim control mode")]
        public AimControls AimControl = AimControls.SecondaryMovement;

        [Header("Weapon Rotation")]
        [MMInformation("Here you can define whether the rotation is free, strict in 4 directions (top, bottom, left, right), or 8 directions (same + diagonals). You can also define a rotation speed, and a min and max angle. For example, if you don't want your character to be able to aim in its back, set min angle to -90 and max angle to 90.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the rotation mode")]
        public RotationModes RotationMode = RotationModes.Free;
        [Tooltip("the the speed at which the weapon reaches its new position. Set it to zero if you want movement to directly follow input")]
        public float WeaponRotationSpeed = 1f;
        [Range(-180, 180)]
        [Tooltip("the minimum angle at which the weapon's rotation will be clamped")]
        public float MinimumAngle = -180f;
        [Range(-180, 180)]
        [Tooltip("the maximum angle at which the weapon's rotation will be clamped")]
        public float MaximumAngle = 180f;
        [Tooltip("the minimum threshold at which the weapon's rotation magnitude will be considered ")]
        public float MinimumMagnitude = 0.2f;

        [Header("Reticle")]
        [MMInformation("You can also display a reticle on screen to check where you're aiming at. Leave it blank if you don't want to use one. If you set the reticle distance to 0, it'll follow the cursor, otherwise it'll be on a circle centered on the weapon. You can also ask it to follow the mouse, even replace the mouse pointer. You can also decide if the pointer should rotate to reflect aim angle or remain stable.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("Defines whether the reticle is placed in the scene or in the UI")]
        public ReticleTypes ReticleType = ReticleTypes.None;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("the gameobject to display as the aim's reticle/crosshair. Leave it blank if you don't want a reticle")]
        public GameObject Reticle;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("the distance at which the reticle will be from the weapon")]
        public float ReticleDistance;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("the height at which the reticle should position itself above the ground, when in Scene mode")]
        public float ReticleHeight;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("if set to true, the reticle will be placed at the mouse's position (like a pointer)")]
        public bool ReticleAtMousePosition;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("if set to true, the reticle will rotate on itself to reflect the weapon's rotation. If not it'll remain stable.")]
        public bool RotateReticle = false;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("if set to true, the reticle will replace the mouse pointer")]
        public bool ReplaceMousePointer = true;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("the radius around the weapon rotation centre where the mouse will be ignored, to avoid glitches")]
        public float MouseDeadZoneRadius = 0.5f;
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        [Tooltip("if set to false, the reticle won't be added and displayed")]
        public bool DisplayReticle = true;

        [Header("CameraTarget")]
        [Tooltip("whether the camera target should be moved towards the reticle to provide a better vision of the possible target. If you don't have a reticle, it'll be moved towards your aim direction.")]
        public bool MoveCameraTargetTowardsReticle = false;
        [Range(0f, 1f)]
        [Tooltip("the offset to apply to the camera target along the transform / reticle line")]
        public float CameraTargetOffset = 0.3f;
        [MMCondition("MoveCameraTargetTowardsReticle", true)]
        [Tooltip("the maximum distance at which to move the camera target from the weapon")]
        public float CameraTargetMaxDistance = 10f;
        [MMCondition("MoveCameraTargetTowardsReticle", true)]
        [Tooltip("the speed at which the camera target should be moved")]
        public float CameraTargetSpeed = 5f;

        public float CurrentAngleAbsolute { get; protected set; }
		public Quaternion CurrentRotation { get { return transform.rotation; } }
        public Vector3 CurrentAim { get { return _currentAim; } }
        public Vector3 CurrentAimAbsolute { get { return _currentAimAbsolute; } }
        public float CurrentAngle { get; protected set; }
        public virtual float CurrentAngleRelative
        {
            get
            {
                if (_weapon != null)
                {
                    if (_weapon.Owner != null)
                    {
                        return CurrentAngle;
                    }
                }
                return 0;
            }
        }
        
        protected Vector2 _lastNonNullMovement;
        protected Weapon _weapon;
        protected Vector3 _currentAim = Vector3.zero;
        protected Vector3 _currentAimAbsolute = Vector3.zero;
        protected Quaternion _lookRotation;
        protected Vector3 _direction;
        protected float[] _possibleAngleValues;
        protected Vector3 _mousePosition;
        protected Vector3 _lastMousePosition;
        protected float _additionalAngle;
        protected Quaternion _initialRotation;
        protected Plane _playerPlane;
        protected GameObject _reticle;
        protected Vector3 _reticlePosition;
        protected Vector3 _newCamTargetPosition;
        protected Vector3 _newCamTargetDirection;
        protected virtual void Start()
        {
            Initialization();
        }
		protected virtual void Initialization()
        {
            _weapon = GetComponent<Weapon>();

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
            _initialRotation = transform.rotation;
            InitializeReticle();
            _playerPlane = new Plane(Vector3.up, Vector3.zero);
        }
		public virtual void SetCurrentAim(Vector3 newAim)
        {
            _currentAim = newAim;
        }


        protected virtual void GetCurrentAim()
        {

        }
        protected virtual void Update()
        {

        }
        protected virtual void LateUpdate()
        {
            ResetAdditionalAngle();
        }
        protected virtual void DetermineWeaponRotation()
        {

        }
        protected virtual void MoveReticle()
        {

        }
        protected virtual void RotateWeapon(Quaternion newRotation)
        {
            if (GameManager.Instance.Paused)
            {
                return;
            }
            if (WeaponRotationSpeed == 0f)
            {
                transform.rotation = newRotation;
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, WeaponRotationSpeed * Time.deltaTime);
            }
        }

        protected Vector3 _aimAtDirection;
        protected Quaternion _aimAtQuaternion;
        
        protected virtual void AimAt(Vector3 target)
        {
        }
		protected virtual void InitializeReticle()
        {
           
        }
        protected virtual void MoveTarget()
        {

        }
        public virtual void RemoveReticle()
        {
            if (_reticle != null)
            {
                Destroy(_reticle.gameObject);
            }
        }
        protected virtual void HideReticle()
        {
            if (_reticle != null)
            {
                if (GameManager.Instance.Paused)
                {
                    _reticle.gameObject.SetActive(false);
                    return;
                }
                _reticle.gameObject.SetActive(DisplayReticle);
            }
        }
        protected virtual void HideMousePointer()
        {
            if (AimControl != AimControls.Mouse)
            {
                return;
            }
            if (GameManager.Instance.Paused)
            {
                Cursor.visible = true;
                return;
            }
            if (ReplaceMousePointer)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
            }
        }
        protected void OnDestroy()
        {
            if (ReplaceMousePointer)
            {
                Cursor.visible = true;
            }
        }
        public virtual void AddAdditionalAngle(float addedAngle)
        {
            _additionalAngle += addedAngle;
        }
        protected virtual void ResetAdditionalAngle()
        {
            _additionalAngle = 0;
        }

        protected virtual void AutoDetectWeaponMode()
        {
            if (_weapon.Owner.LinkedInputManager != null)
            {
                if ((_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (AimControl != AimControls.Off))
                {
                    AimControl = _weapon.Owner.LinkedInputManager.WeaponForcedMode;
                }

                if ((!_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (_weapon.Owner.LinkedInputManager.IsMobile) && (AimControl == AimControls.Mouse))
                {
                    AimControl = AimControls.PrimaryMovement;
                }
            }
        }
    }
}
