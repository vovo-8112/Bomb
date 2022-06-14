using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMoveRandomly3D")]
    public class AIActionMoveRandomly3D : AIAction
    {
        [Header("Duration")]
        [Tooltip("the maximum time a character can spend going in a direction without changing")]
        public float MaximumDurationInADirection = 2f;
        [Header("Obstacles")]
        [Tooltip("the layers the character will try to avoid")]
        public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("the minimum distance from the target this Character can reach.")]
        public float ObstaclesDetectionDistance = 1f;
        [Tooltip("the frequency (in seconds) at which to check for obstacles")]
        public float ObstaclesCheckFrequency = 1f;
        [Tooltip("the minimal random direction to randomize from")]
        public Vector2 MinimumRandomDirection = new Vector2(-1f, -1f);
        [Tooltip("the maximum random direction to randomize from")]
        public Vector2 MaximumRandomDirection = new Vector2(1f, 1f);

        protected CharacterMovement _characterMovement;
        protected Vector2 _direction;
        protected Collider _collider;
        protected float _lastObstacleDetectionTimestamp = 0f;
        protected float _lastDirectionChangeTimestamp = 0f;
        protected Vector3 _rayDirection;
        protected override void Initialization()
        {
            _characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
            _collider = this.gameObject.GetComponentInParent<Collider>();
            PickRandomDirection();
        }
        public override void PerformAction()
        {
            CheckForObstacles();
            CheckForDuration();
            Move();
        }
        protected virtual void Move()
        {
            _characterMovement.SetMovement(_direction);
        }
        protected virtual void CheckForObstacles()
        {
            if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
            {
                return;
            }

            _rayDirection = Vector3.zero;
            _rayDirection.x = _direction.x;
            _rayDirection.z = _direction.y;
            bool hit = Physics.BoxCast(_collider.bounds.center, _collider.bounds.extents, _rayDirection.normalized, this.transform.rotation, ObstaclesDetectionDistance, ObstacleLayerMask);
            if (hit)
            {
                PickRandomDirection();
            }

            _lastObstacleDetectionTimestamp = Time.time;
        }
        protected virtual void CheckForDuration()
        {
            if (Time.time - _lastDirectionChangeTimestamp > MaximumDurationInADirection)
            {
                PickRandomDirection();
            }
        }
        protected virtual void PickRandomDirection()
        {
            _direction.x = UnityEngine.Random.Range(MinimumRandomDirection.x, MaximumRandomDirection.x);
            _direction.y = UnityEngine.Random.Range(MinimumRandomDirection.y, MaximumRandomDirection.y);
            _lastDirectionChangeTimestamp = Time.time;
        }
        public override void OnExitState()
        {
            base.OnExitState();

            _characterMovement?.SetHorizontalMovement(0f);
            _characterMovement?.SetVerticalMovement(0f);
        }
    }
}
