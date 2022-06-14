using MoreMountains.Tools;
using Photon.Pun;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Orientation 3D")]
    public class CharacterOrientation3D : CharacterAbility
    {
        public enum RotationModes
        {
            None,
            MovementDirection,
            WeaponDirection,
            Both
        }
        public enum RotationSpeeds
        {
            Instant,
            Smooth,
            SmoothAbsolute
        }

        [Header("Rotation Mode")]
        [Tooltip("whether the character should face movement direction, weapon direction, or both, or none")]
        public RotationModes RotationMode = RotationModes.None;
        [Tooltip("if this is false, no rotation will occur")]
        public bool CharacterRotationAuthorized = true;

        [Header("Movement Direction")]
        [Tooltip("If this is true, we'll rotate our model towards the direction")]
        public bool ShouldRotateToFaceMovementDirection = true;
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
        [Tooltip("the current rotation mode")]
        public RotationSpeeds MovementRotationSpeed = RotationSpeeds.Instant;
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
        [Tooltip("the object we want to rotate towards direction. If left empty, we'll use the Character's model")]
        public GameObject MovementRotatingModel;
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
        [Tooltip("the speed at which to rotate towards direction (smooth and absolute only)")]
        public float RotateToFaceMovementDirectionSpeed = 10f;
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
        [Tooltip("the threshold after which we start rotating (absolute mode only)")]
        public float AbsoluteThresholdMovement = 0.5f;
        [MMReadOnly]
        [Tooltip("the direction of the model")]
        public Vector3 ModelDirection;
        [MMReadOnly]
        [Tooltip("the direction of the model in angle values")]
        public Vector3 ModelAngles;

        [Header("Weapon Direction")]
        [Tooltip("If this is true, we'll rotate our model towards the weapon's direction")]
        public bool ShouldRotateToFaceWeaponDirection = true;
        [MMCondition("ShouldRotateToFaceWeaponDirection", true)]
        [Tooltip("the current rotation mode")]
        public RotationSpeeds WeaponRotationSpeed = RotationSpeeds.Instant;
        [MMCondition("ShouldRotateToFaceWeaponDirection", true)]
        [Tooltip("the object we want to rotate towards direction. If left empty, we'll use the Character's model")]
        public GameObject WeaponRotatingModel;
        [MMCondition("ShouldRotateToFaceWeaponDirection", true)]
        [Tooltip("the speed at which to rotate towards direction (smooth and absolute only)")]
        public float RotateToFaceWeaponDirectionSpeed = 10f;
        [MMCondition("ShouldRotateToFaceWeaponDirection", true)]
        [Tooltip("the threshold after which we start rotating (absolute mode only)")]
        public float AbsoluteThresholdWeapon = 0.5f;
        [MMCondition("ShouldRotateToFaceWeaponDirection", true)]
        [Tooltip("the threshold after which we start rotating (absolute mode only)")]
        public bool LockVerticalRotation = true;

        [Header("Animation")]
        [Tooltip("the speed at which the instant rotation animation parameter float resets to 0")]
        public float RotationSpeedResetSpeed = 2f;

        [Header("Forced Rotation")]
        [Tooltip("whether the character is being applied a forced rotation")]
        public bool ForcedRotation = false;
        [MMCondition("ForcedRotation", true)]
        [Tooltip("the forced rotation applied by an external script")]
        public Vector3 ForcedRotationDirection;

        protected CharacterHandleWeapon _characterHandleWeapon;
        protected CharacterRun _characterRun;
        protected Vector3 _lastRegisteredVelocity;
        protected Vector3 _rotationDirection;
        protected Vector3 _lastMovement = Vector3.zero;
        protected Vector3 _lastAim = Vector3.zero;
        protected Vector3 _relativeSpeed;
        protected Vector3 _remappedSpeed = Vector3.zero;
        protected Vector3 _relativeSpeedNormalized;
        protected bool _secondaryMovementTriggered = false;
        protected Quaternion _tmpRotation;
        protected Quaternion _newMovementQuaternion;
        protected Quaternion _newWeaponQuaternion;
        protected bool _shouldRotateTowardsWeapon;
        protected float _rotationSpeed;
        protected float _modelAnglesYLastFrame;
        protected Vector3 _currentDirection;
        protected Vector3 _weaponRotationDirection;
        protected const string _relativeForwardSpeedAnimationParameterName = "RelativeForwardSpeed";
        protected const string _relativeLateralSpeedAnimationParameterName = "RelativeLateralSpeed";
        protected const string _remappedForwardSpeedAnimationParameterName = "RemappedForwardSpeedNormalized";
        protected const string _remappedLateralSpeedAnimationParameterName = "RemappedLateralSpeedNormalized";
        protected const string _relativeForwardSpeedNormalizedAnimationParameterName = "RelativeForwardSpeedNormalized";
        protected const string _relativeLateralSpeedNormalizedAnimationParameterName = "RelativeLateralSpeedNormalized";
        protected const string _remappedSpeedNormalizedAnimationParameterName = "RemappedSpeedNormalized";
        protected const string _rotationSpeeddAnimationParameterName = "YRotationSpeed";
        protected int _relativeForwardSpeedAnimationParameter;
        protected int _relativeLateralSpeedAnimationParameter;
        protected int _remappedForwardSpeedAnimationParameter;
        protected int _remappedLateralSpeedAnimationParameter;
        protected int _relativeForwardSpeedNormalizedAnimationParameter;
        protected int _relativeLateralSpeedNormalizedAnimationParameter;
        protected int _remappedSpeedNormalizedAnimationParameter;
        protected int _rotationSpeeddAnimationParameter;

        [SerializeField]
        private PhotonView _photonView;
        protected override void Initialization()
        {
            base.Initialization();

            if ((_model == null) && (MovementRotatingModel == null) && (WeaponRotatingModel == null))
            {
                Debug.LogError("CharacterOrientation3D on " + this.name
                                                            + " : you need to set a CharacterModel on your Character component, and/or specify MovementRotatingModel and WeaponRotatingModel on your CharacterOrientation3D inspector. Check the documentation to learn more about this.");
            }

            if (MovementRotatingModel == null)
            {
                MovementRotatingModel = _model;
            }

            _characterRun = _character?.FindAbility<CharacterRun>();
            _characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();

            if (WeaponRotatingModel == null)
            {
                WeaponRotatingModel = _model;
            }
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();

            if ((MovementRotatingModel == null) && (WeaponRotatingModel == null))
            {
                return;
            }

            if (!AbilityAuthorized)
            {
                return;
            }

            if (CharacterRotationAuthorized)
            {
                RotateToFaceMovementDirection();
                RotateToFaceWeaponDirection();
                RotateModel();
            }
        }

        protected virtual void FixedUpdate()
        {
            ComputeRelativeSpeeds();
        }
        protected virtual void RotateToFaceMovementDirection()
        {
            if (_photonView != null)
            {
                if (!_photonView.IsMine)
                {
                    return;
                }
            }
            if (!ShouldRotateToFaceMovementDirection)
            {
                return;
            }

            if ((RotationMode != RotationModes.MovementDirection) && (RotationMode != RotationModes.Both))
            {
                return;
            }

            _currentDirection = ForcedRotation ? ForcedRotationDirection : _controller.CurrentDirection;
            if (MovementRotationSpeed == RotationSpeeds.Instant)
            {
                if (_currentDirection != Vector3.zero)
                {
                    _newMovementQuaternion = Quaternion.LookRotation(_currentDirection);
                }
            }
            if (MovementRotationSpeed == RotationSpeeds.Smooth)
            {
                if (_currentDirection != Vector3.zero)
                {
                    _tmpRotation = Quaternion.LookRotation(_currentDirection);

                    _newMovementQuaternion = Quaternion.Lerp(MovementRotatingModel.transform.rotation, _tmpRotation,
                        Time.deltaTime * RotateToFaceMovementDirectionSpeed);
                }
            }
            if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
            {
                if (_currentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
                {
                    _lastMovement = _currentDirection;
                }

                if (_lastMovement != Vector3.zero)
                {
                    _tmpRotation = Quaternion.LookRotation(_lastMovement);

                    _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation,
                        Time.deltaTime * RotateToFaceMovementDirectionSpeed);
                }
            }

            ModelDirection = MovementRotatingModel.transform.forward.normalized;
            ModelAngles = MovementRotatingModel.transform.eulerAngles;
        }
        protected virtual void RotateToFaceWeaponDirection()
        {
            if (_photonView != null)
            {
                if (!_photonView.IsMine)
                {
                    return;
                }
            }

            _newWeaponQuaternion = Quaternion.identity;
            _weaponRotationDirection = Vector3.zero;
            _shouldRotateTowardsWeapon = false;
            if (!ShouldRotateToFaceWeaponDirection)
            {
                return;
            }

            if ((RotationMode != RotationModes.WeaponDirection) && (RotationMode != RotationModes.Both))
            {
                return;
            }

            if (_characterHandleWeapon == null)
            {
                return;
            }

            if (_characterHandleWeapon.WeaponAimComponent == null)
            {
                return;
            }

            _shouldRotateTowardsWeapon = true;

            _rotationDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAim.normalized;

            if (LockVerticalRotation)
            {
                _rotationDirection.y = 0;
            }

            _weaponRotationDirection = _rotationDirection;

            MMDebug.DebugDrawArrow(this.transform.position, _rotationDirection, Color.red);
            if (WeaponRotationSpeed == RotationSpeeds.Instant)
            {
                if (_rotationDirection != Vector3.zero)
                {
                    _newWeaponQuaternion = Quaternion.LookRotation(_rotationDirection);
                }
            }
            if (WeaponRotationSpeed == RotationSpeeds.Smooth)
            {
                if (_rotationDirection != Vector3.zero)
                {
                    _newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation,
                        Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
                }
            }
            if (WeaponRotationSpeed == RotationSpeeds.SmoothAbsolute)
            {
                if (_rotationDirection.normalized.magnitude >= AbsoluteThresholdWeapon)
                {
                    _lastMovement = _rotationDirection;
                }

                if (_lastMovement != Vector3.zero)
                {
                    _tmpRotation = Quaternion.LookRotation(_lastMovement);

                    _newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation,
                        Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
                }
            }
        }
        protected virtual void RotateModel()
        {
            if (_photonView != null)
            {
                if (!_photonView.IsMine)
                {
                    return;
                }
            }

            MovementRotatingModel.transform.rotation = _newMovementQuaternion;

            if (_shouldRotateTowardsWeapon && (_weaponRotationDirection != Vector3.zero))
            {
                WeaponRotatingModel.transform.rotation = _newWeaponQuaternion;
            }
        }

        protected Vector3 _positionLastFrame;
        protected Vector3 _newSpeed;
        protected virtual void ComputeRelativeSpeeds()
        {
            if ((MovementRotatingModel == null) && (WeaponRotatingModel == null))
            {
                return;
            }

            if (Time.deltaTime != 0f)
            {
                _newSpeed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
            }
            if ((_characterHandleWeapon == null) || (_characterHandleWeapon.CurrentWeapon == null))
            {
                _relativeSpeed = MovementRotatingModel.transform.InverseTransformVector(_newSpeed);
            }
            else
            {
                _relativeSpeed = WeaponRotatingModel.transform.InverseTransformVector(_newSpeed);
            }

            float maxSpeed = 0f;

            if (_characterMovement != null)
            {
                maxSpeed = _characterMovement.WalkSpeed;
            }

            if (_characterRun != null)
            {
                maxSpeed = _characterRun.RunSpeed;
            }

            _remappedSpeed.x = MMMaths.Remap(_relativeSpeed.x, 0f, maxSpeed, 0f, 1f);
            _remappedSpeed.y = MMMaths.Remap(_relativeSpeed.y, 0f, maxSpeed, 0f, 1f);
            _remappedSpeed.z = MMMaths.Remap(_relativeSpeed.z, 0f, maxSpeed, 0f, 1f);
            _relativeSpeedNormalized = _relativeSpeed.normalized;
            if (Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y) > 1f)
            {
                _rotationSpeed = Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y);
            }
            else
            {
                _rotationSpeed -= Time.time * RotationSpeedResetSpeed;
            }

            if (_rotationSpeed <= 0f)
            {
                _rotationSpeed = 0f;
            }

            _modelAnglesYLastFrame = ModelAngles.y;
            _positionLastFrame = this.transform.position;
        }
        public virtual void Face(Character.FacingDirections direction)
        {
            switch (direction)
            {
                case Character.FacingDirections.East:
                    _newMovementQuaternion = Quaternion.LookRotation(Vector3.right);
                    break;
                case Character.FacingDirections.North:
                    _newMovementQuaternion = Quaternion.LookRotation(Vector3.forward);
                    break;
                case Character.FacingDirections.South:
                    _newMovementQuaternion = Quaternion.LookRotation(Vector3.back);
                    break;
                case Character.FacingDirections.West:
                    _newMovementQuaternion = Quaternion.LookRotation(Vector3.left);
                    break;
            }
        }
        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_rotationSpeeddAnimationParameterName, AnimatorControllerParameterType.Float, out _rotationSpeeddAnimationParameter);

            RegisterAnimatorParameter(_relativeForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out _relativeForwardSpeedAnimationParameter);

            RegisterAnimatorParameter(_relativeLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out _relativeLateralSpeedAnimationParameter);

            RegisterAnimatorParameter(_remappedForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out _remappedForwardSpeedAnimationParameter);

            RegisterAnimatorParameter(_remappedLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float,
                out _remappedLateralSpeedAnimationParameter);

            RegisterAnimatorParameter(_relativeForwardSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float,
                out _relativeForwardSpeedNormalizedAnimationParameter);

            RegisterAnimatorParameter(_relativeLateralSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float,
                out _relativeLateralSpeedNormalizedAnimationParameter);

            RegisterAnimatorParameter(_remappedSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float,
                out _remappedSpeedNormalizedAnimationParameter);
        }
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _rotationSpeeddAnimationParameter, _rotationSpeed, _character._animatorParameters,
                _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedAnimationParameter, _relativeSpeed.z, _character._animatorParameters,
                _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedAnimationParameter, _relativeSpeed.x, _character._animatorParameters,
                _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedForwardSpeedAnimationParameter, _remappedSpeed.z, _character._animatorParameters,
                _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedLateralSpeedAnimationParameter, _remappedSpeed.x, _character._animatorParameters,
                _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.z,
                _character._animatorParameters, _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.x,
                _character._animatorParameters, _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedSpeedNormalizedAnimationParameter, _remappedSpeed.magnitude,
                _character._animatorParameters, _character.RunAnimatorSanityChecks);
        }
    }
}