using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("TopDown Engine/Character/Core/TopDown Controller 3D")]
    public class TopDownController3D : TopDownController
    {
        [MMReadOnly]
        [Tooltip("the current input sent to this character")]
        public Vector3 InputMoveDirection = Vector3.zero;
        public enum UpdateModes { Update, FixedUpdate }
        public enum VelocityTransferOnJump { NoTransfer, InitialVelocity, FloorVelocity, Relative }

        [Header("Settings")]
        [Tooltip("whether the movement computation should occur at Update or FixedUpdate. FixedUpdate is the recommended choice.")]
        public UpdateModes UpdateMode = UpdateModes.FixedUpdate;
        [Tooltip("how the velocity should be affected when jumping from a moving ground")]
        public VelocityTransferOnJump VelocityTransferMethod = VelocityTransferOnJump.FloorVelocity;
        
        [Header("Raycasts")]
        [Tooltip("the layer to consider as obstacles (will prevent movement)")]
        public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("the length of the raycasts to cast downwards")]
        public float GroundedRaycastLength = 5f;
        [Tooltip("the distance to the ground beyond which the character isn't considered grounded anymore")]
        public float MinimumGroundedDistance = 0.2f;

        [Header("Physics interaction")]
        [Tooltip("the speed at which external forces get lerped to zero")]
        public float ImpactFalloff = 5f;
        [Tooltip("the force to apply when colliding with rigidbodies")]
        public float PushPower = 2f;
        [Tooltip("a threshold against which to check when going over steps. Adjust that value if your character has issues going over small steps")] 
        public float GroundNormalHeightThreshold = 0.1f;
        
        [Header("Movement")]
        [Tooltip("the maximum vertical velocity the character can have while falling")]
        public float MaximumFallSpeed = 20.0f;
        [Tooltip("the factor by which to multiply the speed while walking on a slope. x is the angle, y is the factor")]
        public AnimationCurve SlopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));

        [Header("Steep Surfaces")]
        [Tooltip("the current surface normal vector")]
        [MMReadOnly]
        public Vector3 GroundNormal = Vector3.zero;
        [Tooltip("whether or not the character should slide while standing on steep surfaces")]
        public bool SlideOnSteepSurfaces = true;
        [Tooltip("the speed at which the character should slide")]
        public float SlidingSpeed = 15f;
        [Tooltip("the control the player has on the speed while sliding down")]
        public float SlidingSpeedControl = 0.4f;
        [Tooltip("the control the player has on the direction while sliding down")]
        public float SlidingDirectionControl = 1f;
        public override Vector3 ColliderCenter { get { return this.transform.position + _characterController.center; } }
        public override Vector3 ColliderBottom { get { return this.transform.position + _characterController.center + Vector3.down * _characterController.bounds.extents.y; } }
        public override Vector3 ColliderTop { get { return this.transform.position + _characterController.center + Vector3.up * _characterController.bounds.extents.y; } }
        public bool IsSliding() { return (Grounded && SlideOnSteepSurfaces && TooSteep()); }
        public bool CollidingAbove() { return (_collisionFlags & CollisionFlags.CollidedAbove) != 0; }
        public bool TooSteep() { return (GroundNormal.y <= Mathf.Cos(_characterController.slopeLimit * Mathf.Deg2Rad)); }
        public bool ExitedTooSteepSlopeThisFrame { get; set; }
        public override bool OnAMovingPlatform { get { return ShouldMoveWithPlatformThisFrame(); } }
        public override Vector3 MovingPlatformSpeed
        {
            get { return _movingPlatformVelocity;  }
        }
        
        protected Transform _transform;
        protected Rigidbody _rigidBody;
        protected Collider _collider;
        protected CharacterController _characterController;
        protected float _originalColliderHeight;
        protected Vector3 _originalColliderCenter;
        protected Vector3 _originalSizeRaycastOrigin;
        protected Rigidbody _pushedRigidbody;
        protected Vector3 _pushDirection;
        protected Vector3 _lastGroundNormal = Vector3.zero;
        protected WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
        protected Transform _movingPlatformHitCollider;
        protected Transform _movingPlatformCurrentHitCollider;
        protected Vector3 _movingPlatformCurrentHitColliderLocal;
        protected Vector3 _movingPlatformCurrentGlobalPoint;
        protected Quaternion _movingPlatformLocalRotation;
        protected Quaternion _movingPlatformGlobalRotation;
        protected Matrix4x4 _lastMovingPlatformMatrix;
        protected Vector3 _movingPlatformVelocity;
        protected bool _newMovingPlatform;
        protected CollisionFlags _collisionFlags;
        protected Vector3 _frameVelocity = Vector3.zero;
        protected Vector3 _hitPoint = Vector3.zero;
        protected Vector3 _lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
        protected Vector3 _newVelocity;
        protected Vector3 _lastHorizontalVelocity;
        protected Vector3 _newHorizontalVelocity;
        protected Vector3 _motion;
        protected Vector3 _idealVelocity;
        protected Vector3 _idealDirection;
        protected Vector3 _horizontalVelocityDelta;
        protected float _stickyOffset = 0f;
        protected RaycastHit _movePositionHit;
        protected Vector3 _capsulePoint1;
        protected Vector3 _capsulePoint2;
        protected Vector3 _movePositionDirection;
        protected float _movePositionDistance;
        protected RaycastHit _cardinalRaycast;
        
        protected float _smallestDistance = Single.MaxValue;
        protected float _longestDistance = Single.MinValue;

        protected RaycastHit _smallestRaycast;
        protected RaycastHit _emptyRaycast = new RaycastHit();
        protected Vector3 _downRaycastsOffset;

        protected Vector3 _moveWithPlatformMoveDistance;
        protected Vector3 _moveWithPlatformGlobalPoint;
        protected Quaternion _moveWithPlatformGlobalRotation;
        protected Quaternion _moveWithPlatformRotationDiff;
        protected RaycastHit _raycastDownHit;
        protected Vector3 _raycastDownDirection = Vector3.down;
        protected RaycastHit _canGoBackHeadCheck;
        protected bool _tooSteepLastFrame;
        protected override void Awake()
        {
            base.Awake();

            _characterController = this.gameObject.GetComponent<CharacterController>();
            _transform = this.transform;
            _rigidBody = this.gameObject.GetComponent<Rigidbody>();
            _collider = this.gameObject.GetComponent<Collider>();
            _originalColliderHeight = _characterController.height;
            _originalColliderCenter = _characterController.center;
        }

        #region Update
        protected override void LateUpdate()
        {
            base.LateUpdate();
            VelocityLastFrame = Velocity;
        }
        protected override void Update()
        {
            base.Update();
            if (UpdateMode == UpdateModes.Update)
            {
                ProcessUpdate();
            }
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            ApplyImpact();
            GetMovingPlatformVelocity();
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                ProcessUpdate();
            }
        }
        protected virtual void ProcessUpdate()
        {
            if (_transform == null)
            {
                return;
            }

            if (!FreeMovement)
            {
                return;
            }

            _newVelocity = Velocity;

            _positionLastFrame = _transform.position;

            AddInput();
            AddGravity();
            MoveWithPlatform();
            ComputeVelocityDelta();
            MoveCharacterController();
            DetectNewMovingPlatform();
            ComputeNewVelocity();
            ManualControllerColliderHit();
            HandleGroundContact();
        }
        protected virtual void AddInput()
        {
            if (Grounded && TooSteep())
            {
                _idealVelocity.x = GroundNormal.x;
                _idealVelocity.y = 0;
                _idealVelocity.z = GroundNormal.z;
                _idealVelocity = _idealVelocity.normalized;
                _idealDirection = Vector3.Project(InputMoveDirection, _idealVelocity);
                _idealVelocity = _idealVelocity + (_idealDirection * SlidingSpeedControl) + (InputMoveDirection - _idealDirection) * SlidingDirectionControl;
                _idealVelocity = _idealVelocity * SlidingSpeed;
            }
            else
            {
                _idealVelocity = CurrentMovement;
            }

            if (VelocityTransferMethod == VelocityTransferOnJump.FloorVelocity)
            {
                _idealVelocity += _frameVelocity;
                _idealVelocity.y = 0;
            }

            if (Grounded)
            {
                Vector3 sideways = Vector3.Cross(Vector3.up, _idealVelocity);
                _idealVelocity = Vector3.Cross(sideways, GroundNormal).normalized * _idealVelocity.magnitude;
            }

            _newVelocity = _idealVelocity;
            _newVelocity.y = Grounded ? Mathf.Min(_newVelocity.y, 0) : _newVelocity.y;
        }
        protected virtual void AddGravity()
        {
            if (GravityActive)
            {
                if (Grounded)
                {
                    _newVelocity.y = Mathf.Min(0, _newVelocity.y) - Gravity * Time.deltaTime;
                }
                else
                {
                    _newVelocity.y = Velocity.y - Gravity * Time.deltaTime;
                    _newVelocity.y = Mathf.Max(_newVelocity.y, -MaximumFallSpeed);
                }
            }
            _newVelocity += AddedForce;
            AddedForce = Vector3.zero;
        }
        protected virtual void MoveWithPlatform()
        {
            if (ShouldMoveWithPlatformThisFrame())
            {
                _moveWithPlatformMoveDistance.x = _moveWithPlatformMoveDistance.y = _moveWithPlatformMoveDistance.z = 0f;
                _moveWithPlatformGlobalPoint = _movingPlatformCurrentHitCollider.TransformPoint(_movingPlatformCurrentHitColliderLocal);
                _moveWithPlatformMoveDistance = (_moveWithPlatformGlobalPoint - _movingPlatformCurrentGlobalPoint);
                if (_moveWithPlatformMoveDistance != Vector3.zero)
                {
                    _characterController.Move(_moveWithPlatformMoveDistance);
                }
                _moveWithPlatformGlobalRotation = _movingPlatformCurrentHitCollider.rotation * _movingPlatformLocalRotation;
                _moveWithPlatformRotationDiff = _moveWithPlatformGlobalRotation * Quaternion.Inverse(_movingPlatformGlobalRotation);
                float yRotation = _moveWithPlatformRotationDiff.eulerAngles.y;
                if (yRotation != 0)
                {
                    _transform.Rotate(0, yRotation, 0);
                }
            }
        }
        protected virtual void ComputeVelocityDelta()
        {
            _motion = _newVelocity * Time.deltaTime;
            _horizontalVelocityDelta.x = _motion.x;
            _horizontalVelocityDelta.y = 0f;
            _horizontalVelocityDelta.z = _motion.z;
            _stickyOffset = Mathf.Max(_characterController.stepOffset, _horizontalVelocityDelta.magnitude);
            if (Grounded)
            {
                _motion -= _stickyOffset * Vector3.up;
            }
        }
        protected virtual void MoveCharacterController()
        {
            GroundNormal.x = GroundNormal.y = GroundNormal.z = 0f;

            _collisionFlags = _characterController.Move(_motion);

            _lastHitPoint = _hitPoint;
            _lastGroundNormal = GroundNormal;
        }
        protected virtual void DetectNewMovingPlatform()
        {
            if (_movingPlatformCurrentHitCollider != _movingPlatformHitCollider)
            {
                if (_movingPlatformHitCollider != null)
                {
                    _movingPlatformCurrentHitCollider = _movingPlatformHitCollider;
                    _lastMovingPlatformMatrix = _movingPlatformHitCollider.localToWorldMatrix;
                    _newMovingPlatform = true;
                }
            }
        }
        protected virtual void ComputeNewVelocity()
        {
            _lastHorizontalVelocity.x = _newVelocity.x;
            _lastHorizontalVelocity.y = 0;
            _lastHorizontalVelocity.z = _newVelocity.z;

            if (Time.deltaTime != 0f)
            {
                Velocity = (_transform.position - _positionLastFrame) / Time.deltaTime;
            }

            _newHorizontalVelocity.x = Velocity.x;
            _newHorizontalVelocity.y = 0;
            _newHorizontalVelocity.z = Velocity.z;

            if (_lastHorizontalVelocity == Vector3.zero)
            {
                Velocity.x = 0f;
                Velocity.z = 0f;
            }
            else
            {
                float newVelocity = Vector3.Dot(_newHorizontalVelocity, _lastHorizontalVelocity) / _lastHorizontalVelocity.sqrMagnitude;
                Velocity = _lastHorizontalVelocity * Mathf.Clamp01(newVelocity) + Velocity.y * Vector3.up;
            }
            if (Velocity.y < _newVelocity.y - 0.001)
            {
                if (Velocity.y < 0)
                {
                    Velocity.y = _newVelocity.y;
                }
            }

            Acceleration = (Velocity - VelocityLastFrame) / Time.deltaTime;
        }
        protected virtual void HandleGroundContact()
        {
            Grounded = _characterController.isGrounded;
            
            if (Grounded && !IsGroundedTest())
            {
                Grounded = false;
                if ((VelocityTransferMethod == VelocityTransferOnJump.InitialVelocity ||
                     VelocityTransferMethod == VelocityTransferOnJump.FloorVelocity))
                {
                    _frameVelocity = _movingPlatformVelocity;
                    Velocity += _movingPlatformVelocity;
                }
            }
            else if (!Grounded && IsGroundedTest())
            {
                Grounded = true;
                SubstractNewPlatformVelocity();
            }

            if (ShouldMoveWithPlatformThisFrame())
            {
                _movingPlatformCurrentHitColliderLocal = _movingPlatformCurrentHitCollider.InverseTransformPoint(_movingPlatformCurrentGlobalPoint);
                _movingPlatformGlobalRotation = _transform.rotation;
                _movingPlatformLocalRotation = Quaternion.Inverse(_movingPlatformCurrentHitCollider.rotation) * _movingPlatformGlobalRotation;
            }

            ExitedTooSteepSlopeThisFrame = (_tooSteepLastFrame && !TooSteep());
            
            _tooSteepLastFrame = TooSteep();
        }
        protected override void DetermineDirection()
        {
            if (CurrentMovement.magnitude > 0f)
            {
                CurrentDirection = CurrentMovement.normalized;
            }
        }
        protected override void HandleFriction()
        {
        }

        #endregion

        #region Rigidbody push mechanics
        
        protected virtual void ManualControllerColliderHit()
        {
            _smallestDistance = Single.MaxValue;
            _longestDistance = Single.MinValue;
            _smallestRaycast = _emptyRaycast;
            float offset = _characterController.radius;
            
            _downRaycastsOffset.x = 0f;
            _downRaycastsOffset.y = 0f;
            _downRaycastsOffset.z = 0f;
            CastRayDownwards();
            _downRaycastsOffset.x = -offset;
            _downRaycastsOffset.y = offset;
            _downRaycastsOffset.z = 0f;
            CastRayDownwards();
            _downRaycastsOffset.x = 0f;
            _downRaycastsOffset.y = offset;
            _downRaycastsOffset.z = -offset;
            CastRayDownwards();
            _downRaycastsOffset.x = offset;
            _downRaycastsOffset.y = offset;
            _downRaycastsOffset.z = 0f;
            CastRayDownwards();
            _downRaycastsOffset.x = 0f;
            _downRaycastsOffset.y = offset;
            _downRaycastsOffset.z = offset;
            CastRayDownwards();
            if (_smallestRaycast.collider != null)
            {
                float adjustedDistance = AdjustDistance(_smallestRaycast.distance);
                
                if (_smallestRaycast.normal.y > 0 && _smallestRaycast.normal.y > GroundNormal.y )
                {
                    if (
                        (_smallestRaycast.point.y - _lastHitPoint.y < GroundNormalHeightThreshold) 
                        && ((_smallestRaycast.point != _lastHitPoint)
                            || (_lastGroundNormal == Vector3.zero)))
                    {
                        GroundNormal = _smallestRaycast.normal;
                    }
                    else
                    {
                        GroundNormal = _lastGroundNormal;
                    }
                    _movingPlatformHitCollider = _smallestRaycast.collider.transform;
                    _hitPoint = _smallestRaycast.point;
                    _frameVelocity.x = _frameVelocity.y = _frameVelocity.z = 0f;
                }    
            }
            Physics.Raycast(this._transform.position + _characterController.center, CurrentMovement.normalized, out _raycastDownHit, 
                _characterController.radius + _characterController.skinWidth, ObstaclesLayerMask);
            
            if (_raycastDownHit.collider != null)
            {
                HandlePush(_raycastDownHit, _raycastDownHit.point);
            }
        }
        protected virtual void CastRayDownwards()
        {
            if (_smallestDistance <= MinimumGroundedDistance)
            {
                return;
            }
            
            Physics.Raycast(this._transform.position + _characterController.center + _downRaycastsOffset, _raycastDownDirection, out _raycastDownHit, 
                _characterController.height/2f + GroundedRaycastLength, ObstaclesLayerMask);
            
            if (_raycastDownHit.collider != null)
            {
                float adjustedDistance = AdjustDistance(_raycastDownHit.distance);
                
                if (adjustedDistance < _smallestDistance) { _smallestDistance = adjustedDistance; _smallestRaycast = _raycastDownHit; }
                if (adjustedDistance > _longestDistance) { _longestDistance = adjustedDistance; }
            }
        }
        protected float AdjustDistance(float distance)
        {
            float adjustedDistance = distance - _characterController.height / 2f -
                                     _characterController.skinWidth;
            return adjustedDistance;
        }

        protected Vector3 _onTriggerEnterPushbackDirection;
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.MMGetComponentNoAlloc<MovingPlatform3D>() != null)
            {
                if (this.transform.position.y < other.transform.position.y)
                {
                    _onTriggerEnterPushbackDirection = (this.transform.position - other.transform.position).normalized;
                    this.Impact(_onTriggerEnterPushbackDirection.normalized, other.gameObject.MMGetComponentNoAlloc<MovingPlatform3D>().PushForce);
                }
            }
        }
        protected virtual void HandlePush(RaycastHit hit, Vector3 hitPosition)
        {
            _pushedRigidbody = hit.collider.attachedRigidbody;

            if ((_pushedRigidbody == null) || (_pushedRigidbody.isKinematic))
            {
                return;
            }
            
            _pushDirection.x = CurrentMovement.normalized.x;
            _pushDirection.y = 0;
            _pushDirection.z = CurrentMovement.normalized.z;

            _pushedRigidbody.AddForceAtPosition(_pushDirection * PushPower, hitPosition);
        }

        #endregion

        #region Moving Platforms
        protected virtual void GetMovingPlatformVelocity()
        {
            if (_movingPlatformCurrentHitCollider != null)
            {
                if (!_newMovingPlatform && (Time.deltaTime != 0f))
                {
                    _movingPlatformVelocity = (
                        _movingPlatformCurrentHitCollider.localToWorldMatrix.MultiplyPoint3x4(_movingPlatformCurrentHitColliderLocal)
                        - _lastMovingPlatformMatrix.MultiplyPoint3x4(_movingPlatformCurrentHitColliderLocal)
                    ) / Time.deltaTime;
                }
                _lastMovingPlatformMatrix = _movingPlatformCurrentHitCollider.localToWorldMatrix;
                _newMovingPlatform = false;
            }
            else
            {
                _movingPlatformVelocity = Vector3.zero;
            }
        }
        protected virtual IEnumerator SubstractNewPlatformVelocity()
        {
            if ((VelocityTransferMethod == VelocityTransferOnJump.InitialVelocity ||
                 VelocityTransferMethod == VelocityTransferOnJump.FloorVelocity))
            {
                if (_newMovingPlatform)
                {
                    Transform platform = _movingPlatformCurrentHitCollider;
                    yield return _waitForFixedUpdate;
                    if (Grounded && platform == _movingPlatformCurrentHitCollider)
                    {
                        yield return 1;
                    }
                }
                Velocity -= _movingPlatformVelocity;
            }
        }
        protected virtual bool ShouldMoveWithPlatformThisFrame()
        {
            return (
                (Grounded || VelocityTransferMethod == VelocityTransferOnJump.Relative)
                && _movingPlatformCurrentHitCollider != null
            );
        }

        #endregion

        #region Collider Resizing
        public override bool CanGoBackToOriginalSize()
        {
            if (_collider.bounds.size.y == _originalColliderHeight)
            {
                return true;
            }
            float headCheckDistance = _originalColliderHeight * transform.localScale.y * CrouchedRaycastLengthMultiplier;
            _originalSizeRaycastOrigin = ColliderTop + transform.up * _smallValue;

            _canGoBackHeadCheck = MMDebug.Raycast3D(_originalSizeRaycastOrigin, transform.up, headCheckDistance, ObstaclesLayerMask, Color.cyan, true);
            if (_canGoBackHeadCheck.collider != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public override void ResizeColliderHeight(float newHeight)
        {
            float newYOffset = _originalColliderCenter.y - (_originalColliderHeight - newHeight) / 2;
            _characterController.height = newHeight;
            _characterController.center = ((_originalColliderHeight - newHeight) / 2) * Vector3.up;
            this.transform.Translate((newYOffset / 2f) * Vector3.up);
        }
        public override void ResetColliderSize()
        {
            _characterController.height = _originalColliderHeight;
            _characterController.center = _originalColliderCenter;
        }

        #endregion

        #region Grounded Tests
        public virtual bool IsGroundedTest()
        {
            
            bool grounded = false;

            if (_smallestDistance <= MinimumGroundedDistance)
            {
                grounded = (GroundNormal.y > 0.01) ;
            }

            return grounded;
        }
        protected override void CheckIfGrounded()
        {
            JustGotGrounded = (!_groundedLastFrame && Grounded);
            _groundedLastFrame = Grounded;
        }

        #endregion

        #region Public Methods
        public override void CollisionsOn()
        {
            _collider.enabled = true;
        }
        public override void CollisionsOff()
        {
            _collider.enabled = false;
        }
        public override void DetectObstacles(float distance, Vector3 offset)
        {
            if (!PerformCardinalObstacleRaycastDetection)
            {
                return;
            }
            
            CollidingWithCardinalObstacle = false;
            _cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.right, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_cardinalRaycast.collider != null) { DetectedObstacleRight = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleRight = null; }
            _cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.left, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_cardinalRaycast.collider != null) { DetectedObstacleLeft = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleLeft = null; }
            _cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.forward, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_cardinalRaycast.collider != null) { DetectedObstacleUp = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleUp = null; }
            _cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.back, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_cardinalRaycast.collider != null) { DetectedObstacleDown = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleDown = null; }
        }
        protected virtual void ApplyImpact()
        {
            if (_impact.magnitude > 0.2f)
            {
                _characterController.Move(_impact * Time.deltaTime);
            }
            _impact = Vector3.Lerp(_impact, Vector3.zero, ImpactFalloff * Time.deltaTime);
        }
        public override void AddForce(Vector3 movement)
        {
            AddedForce += movement;
        }
        public override void Impact(Vector3 direction, float force)
        {
            direction = direction.normalized;
            if (direction.y < 0) { direction.y = -direction.y; }
            _impact += direction.normalized * force;
        }
        public override void SetMovement(Vector3 movement)
        {
            CurrentMovement = movement;

            Vector3 directionVector;
            directionVector = movement;
            if (directionVector != Vector3.zero)
            {
                float directionLength = directionVector.magnitude;
                directionVector = directionVector / directionLength;
                directionLength = Mathf.Min(1, directionLength);
                directionLength = directionLength * directionLength;
                directionVector = directionVector * directionLength;
            }
            InputMoveDirection = transform.rotation * directionVector;
        }
        public override void SetKinematic(bool state)
        {
            _rigidBody.isKinematic = state;
        }
        public override void MovePosition(Vector3 newPosition)
        {
            
            _movePositionDirection = (newPosition - this.transform.position);
            _movePositionDistance = Vector3.Distance(this.transform.position, newPosition) ;

            _capsulePoint1 =    this.transform.position 
                                + _characterController.center 
                                - (Vector3.up * _characterController.height / 2f) 
                                + Vector3.up * _characterController.skinWidth 
                                + Vector3.up * _characterController.radius;
            _capsulePoint2 =    this.transform.position
                                + _characterController.center
                                + (Vector3.up * _characterController.height / 2f)
                                - Vector3.up * _characterController.skinWidth
                                - Vector3.up * _characterController.radius;

            if (!Physics.CapsuleCast(_capsulePoint1, _capsulePoint2, _characterController.radius, _movePositionDirection, out _movePositionHit, _movePositionDistance, ObstaclesLayerMask))
            {
                this.transform.position = newPosition;
            }
        }

        #endregion
        
    }
}