using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMovePatrol3D")]
    public class AIActionMovePatrol3D : AIAction
    {        
        [Header("Obstacle Detection")]
        [Tooltip("If set to true, the agent will change direction when hitting an obstacle")]
        public bool ChangeDirectionOnObstacle = true;
        [Tooltip("the distance to look for obstacles at")]
        public float ObstacleDetectionDistance = 1f;
        [Tooltip("the frequency (in seconds) at which to check for obstacles")]
        public float ObstaclesCheckFrequency = 1f;
        [Tooltip("the layer(s) to look for obstacles on")]
        public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
        public Vector3 LastReachedPatrolPoint { get; set; }

        [Header("Debug")]
        [Tooltip("the index of the current MMPath element this agent is patrolling towards")]
        [MMReadOnly]
        public int CurrentPathIndex = 0;
        protected TopDownController _controller;
        protected Character _character;
        protected CharacterMovement _characterMovement;
        protected Health _health;
        protected Vector3 _direction;
        protected Vector3 _startPosition;
        protected Vector3 _initialDirection;
        protected Vector3 _initialScale;
        protected float _distanceToTarget;
        protected Vector3 _initialPosition;
        protected MMPath _mmPath;
        protected Collider _collider;
        protected float _lastObstacleDetectionTimestamp = 0f;        
        protected int _indexLastFrame = -1;
        protected float _waitingDelay = 0f;
        protected override void Awake()
        {
            base.Awake();
            InitializePatrol();
        }

        protected virtual void InitializePatrol()
        {
            _collider = this.gameObject.GetComponentInParent<Collider>();
            _controller = this.gameObject.GetComponentInParent<TopDownController>();
            _character = this.gameObject.GetComponentInParent<Character>();
            _characterMovement = _character?.FindAbility<CharacterMovement>();
            _health = _character._health;
            _mmPath = this.gameObject.GetComponentInParent<MMPath>();
            _startPosition = transform.position;
            _initialPosition = this.transform.position;
            _initialDirection = _direction;
            _initialScale = transform.localScale;
            CurrentPathIndex = 0;
            _indexLastFrame = -1;
            LastReachedPatrolPoint = this.transform.position;
        }
        public override void PerformAction()
        {
            Patrol();
        }
        protected virtual void Patrol()
        {
            _waitingDelay -= Time.deltaTime;

            if (_character == null)
            {
                return;
            }

            if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
                || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
            {
                return;
            }

            if (_waitingDelay > 0)
            {
                _characterMovement.SetHorizontalMovement(0f);
                _characterMovement.SetVerticalMovement(0f);
                return;
            }
            CheckForObstacles();

            CurrentPathIndex = _mmPath.CurrentIndex();
            if (CurrentPathIndex != _indexLastFrame)
            {
                LastReachedPatrolPoint = _mmPath.CurrentPoint();
                _waitingDelay = _mmPath.PathElements[CurrentPathIndex].Delay;

                if (_waitingDelay > 0)
                {
                    _characterMovement.SetHorizontalMovement(0f);
                    _characterMovement.SetVerticalMovement(0f);
                    _indexLastFrame = CurrentPathIndex;
                    return;
                }
            }

            _direction = _mmPath.CurrentPoint() - this.transform.position;
            _direction = _direction.normalized;

            _characterMovement.SetHorizontalMovement(_direction.x);
            _characterMovement.SetVerticalMovement(_direction.z);

            _indexLastFrame = CurrentPathIndex;
        }
        protected virtual void OnDrawGizmosSelected()
        {
            if (_mmPath == null)
            {
                return;
            }
            Gizmos.color = MMColors.IndianRed;
            Gizmos.DrawLine(this.transform.position, _mmPath.CurrentPoint());
        }
        public override void OnExitState()
        {
            base.OnExitState();
            _characterMovement?.SetHorizontalMovement(0f);
            _characterMovement?.SetVerticalMovement(0f);
        }
	    protected virtual void CheckForObstacles()
        {
            if (!ChangeDirectionOnObstacle)
            {
                return;
            }

            if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
            {
                return;
            }
                        
            bool hit = Physics.BoxCast(_collider.bounds.center, _collider.bounds.extents, _controller.CurrentDirection.normalized, this.transform.rotation, ObstacleDetectionDistance, ObstacleLayerMask);
            if (hit)
            {
                ChangeDirection();
            }

            _lastObstacleDetectionTimestamp = Time.time;
        }
        public virtual void ChangeDirection()
        {
            _direction = -_direction;
            _mmPath.ChangeDirection();
        }
        protected virtual void OnRevive()
        {            
            InitializePatrol();
        }
        protected virtual void OnEnable()
        {
            if (_health == null)
            {
                _health = this.gameObject.GetComponent<Health>();
            }

            if (_health != null)
            {
                _health.OnRevive += OnRevive;
            }
        }
        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnRevive -= OnRevive;
            }
        }
    }
}