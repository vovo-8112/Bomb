using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Rotation 2D")]
    public class CharacterRotation2D : CharacterAbility
    {
		public enum RotationModes { None, MovementDirection, WeaponDirection, Both }
		public enum RotationSpeeds { Instant, Smooth, SmoothAbsolute }

        [Header("Rotation Mode")]
		[Tooltip("whether the character should face movement direction, weapon direction, or both, or none")]
        public RotationModes RotationMode = RotationModes.None;
        [Tooltip("whether the character is being applied a forced rotation")]
        public bool ForcedRotation = false;
        [MMCondition("ForcedRotation", true)]
        [Tooltip("the forced rotation applied by an external script")]
        public Vector3 ForcedRotationDirection;

        [Header("Movement Direction")]
        [Tooltip("If this is true, we'll rotate our model towards the direction")]
        public bool ShouldRotateToFaceMovementDirection = true;
        [Tooltip("the current rotation mode")]
        public RotationSpeeds MovementRotationSpeed = RotationSpeeds.Instant;
        [Tooltip("the object we want to rotate towards direction. If left empty, we'll use the Character's model")]
        public GameObject MovementRotatingModel;
        [Tooltip("the speed at which to rotate towards direction (smooth and absolute only)")]
        public float RotateToFaceMovementDirectionSpeed = 10f;
        [Tooltip("the threshold after which we start rotating (absolute mode only)")]
        public float AbsoluteThresholdMovement = 0.5f;
        [MMReadOnly]
        [Tooltip("the direction of the model")]
        public Vector3 ModelDirection;
        [MMReadOnly]
        [Tooltip("the direction of the model in angle values")]
        public Vector3 ModelAngles;

        [Header("Weapon Direction")]
        [Tooltip("if this is true, we'll rotate our model towards the weapon's direction")]
        public bool ShouldRotateToFaceWeaponDirection = true;
        [Tooltip("the current rotation mode")]
        public RotationSpeeds WeaponRotationSpeed = RotationSpeeds.Instant;
        [Tooltip("the object we want to rotate towards direction. If left empty, we'll use the Character's model")]
        public GameObject WeaponRotatingModel;
        [Tooltip("the speed at which to rotate towards direction (smooth and absolute only)")]
        public float RotateToFaceWeaponDirectionSpeed = 10f;
        [Tooltip("the threshold after which we start rotating (absolute mode only)")]
        public float AbsoluteThresholdWeapon = 0.5f;

        protected CharacterHandleWeapon _characterHandleWeapon;
        protected Vector3 _lastRegisteredVelocity;
        protected Vector3 _rotationDirection;
        protected Vector3 _lastMovement = Vector3.zero;
        protected Vector3 _lastAim = Vector3.zero;
        protected Vector3 _relativeSpeed;
        protected Vector3 _relativeSpeedNormalized;
        protected bool _secondaryMovementTriggered = false;
        protected Quaternion _tmpRotation;
        protected Quaternion _newMovementQuaternion;
        protected Quaternion _newWeaponQuaternion;
        protected bool _shouldRotateTowardsWeapon;
        protected const string _relativeForwardSpeedAnimationParameterName = "RelativeForwardSpeed";
        protected const string _relativeLateralSpeedAnimationParameterName = "RelativeLateralSpeed";
        protected const string _relativeForwardSpeedNormalizedAnimationParameterName = "RelativeForwardSpeedNormalized";
        protected const string _relativeLateralSpeedNormalizedAnimationParameterName = "RelativeLateralSpeedNormalized";
        protected int _relativeForwardSpeedAnimationParameter;
        protected int _relativeLateralSpeedAnimationParameter;
        protected int _relativeForwardSpeedNormalizedAnimationParameter;
        protected int _relativeLateralSpeedNormalizedAnimationParameter;
        protected override void Initialization()
        {
            base.Initialization();
            if (MovementRotatingModel == null)
            {
                MovementRotatingModel = _model;
            }

            _characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
            if (WeaponRotatingModel == null)
            {
                WeaponRotatingModel = _model;
            }
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            RotateToFaceMovementDirection();
            RotateToFaceWeaponDirection();
            RotateToFaceForcedRotation();
            RotateModel();
        }


        protected virtual void FixedUpdate()
        {
            ComputeRelativeSpeeds();
        }
        protected virtual void RotateToFaceMovementDirection()
        {
            if (!ShouldRotateToFaceMovementDirection) { return; }
            if ((RotationMode != RotationModes.MovementDirection) && (RotationMode != RotationModes.Both)) { return; }

            float angle = Mathf.Atan2(_controller.CurrentDirection.y, _controller.CurrentDirection.x) * Mathf.Rad2Deg;

            if (MovementRotationSpeed == RotationSpeeds.Instant)
            {
                if (_controller.CurrentDirection != Vector3.zero)
                {
                    _newMovementQuaternion = Quaternion.Euler(angle * Vector3.forward);
                }
            }
            if (MovementRotationSpeed == RotationSpeeds.Smooth)
            {
                if (_controller.CurrentDirection != Vector3.zero)
                {
                    _tmpRotation = Quaternion.Euler(angle * Vector3.forward);
                    _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
                }
            }
            if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
            {
                if (_controller.CurrentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
                {
                    _lastMovement = _controller.CurrentDirection;
                }
                if (_lastMovement != Vector3.zero)
                {
                    float lastAngle = Mathf.Atan2(_lastMovement.y, _lastMovement.x) * Mathf.Rad2Deg;
                    _tmpRotation = Quaternion.Euler(lastAngle * Vector3.forward);

                    _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
                }
            }

            ModelDirection = MovementRotatingModel.transform.forward.normalized;
            ModelAngles = MovementRotatingModel.transform.eulerAngles;
        }
		protected virtual void RotateToFaceWeaponDirection()
        {
            _newWeaponQuaternion = Quaternion.identity;
            _shouldRotateTowardsWeapon = false;
            if (!ShouldRotateToFaceWeaponDirection) { return; }
            if ((RotationMode != RotationModes.WeaponDirection) && (RotationMode != RotationModes.Both)) { return; }
            if (_characterHandleWeapon == null) { return; }
            if (_characterHandleWeapon.WeaponAimComponent == null) { return; }

            _shouldRotateTowardsWeapon = true;
            _rotationDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAim.normalized;
            MMDebug.DebugDrawArrow(this.transform.position, _rotationDirection, Color.red);
            
            float angle = Mathf.Atan2(_rotationDirection.y, _rotationDirection.x) * Mathf.Rad2Deg;
            if (WeaponRotationSpeed == RotationSpeeds.Instant)
            {
                if (_rotationDirection != Vector3.zero)
                {
                    _newWeaponQuaternion = Quaternion.Euler(angle * Vector3.forward);
                }
            }
            if (WeaponRotationSpeed == RotationSpeeds.Smooth)
            {
                if (_rotationDirection != Vector3.zero)
                {
                    _tmpRotation = Quaternion.Euler(angle * Vector3.forward);
                    _newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
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
                    float lastAngle = Mathf.Atan2(_lastMovement.y, _lastMovement.x) * Mathf.Rad2Deg;
                    _tmpRotation = Quaternion.Euler(lastAngle * Vector3.forward);
                    _newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
                }
            }
        }
        protected virtual void RotateToFaceForcedRotation()
        {
            if (ForcedRotation)
            {
                float angle = Mathf.Atan2(ForcedRotationDirection.y, ForcedRotationDirection.x) * Mathf.Rad2Deg;

                if (MovementRotationSpeed == RotationSpeeds.Instant)
                {
                    if (ForcedRotationDirection != Vector3.zero)
                    {
                        _newMovementQuaternion = Quaternion.Euler(angle * Vector3.forward);
                    }
                }
                if (MovementRotationSpeed == RotationSpeeds.Smooth)
                {
                    if (ForcedRotationDirection != Vector3.zero)
                    {
                        _tmpRotation = Quaternion.Euler(angle * Vector3.forward);
                        _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
                    }
                }
                if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
                {
                    if (ForcedRotationDirection.normalized.magnitude >= AbsoluteThresholdMovement)
                    {
                        _lastMovement = ForcedRotationDirection;
                    }
                    if (_lastMovement != Vector3.zero)
                    {
                        float lastAngle = Mathf.Atan2(_lastMovement.y, _lastMovement.x) * Mathf.Rad2Deg;
                        _tmpRotation = Quaternion.Euler(lastAngle * Vector3.forward);

                        _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
                    }
                }

                ModelDirection = MovementRotatingModel.transform.forward.normalized;
                ModelAngles = MovementRotatingModel.transform.eulerAngles;
            }
        }
        protected virtual void RotateModel()
        {
            MovementRotatingModel.transform.rotation = _newMovementQuaternion;

            if (_shouldRotateTowardsWeapon)
            {
                WeaponRotatingModel.transform.rotation = _newWeaponQuaternion;
            }
        }

        protected Vector3 _positionLastFrame;
        protected Vector3 _newSpeed;
        protected virtual void ComputeRelativeSpeeds()
        {
            if (Time.deltaTime != 0f)
            {
                _newSpeed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
            }            

            if (_characterHandleWeapon == null)
            {
                _relativeSpeed = MovementRotatingModel.transform.InverseTransformVector(_newSpeed);
            }
            else
            {
                _relativeSpeed = WeaponRotatingModel.transform.InverseTransformVector(_newSpeed);
            }
            _relativeSpeedNormalized = _relativeSpeed.normalized;
            _positionLastFrame = this.transform.position;
        }
        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_relativeForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedAnimationParameter);
            RegisterAnimatorParameter(_relativeLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedAnimationParameter);
            RegisterAnimatorParameter(_relativeForwardSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedNormalizedAnimationParameter);
            RegisterAnimatorParameter(_relativeLateralSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedNormalizedAnimationParameter);
        }
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedAnimationParameter, _relativeSpeed.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedAnimationParameter, _relativeSpeed.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
        }
    }
}
