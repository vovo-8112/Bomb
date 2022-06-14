using System.Collections;
using MoreMountains.Tools;
using Photon.Pun;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Grid Movement")]
    public class CharacterGridMovement : CharacterAbility
    {
        public enum GridDirections
        {
            None,
            Up,
            Down,
            Left,
            Right
        }
        public enum InputModes
        {
            InputManager,
            Script
        }
        public enum DimensionModes
        {
            TwoD,
            ThreeD
        }

        [Header("Movement")]
        [Tooltip("the maximum speed of the character")]
        public float MaximumSpeed = 8;
        [Tooltip("the acceleration of the character")]
        public float Acceleration = 5;
        [MMReadOnly]
        [Tooltip("the current speed at which the character is going")]
        public float CurrentSpeed;
        [MMReadOnly]
        [Tooltip("a multiplier to apply to the maximum speed")]
        public float MaximumSpeedMultiplier = 1f;
        [MMReadOnly]
        [Tooltip("a multiplier to apply to the acceleration, letting you modify it safely from outside")]
        public float AccelerationMultiplier = 1f;

        [Header("Input Settings")]
        [Tooltip("whether to use the input manager or a script to feed the inputs")]
        public InputModes InputMode = InputModes.InputManager;
        [Tooltip("whether or not input should be buffered (to anticipate the next turn)")]
        public bool UseInputBuffer = true;
        [Tooltip("the size of the input buffer (in grid units)")]
        public int BufferSize = 2;
        [Tooltip("whether or not the agent can perform fast direction changes such as U turns")]
        public bool FastDirectionChanges = true;
        [Tooltip("the speed threshold after which the character is not considered idle anymore")]
        public float IdleThreshold = 0.05f;
        [Tooltip("if this is true, movement values will be normalized - prefer checking this when using mobile controls")]
        public bool NormalizedInput = false;

        [Header("Grid")]
        [Tooltip("the offset to apply when detecting obstacles")]
        public Vector3 ObstacleDetectionOffset = new Vector3(0f, 0.5f, 0f);
        public Vector3Int CurrentGridPosition { get; protected set; }
        public Vector3Int TargetGridPosition { get; protected set; }
        [MMReadOnly]
        [Tooltip("this is true everytime a character is at the exact position of a tile")]
        public bool PerfectTile;
        [MMReadOnly]
        [Tooltip("the coordinates of the cell this character currently occupies")]
        public Vector3Int CurrentCellCoordinates;
        [MMReadOnly]
        [Tooltip("whether this character is in 2D or 3D. This gets automatically computed at start")]
        public DimensionModes DimensionMode = DimensionModes.TwoD;

        [Header("Test")]
        [MMInspectorButton("Left")]
        public bool LeftButton;

        [MMInspectorButton("Right")]
        public bool RightButton;

        [MMInspectorButton("Up")]
        public bool UpButton;

        [MMInspectorButton("Down")]
        public bool DownButton;

        [MMInspectorButton("StopMovement")]
        public bool StopButton;

        [MMInspectorButton("LeftOneCell")]
        public bool LeftOneCellButton;

        [MMInspectorButton("RightOneCell")]
        public bool RightOneCellButton;

        [MMInspectorButton("UpOneCell")]
        public bool UpOneCellButton;

        [MMInspectorButton("DownOneCell")]
        public bool DownOneCellButton;

        protected GridDirections _inputDirection;
        protected GridDirections _currentDirection = GridDirections.Up;
        protected GridDirections _bufferedDirection;
        protected bool _movementInterruptionBuffered = false;
        protected bool _perfectTile = false;
        protected Vector3 _inputMovement;
        protected Vector3 _endWorldPosition;
        protected bool _movingToNextGridUnit = false;
        protected bool _stopBuffered = false;
        protected int _lastBufferInGridUnits;
        protected bool _agentMoving;
        protected GridDirections _newDirection;
        protected float _horizontalMovement;
        protected float _verticalMovement;
        protected Vector3Int _lastOccupiedCellCoordinates;
        protected const string _speedAnimationParameterName = "Speed";
        protected const string _walkingAnimationParameterName = "Walking";
        protected const string _idleAnimationParameterName = "Idle";
        protected int _speedAnimationParameter;
        protected int _walkingAnimationParameter;
        protected int _idleAnimationParameter;
        protected bool _firstPositionRegistered = false;
        protected Vector3Int _newCellCoordinates = Vector3Int.zero;
        protected Vector3 _lastCurrentDirection;

        protected bool _leftPressedLastFrame = false;
        protected bool _rightPressedLastFrame = false;
        protected bool _downPressedLastFrame = false;
        protected bool _upPressedLastFrame = false;
        private PhotonView photonView;
        protected override void Awake()
        {
            photonView = GetComponent<PhotonView>();
            base.Awake();
        }

        public virtual void SetMovement(Vector2 newMovement)
        {
            _horizontalMovement = newMovement.x;
            _verticalMovement = newMovement.y;
        }
        public virtual void LeftOneCell()
        {
            StartCoroutine(OneCell(Vector2.left));
        }
        public virtual void RightOneCell()
        {
            StartCoroutine(OneCell(Vector2.right));
        }
        public virtual void UpOneCell()
        {
            StartCoroutine(OneCell(Vector2.up));
        }
        public virtual void DownOneCell()
        {
            StartCoroutine(OneCell(Vector2.down));
        }
        protected virtual IEnumerator OneCell(Vector2 direction)
        {
            SetMovement(direction);
            yield return null;

            SetMovement(Vector2.zero);
        }
        public virtual void Left()
        {
            SetMovement(Vector2.left);
        }
        public virtual void Right()
        {
            SetMovement(Vector2.right);
        }
        public virtual void Up()
        {
            SetMovement(Vector2.up);
        }
        public virtual void Down()
        {
            SetMovement(Vector2.down);
        }
        public virtual void StopMovement()
        {
            SetMovement(Vector2.zero);
        }
        protected override void Initialization()
        {
            base.Initialization();
            DimensionMode = DimensionModes.ThreeD;

            if (_controller.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
            {
                DimensionMode = DimensionModes.TwoD;
                _controller.FreeMovement = false;
            }

            _controller.PerformCardinalObstacleRaycastDetection = true;
            _bufferedDirection = GridDirections.None;
        }
        protected virtual void RegisterFirstPosition()
        {
            if (!_firstPositionRegistered)
            {
                _endWorldPosition = this.transform.position;
                _lastOccupiedCellCoordinates = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);
                CurrentCellCoordinates = _lastOccupiedCellCoordinates;
                GridManager.Instance.OccupyCell(_lastOccupiedCellCoordinates);
                GridManager.Instance.SetLastPosition(this.gameObject, _lastOccupiedCellCoordinates);
                GridManager.Instance.SetNextPosition(this.gameObject, _lastOccupiedCellCoordinates);
                _firstPositionRegistered = true;
            }
        }

        public virtual void SetCurrentWorldPositionAsNewPosition()
        {
            _endWorldPosition = this.transform.position;
            GridManager.Instance.FreeCell(_lastOccupiedCellCoordinates);
            _lastOccupiedCellCoordinates = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);
            CurrentCellCoordinates = _lastOccupiedCellCoordinates;
            GridManager.Instance.OccupyCell(_lastOccupiedCellCoordinates);
            GridManager.Instance.SetLastPosition(this.gameObject, _lastOccupiedCellCoordinates);
            GridManager.Instance.SetNextPosition(this.gameObject, _lastOccupiedCellCoordinates);
            _firstPositionRegistered = true;
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();

            if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
            {
                return;
            }

            RegisterFirstPosition();

            _controller.DetectObstacles(GridManager.Instance.GridUnitSize, ObstacleDetectionOffset);
            DetermineInputDirection();
            ApplyAcceleration();
            HandleMovement();
            HandleState();
        }
        protected override void HandleInput()
        {
            if (!AbilityAuthorized)
            {
                return;
            }

            if (InputMode == InputModes.InputManager)
            {
                _horizontalMovement = NormalizedInput ? Mathf.Round(_horizontalInput) : _horizontalInput;
                _verticalMovement = NormalizedInput ? Mathf.Round(_verticalInput) : _verticalInput;
            }
        }
        protected virtual void DetermineInputDirection()
        {
            if ((Mathf.Abs(_horizontalMovement) <= IdleThreshold) && (Mathf.Abs(_verticalMovement) <= IdleThreshold))
            {
                Stop(_newDirection);
                _newDirection = GridDirections.None;
                _inputMovement = Vector3.zero;
            }
            if ((_horizontalMovement < 0f) && !_leftPressedLastFrame)
            {
                _newDirection = GridDirections.Left;
                _inputMovement = Vector3.left;
            }

            if ((_horizontalMovement > 0f) && !_rightPressedLastFrame)
            {
                _newDirection = GridDirections.Right;
                _inputMovement = Vector3.right;
            }

            if ((_verticalMovement < 0f) && !_downPressedLastFrame)
            {
                _newDirection = GridDirections.Down;
                _inputMovement = Vector3.down;
            }

            if ((_verticalMovement > 0f) && !_upPressedLastFrame)
            {
                _newDirection = GridDirections.Up;
                _inputMovement = Vector3.up;
            }
            if ((_horizontalMovement == 0f) && (_leftPressedLastFrame || _rightPressedLastFrame))
            {
                _newDirection = GridDirections.None;
            }

            if ((_verticalMovement == 0f) && (_downPressedLastFrame || _upPressedLastFrame))
            {
                _newDirection = GridDirections.None;
            }
            if (_newDirection == GridDirections.None)
            {
                if (_horizontalMovement < 0f)
                {
                    _newDirection = GridDirections.Left;
                    _inputMovement = Vector3.left;
                }

                if (_horizontalMovement > 0f)
                {
                    _newDirection = GridDirections.Right;
                    _inputMovement = Vector3.right;
                }

                if (_verticalMovement < 0f)
                {
                    _newDirection = GridDirections.Down;
                    _inputMovement = Vector3.down;
                }

                if (_verticalMovement > 0f)
                {
                    _newDirection = GridDirections.Up;
                    _inputMovement = Vector3.up;
                }
            }

            _inputDirection = _newDirection;
            _leftPressedLastFrame = (_horizontalMovement < 0f);
            _rightPressedLastFrame = (_horizontalMovement > 0f);
            _downPressedLastFrame = (_verticalMovement < 0f);
            _upPressedLastFrame = (_verticalMovement > 0f);
        }
        public virtual void Stop(GridDirections direction)
        {
            if (direction == GridDirections.None)
            {
                return;
            }

            _bufferedDirection = direction;
            _stopBuffered = true;
        }
        protected virtual void ApplyAcceleration()
        {
            if ((_currentDirection != GridDirections.None) && (CurrentSpeed < MaximumSpeed * MaximumSpeedMultiplier))
            {
                CurrentSpeed = CurrentSpeed + Acceleration * AccelerationMultiplier * Time.deltaTime;
                CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0f, MaximumSpeed * MaximumSpeedMultiplier);
            }
        }
        protected virtual void HandleMovement()
        {
            if (photonView != null)
            {
                if (!photonView.IsMine)
                {
                    return;
                }
            }


            _perfectTile = false;
            PerfectTile = false;
            ProcessBuffer();
            if (!_movingToNextGridUnit)
            {
                PerfectTile = true;
                if (_movementInterruptionBuffered)
                {
                    _perfectTile = true;
                    _movementInterruptionBuffered = false;
                    return;
                }
                if (_bufferedDirection == GridDirections.None)
                {
                    _currentDirection = GridDirections.None;
                    _bufferedDirection = GridDirections.None;
                    _agentMoving = false;
                    CurrentSpeed = 0;

                    GridManager.Instance.SetLastPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));
                    GridManager.Instance.SetNextPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));

                    return;
                }
                if (((_currentDirection == GridDirections.Left) && (_controller.DetectedObstacleLeft != null))
                    || ((_currentDirection == GridDirections.Right) && (_controller.DetectedObstacleRight != null))
                    || ((_currentDirection == GridDirections.Up) && (_controller.DetectedObstacleUp != null))
                    || ((_currentDirection == GridDirections.Down) && (_controller.DetectedObstacleDown != null)))
                {
                    _currentDirection = _bufferedDirection;

                    GridManager.Instance.SetLastPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));
                    GridManager.Instance.SetNextPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));

                    return;
                }
                if (((_bufferedDirection == GridDirections.Left) && !(_controller.DetectedObstacleLeft != null))
                    || ((_bufferedDirection == GridDirections.Right) && !(_controller.DetectedObstacleRight != null))
                    || ((_bufferedDirection == GridDirections.Up) && !(_controller.DetectedObstacleUp != null))
                    || ((_bufferedDirection == GridDirections.Down) && !(_controller.DetectedObstacleDown != null)))
                {
                    _currentDirection = _bufferedDirection;
                }
                _movingToNextGridUnit = true;
                DetermineEndPosition();
                if (GridManager.Instance.CellIsOccupied(TargetGridPosition))
                {
                    _movingToNextGridUnit = false;
                    _currentDirection = GridDirections.None;
                    _bufferedDirection = GridDirections.None;
                    _agentMoving = false;
                    CurrentSpeed = 0;
                }
                else
                {
                    GridManager.Instance.FreeCell(_lastOccupiedCellCoordinates);
                    GridManager.Instance.SetLastPosition(this.gameObject, _lastOccupiedCellCoordinates);
                    GridManager.Instance.SetNextPosition(this.gameObject, TargetGridPosition);
                    GridManager.Instance.OccupyCell(TargetGridPosition);
                    CurrentCellCoordinates = TargetGridPosition;
                    _lastOccupiedCellCoordinates = TargetGridPosition;
                }
            }
            TargetGridPosition = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);
            Vector3 newPosition = Vector3.MoveTowards(transform.position, _endWorldPosition, Time.deltaTime * CurrentSpeed);

            _lastCurrentDirection = _endWorldPosition - this.transform.position;
            _lastCurrentDirection = _lastCurrentDirection.MMRound();

            if (_lastCurrentDirection != Vector3.zero)
            {
                _controller.CurrentDirection = _lastCurrentDirection;
            }

            _controller.MovePosition(newPosition);
        }
        protected virtual void ProcessBuffer()
        {
            if ((_inputDirection != GridDirections.None) && !_stopBuffered)
            {
                _bufferedDirection = _inputDirection;
                _lastBufferInGridUnits = BufferSize;
            }
            if (!_agentMoving && _inputDirection != GridDirections.None)
            {
                _currentDirection = _inputDirection;
                _agentMoving = true;
            }
            if (_movingToNextGridUnit && (transform.position == _endWorldPosition))
            {
                _movingToNextGridUnit = false;
                CurrentGridPosition = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);
            }
            if ((_bufferedDirection != GridDirections.None) && !_movingToNextGridUnit && (_inputDirection == GridDirections.None) && UseInputBuffer)
            {
                _lastBufferInGridUnits--;
                if ((_lastBufferInGridUnits < 0) && (_bufferedDirection != _currentDirection))
                {
                    _bufferedDirection = _currentDirection;
                }
            }
            if ((_stopBuffered) && !_movingToNextGridUnit)
            {
                _bufferedDirection = GridDirections.None;
                _stopBuffered = false;
            }
        }
        protected virtual void DetermineEndPosition()
        {
            TargetGridPosition = CurrentCellCoordinates + ConvertDirectionToVector3Int(_currentDirection);
            _endWorldPosition = GridManager.Instance.CellToWorldCoordinates(TargetGridPosition);
            _endWorldPosition = DimensionClamp(_endWorldPosition);
        }

        protected Vector3 DimensionClamp(Vector3 newPosition)
        {
            if (DimensionMode == DimensionModes.TwoD)
            {
                newPosition.z = this.transform.position.z;
            }
            else
            {
                newPosition.y = this.transform.position.y;
            }

            return newPosition;
        }
        protected virtual Vector3Int ConvertDirectionToVector3Int(GridDirections direction)
        {
            if (direction != GridDirections.None)
            {
                if (direction == GridDirections.Left) return Vector3Int.left;
                if (direction == GridDirections.Right) return Vector3Int.right;

                if (DimensionMode == DimensionModes.TwoD)
                {
                    if (direction == GridDirections.Up) return Vector3Int.up;
                    if (direction == GridDirections.Down) return Vector3Int.down;
                }
                else
                {
                    if (direction == GridDirections.Up) return Vector3Int.RoundToInt(Vector3.forward);
                    if (direction == GridDirections.Down) return Vector3Int.RoundToInt(Vector3.back);
                }
            }

            return Vector3Int.zero;
        }
        protected virtual GridDirections GetInverseDirection(GridDirections direction)
        {
            if (direction != GridDirections.None)
            {
                if (direction == GridDirections.Left) return GridDirections.Right;
                if (direction == GridDirections.Right) return GridDirections.Left;
                if (direction == GridDirections.Up) return GridDirections.Down;
                if (direction == GridDirections.Down) return GridDirections.Up;
            }

            return GridDirections.None;
        }

        protected virtual void HandleState()
        {
            if (_movingToNextGridUnit)
            {
                if (_movement.CurrentState != CharacterStates.MovementStates.Walking)
                {
                    _movement.ChangeState(CharacterStates.MovementStates.Walking);
                    PlayAbilityStartFeedbacks();
                }
            }
            else
            {
                if (_movement.CurrentState != CharacterStates.MovementStates.Idle)
                {
                    _movement.ChangeState(CharacterStates.MovementStates.Idle);

                    if (_startFeedbackIsPlaying)
                    {
                        StopStartFeedbacks();
                        PlayAbilityStopFeedbacks();
                    }
                }
            }
        }
        protected override void OnDeath()
        {
            base.OnDeath();
            GridManager.Instance.FreeCell(_lastOccupiedCellCoordinates);
        }
        protected override void OnRespawn()
        {
            base.OnRespawn();
            Initialization();
            _firstPositionRegistered = false;
        }
        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_speedAnimationParameterName, AnimatorControllerParameterType.Float, out _speedAnimationParameter);
            RegisterAnimatorParameter(_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
            RegisterAnimatorParameter(_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
        }
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, CurrentSpeed, _character._animatorParameters,
                _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Walking),
                _character._animatorParameters, _character.RunAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Idle),
                _character._animatorParameters, _character.RunAnimatorSanityChecks);
        }
    }
}