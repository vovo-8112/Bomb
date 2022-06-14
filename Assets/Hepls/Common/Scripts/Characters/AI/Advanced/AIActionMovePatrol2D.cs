using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMovePatrol2D")]
    public class AIActionMovePatrol2D : AIAction
    {        
        [Header("Obstacle Detection")]
        [Tooltip("If set to true, the agent will change direction when hitting an obstacle")]
        public bool ChangeDirectionOnObstacle = true;
        [Tooltip("the layermask to look for obstacles on")]
        public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("the frequency (in seconds) at which to check for obstacles")]
        public float ObstaclesCheckFrequency = 1f;
        public Vector3 LastReachedPatrolPoint { get; set; }
        protected TopDownController _controller;
        protected Character _character;
        protected CharacterOrientation2D _orientation2D;
        protected CharacterMovement _characterMovement;
        protected Health _health;
        protected Vector2 _direction;
        protected Vector2 _startPosition;
        protected Vector3 _initialScale;
        protected MMPath _mmPath;
        protected float _lastObstacleDetectionTimestamp = 0f;
        protected bool _initialized = false;

        protected int _currentIndex = 0;
        protected int _indexLastFrame = -1;
        protected float _waitingDelay = 0f;
        protected override void Awake()
        {
            base.Awake();
            InitializePatrol();
        }
        protected virtual void InitializePatrol()
        {
            _controller = this.gameObject.GetComponentInParent<TopDownController>();
            _character = this.gameObject.GetComponentInParent<Character>();
            _orientation2D = _character?.FindAbility<CharacterOrientation2D>();
            _characterMovement = _character?.FindAbility<CharacterMovement>();
            _health = _character?._health;
            _mmPath = this.gameObject.GetComponentInParent<MMPath>();
            _startPosition = transform.position;
            _direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
            _initialScale = transform.localScale;
            _currentIndex = 0;
            _indexLastFrame = -1;
            _waitingDelay = 0;
            _initialized = true;
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

            _currentIndex = _mmPath.CurrentIndex();
            if (_currentIndex != _indexLastFrame)
            {
                LastReachedPatrolPoint = _mmPath.CurrentPoint();
                _waitingDelay = _mmPath.PathElements[_currentIndex].Delay;
            }

            _direction = _mmPath.CurrentPoint() - this.transform.position;
            _direction = _direction.normalized;

            _characterMovement.SetHorizontalMovement(_direction.x);
            _characterMovement.SetVerticalMovement(_direction.y);

            _indexLastFrame = _currentIndex;
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

            RaycastHit2D raycast = MMDebug.RayCast(_controller.ColliderCenter, _direction, 1f, ObstaclesLayerMask, MMColors.Gold, true);
            if (raycast)
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
            if (!_initialized)
            {
                return;
            }
            
            if (_orientation2D != null)
            {
                _direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
            }
            
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