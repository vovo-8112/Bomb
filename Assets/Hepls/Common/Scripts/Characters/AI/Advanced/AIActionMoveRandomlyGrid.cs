using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMoveRandomlyGrid")]
    public class AIActionMoveRandomlyGrid : AIAction
    {
        public enum Modes { TwoD, ThreeD }

        [Header("Dimension")]
        public Modes Mode = Modes.ThreeD;

        [Header("Duration")]
        [Tooltip("the maximum time a character can spend going in a direction without changing")]
        public float MaximumDurationInADirection = 3f;

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
        [Tooltip("if this is true, the AI will avoid 180° turns if possible")]
        public bool Avoid180 = true;

        protected CharacterGridMovement _characterGridMovement;
        protected TopDownController _topDownController;
        protected Vector2 _direction;
        protected Collider _collider;
        protected Collider2D _collider2D;
        protected float _lastObstacleDetectionTimestamp = 0f;
        protected float _lastDirectionChangeTimestamp = 0f;
        protected Vector3 _rayDirection;
        protected Vector2 _temp2DVector;
        protected Vector3 _temp3DVector;

        protected Vector2[] _raycastDirections2D;
        protected Vector3[] _raycastDirections3D;
        protected RaycastHit _hit;
        protected RaycastHit2D _hit2D;
        protected override void Initialization()
        {
            _characterGridMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterGridMovement>();
            _topDownController = this.gameObject.GetComponentInParent<TopDownController>();
            _collider = this.gameObject.GetComponentInParent<Collider>();
            _collider2D = this.gameObject.GetComponentInParent<Collider2D>();

            _raycastDirections2D = new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
            _raycastDirections3D = new[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            PickNewDirection();
        }
        public override void PerformAction()
        {
            CheckForObstacles();
            CheckForDuration();
            Move();
        }
        protected virtual void Move()
        {
            _characterGridMovement.SetMovement(_direction);
        }
        protected virtual void CheckForObstacles()
        {
            if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
            {
                return;
            }

            _lastObstacleDetectionTimestamp = Time.time;

            if (Mode == Modes.ThreeD)
            {
                _temp3DVector = _direction;
                _temp3DVector.z = _direction.y;
                _temp3DVector.y = 0;
                _hit = MMDebug.Raycast3D(_collider.bounds.center, _temp3DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
                if (_topDownController.CollidingWithCardinalObstacle)
                {
                    PickNewDirection();
                }
            }
            else
            {
                _temp2DVector = _direction;
                _hit2D = MMDebug.RayCast(_collider2D.bounds.center, _temp2DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
                if (_topDownController.CollidingWithCardinalObstacle)
                {
                    PickNewDirection();
                }

            }
        }
        protected virtual void PickNewDirection()
        {
            int retries = 0;
            switch (Mode)
            {

                case Modes.ThreeD:  
                    while (retries < 10)
                    {
                        retries++;
                        int random = MMMaths.RollADice(4) - 1;
                        _temp3DVector = _raycastDirections3D[random];
                        
                        if (Avoid180)
                        {
                            if ((_temp3DVector.x == -_direction.x) && (Mathf.Abs(_temp3DVector.x) > 0))
                            {
                                continue;
                            }
                            if ((_temp3DVector.y == -_direction.y) && (Mathf.Abs(_temp3DVector.y) > 0))
                            {
                                continue;
                            }
                        }

                        _hit = MMDebug.Raycast3D(_collider.bounds.center, _temp3DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
                        if (_hit.collider == null)
                        {
                            _direction = _temp3DVector;
                            _direction.y = _temp3DVector.z;

                            return;
                        }
                    }
                    break;

                case Modes.TwoD:
                    while (retries < 10)
                    {
                        retries++;
                        int random = MMMaths.RollADice(4) - 1;
                        _temp2DVector = _raycastDirections2D[random];

                        if (Avoid180)
                        {
                            if ((_temp2DVector.x == -_direction.x) && (Mathf.Abs(_temp2DVector.x) > 0))
                            {
                                continue;
                            }
                            if ((_temp2DVector.y == -_direction.y) && (Mathf.Abs(_temp2DVector.y) > 0))
                            {
                                continue;
                            }
                        }

                        _hit2D = MMDebug.RayCast(_collider2D.bounds.center, _temp2DVector, ObstaclesDetectionDistance, ObstacleLayerMask, Color.gray);
                        if (_hit2D.collider == null)
                        {
                            _direction = _temp2DVector;

                            return;
                        }
                    }
                    break;
            }
        }
        protected virtual void CheckForDuration()
        {
            if (Time.time - _lastDirectionChangeTimestamp > MaximumDurationInADirection)
            {
                PickNewDirection();
                _lastDirectionChangeTimestamp = Time.time;
            }
        }
        public override void OnExitState()
        {
            base.OnExitState();

            _characterGridMovement?.StopMovement();
        }
    }
}
