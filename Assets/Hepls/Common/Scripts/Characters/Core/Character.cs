using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    [SelectionBase]
    [AddComponentMenu("TopDown Engine/Character/Core/Character")]
    public class Character : MonoBehaviour
    {
        public enum FacingDirections
        {
            West,
            East,
            North,
            South
        }

        public enum CharacterDimensions
        {
            Type2D,
            Type3D
        }

        [MMReadOnly]
        public CharacterDimensions CharacterDimension;
        public enum CharacterTypes
        {
            Player,
            AI
        }

        [MMInformation(
            "The Character script is the mandatory basis for all Character abilities. Your character can either be a Non Player Character, controlled by an AI, or a Player character, controlled by the player. In this case, you'll need to specify a PlayerID, which must match the one specified in your InputManager. Usually 'Player1', 'Player2', etc.",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("Is the character player-controlled or controlled by an AI ?")]
        public CharacterTypes CharacterType = CharacterTypes.AI;
        [Tooltip(
            "Only used if the character is player-controlled. The PlayerID must match an input manager's PlayerID. It's also used to match Unity's input settings. So you'll be safe if you keep to Player1, Player2, Player3 or Player4")]
        public string PlayerID = "";
        public CharacterStates CharacterState { get; protected set; }

        [Header("Animator")]
        [MMInformation(
            "The engine will try and find an animator for this character. If it's on the same gameobject it should have found it. If it's nested somewhere, you'll need to bind it below. You can also decide to get rid of it altogether, in that case, just uncheck 'use mecanim'.",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the character animator, that this class and all abilities should update parameters on")]
        public Animator CharacterAnimator;
        [Tooltip("Set this to false if you want to implement your own animation system")]
        public bool UseDefaultMecanim = true;
        [Tooltip(
            "If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.")]
        public bool RunAnimatorSanityChecks = false;
        [Tooltip("if this is true, animator logs for the associated animator will be turned off to avoid potential spam")]
        public bool DisableAnimatorLogs = true;

        [Header("Model")]
        [MMInformation(
            "Leave this unbound if this is a regular, sprite-based character, and if the SpriteRenderer and the Character are on the same GameObject. If not, you'll want to parent the actual model to the Character object, and bind it below. See the 3D demo characters for an example of that. The idea behind that is that the model may move, flip, but the collider will remain unchanged.",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip(
            "the 'model' (can be any gameobject) used to manipulate the character. Ideally it's separated (and nested) from the collider/TopDown controller/abilities, to avoid messing with collisions.")]
        public GameObject CharacterModel;

        [Header("Events")]
        [MMInformation(
            "Here you can define whether or not you want to have that character trigger events when changing state. See the MMTools' State Machine doc for more info.",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("If this is true, the Character's state machine will emit events when entering/exiting a state")]
        public bool SendStateChangeEvents = true;

        [Header("Abilities")]
        [Tooltip("A list of gameobjects (usually nested under the Character) under which to search for additional abilities")]
        public List<GameObject> AdditionalAbilityNodes;

        [Header("AI")]
        [Tooltip(
            "The brain currently associated with this character, if it's an Advanced AI. By default the engine will pick the one on this object, but you can attach another one if you'd like")]
        public AIBrain CharacterBrain;
        [Tooltip("Whether to optimize this character for mobile. Will disable its cone of vision on mobile")]
        public bool OptimizeForMobile = true;
        public MMStateMachine<CharacterStates.MovementStates> MovementState;

        public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;
        public InputManager LinkedInputManager { get; protected set; }
        public Animator _animator { get; protected set; }
        public HashSet<int> _animatorParameters { get; set; }
        public CharacterOrientation2D Orientation2D { get; protected set; }
        public CharacterOrientation3D Orientation3D { get; protected set; }
        public GameObject CameraTarget { get; set; }
        public Health _health { get; protected set; }
        public Vector3 CameraDirection { get; protected set; }

        protected CharacterAbility[] _characterAbilities;
        protected bool _abilitiesCachedOnce = false;
        protected TopDownController _controller;
        protected float _animatorRandomNumber;
        protected bool _spawnDirectionForced = false;

        protected const string _groundedAnimationParameterName = "Grounded";
        protected const string _aliveAnimationParameterName = "Alive";
        protected const string _currentSpeedAnimationParameterName = "CurrentSpeed";
        protected const string _xSpeedAnimationParameterName = "xSpeed";
        protected const string _ySpeedAnimationParameterName = "ySpeed";
        protected const string _zSpeedAnimationParameterName = "zSpeed";
        protected const string _xVelocityAnimationParameterName = "xVelocity";
        protected const string _yVelocityAnimationParameterName = "yVelocity";
        protected const string _zVelocityAnimationParameterName = "zVelocity";
        protected const string _idleAnimationParameterName = "Idle";
        protected const string _randomAnimationParameterName = "Random";
        protected const string _randomConstantAnimationParameterName = "RandomConstant";
        protected int _groundedAnimationParameter;
        protected int _aliveAnimationParameter;
        protected int _currentSpeedAnimationParameter;
        protected int _xSpeedAnimationParameter;
        protected int _ySpeedAnimationParameter;
        protected int _zSpeedAnimationParameter;
        protected int _xVelocityAnimationParameter;
        protected int _yVelocityAnimationParameter;
        protected int _zVelocityAnimationParameter;
        protected int _idleAnimationParameter;
        protected int _randomAnimationParameter;
        protected int _randomConstantAnimationParameter;
        protected bool _animatorInitialized = false;
        protected CharacterPersistence _characterPersistence;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (this.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
            {
                CharacterDimension = CharacterDimensions.Type2D;
            }

            if (this.gameObject.MMGetComponentNoAlloc<TopDownController3D>() != null)
            {
                CharacterDimension = CharacterDimensions.Type3D;
            }
            MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject, SendStateChangeEvents);
            ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject, SendStateChangeEvents);
            SetInputManager();
            CharacterState = new CharacterStates();
            _controller = this.gameObject.GetComponent<TopDownController>();
            _health = this.gameObject.GetComponent<Health>();

            CacheAbilitiesAtInit();

            if (CharacterBrain == null)
            {
                CharacterBrain = this.gameObject.GetComponent<AIBrain>();
            }

            Orientation2D = FindAbility<CharacterOrientation2D>();
            Orientation3D = FindAbility<CharacterOrientation3D>();
            _characterPersistence = FindAbility<CharacterPersistence>();

            AssignAnimator();
            if (CameraTarget == null)
            {
                CameraTarget = new GameObject();
            }

            CameraTarget.transform.SetParent(this.transform);
            CameraTarget.transform.localPosition = Vector3.zero;
            CameraTarget.name = "CameraTarget";

            if (LinkedInputManager != null)
            {
                if (OptimizeForMobile && LinkedInputManager.IsMobile)
                {
                    if (this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>() != null)
                    {
                        this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>().enabled = false;
                    }
                }
            }
        }

        protected virtual void CacheAbilitiesAtInit()
        {
            if (_abilitiesCachedOnce)
            {
                return;
            }

            CacheAbilities();
        }
        public virtual void CacheAbilities()
        {
            _characterAbilities = this.gameObject.GetComponents<CharacterAbility>();
            if ((AdditionalAbilityNodes != null) && (AdditionalAbilityNodes.Count > 0))
            {
                List<CharacterAbility> tempAbilityList = new List<CharacterAbility>();
                for (int i = 0; i < _characterAbilities.Length; i++)
                {
                    tempAbilityList.Add(_characterAbilities[i]);
                }
                for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
                {
                    CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();

                    foreach (CharacterAbility ability in tempArray)
                    {
                        tempAbilityList.Add(ability);
                    }
                }

                _characterAbilities = tempAbilityList.ToArray();
            }

            _abilitiesCachedOnce = true;
        }
        public T FindAbility<T>() where T : CharacterAbility
        {
            CacheAbilitiesAtInit();

            Type searchedAbilityType = typeof(T);

            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability is T characterAbility)
                {
                    return characterAbility;
                }
            }

            return null;
        }
        public virtual void AssignAnimator()
        {
            if (_animatorInitialized)
            {
                return;
            }

            _animatorParameters = new HashSet<int>();

            if (CharacterAnimator != null)
            {
                _animator = CharacterAnimator;
            }
            else
            {
                _animator = this.gameObject.GetComponent<Animator>();
            }

            if (_animator != null)
            {
                if (DisableAnimatorLogs)
                {
                    _animator.logWarnings = false;
                }

                InitializeAnimatorParameters();
            }

            _animatorInitialized = true;
        }
        protected virtual void InitializeAnimatorParameters()
        {
            if (_animator == null)
            {
                return;
            }

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _groundedAnimationParameterName, out _groundedAnimationParameter,
                AnimatorControllerParameterType.Bool, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _currentSpeedAnimationParameterName, out _currentSpeedAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xSpeedAnimationParameterName, out _xSpeedAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _ySpeedAnimationParameterName, out _ySpeedAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _zSpeedAnimationParameterName, out _zSpeedAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _idleAnimationParameterName, out _idleAnimationParameter,
                AnimatorControllerParameterType.Bool, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _aliveAnimationParameterName, out _aliveAnimationParameter,
                AnimatorControllerParameterType.Bool, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomAnimationParameterName, out _randomAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomConstantAnimationParameterName, out _randomConstantAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xVelocityAnimationParameterName, out _xVelocityAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _yVelocityAnimationParameterName, out _yVelocityAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _zVelocityAnimationParameterName, out _zVelocityAnimationParameter,
                AnimatorControllerParameterType.Float, _animatorParameters);
            int randomConstant = Random.Range(0, 1000);

            MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _randomConstantAnimationParameter, randomConstant, _animatorParameters,
                RunAnimatorSanityChecks);
        }
        public virtual void SetInputManager()
        {
            if (CharacterType == CharacterTypes.AI)
            {
                LinkedInputManager = null;
                UpdateInputManagersInAbilities();
                return;
            }
            if (!string.IsNullOrEmpty(PlayerID))
            {
                LinkedInputManager = null;
                InputManager[] foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];

                foreach (InputManager foundInputManager in foundInputManagers)
                {
                    if (foundInputManager.PlayerID == PlayerID)
                    {
                        LinkedInputManager = foundInputManager;
                    }
                }
            }

            UpdateInputManagersInAbilities();
        }
        public virtual void SetInputManager(InputManager inputManager)
        {
            LinkedInputManager = inputManager;
            UpdateInputManagersInAbilities();
        }
        protected virtual void UpdateInputManagersInAbilities()
        {
            if (_characterAbilities == null)
            {
                return;
            }

            for (int i = 0; i < _characterAbilities.Length; i++)
            {
                _characterAbilities[i].SetInputManager(LinkedInputManager);
            }
        }
        public virtual void ResetInput()
        {
            if (_characterAbilities == null)
            {
                return;
            }

            foreach (CharacterAbility ability in _characterAbilities)
            {
                ability.ResetInput();
            }
        }
        public virtual void SetPlayerID(string newPlayerID)
        {
            PlayerID = newPlayerID;
            SetInputManager();
        }
        protected virtual void Update()
        {
            EveryFrame();
        }
        protected virtual void EveryFrame()
        {
            EarlyProcessAbilities();
            ProcessAbilities();
            LateProcessAbilities();
            UpdateAnimators();
        }
        protected virtual void EarlyProcessAbilities()
        {
            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.EarlyProcessAbility();
                }
            }
        }
        protected virtual void ProcessAbilities()
        {
            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.ProcessAbility();
                }
            }
        }
        protected virtual void LateProcessAbilities()
        {
            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.LateProcessAbility();
                }
            }
        }
        protected virtual void UpdateAnimators()
        {
            UpdateAnimationRandomNumber();

            if ((UseDefaultMecanim) && (_animator != null))
            {
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, _groundedAnimationParameter, _controller.Grounded, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorBool(_animator, _aliveAnimationParameter,
                    (ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead), _animatorParameters, RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _currentSpeedAnimationParameter, _controller.CurrentMovement.magnitude, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xSpeedAnimationParameter, _controller.CurrentMovement.x, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _ySpeedAnimationParameter, _controller.CurrentMovement.y, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _zSpeedAnimationParameter, _controller.CurrentMovement.z, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, (MovementState.CurrentState == CharacterStates.MovementStates.Idle),
                    _animatorParameters, RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _randomAnimationParameter, _animatorRandomNumber, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xVelocityAnimationParameter, _controller.Velocity.x, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _yVelocityAnimationParameter, _controller.Velocity.y, _animatorParameters,
                    RunAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _zVelocityAnimationParameter, _controller.Velocity.z, _animatorParameters,
                    RunAnimatorSanityChecks);

                foreach (CharacterAbility ability in _characterAbilities)
                {
                    if (ability.enabled && ability.AbilityInitialized)
                    {
                        ability.UpdateAnimator();
                    }
                }
            }
        }
        public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection)
        {
            transform.position = spawnPoint.position;

            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            if (this.gameObject.MMGetComponentNoAlloc<Collider2D>() != null)
            {
                this.gameObject.MMGetComponentNoAlloc<Collider2D>().enabled = true;
            }
            if (this.gameObject.MMGetComponentNoAlloc<Collider>() != null)
            {
                this.gameObject.MMGetComponentNoAlloc<Collider>().enabled = true;
            }
            _controller.enabled = true;
            _controller.CollisionsOn();
            _controller.Reset();
            if (this.gameObject.MMGetComponentNoAlloc<Rigidbody>() != null)
            {
                this.gameObject.MMGetComponentNoAlloc<Rigidbody>().velocity = Vector3.zero;
            }

            if (this.gameObject.MMGetComponentNoAlloc<Rigidbody2D>() != null)
            {
                this.gameObject.MMGetComponentNoAlloc<Rigidbody2D>().velocity = Vector3.zero;
            }

            Reset();
            UnFreeze();

            if (_health != null)
            {
                if (_characterPersistence != null)
                {
                    if (_characterPersistence.Initialized)
                    {
                        if (_health != null)
                        {
                            _health.UpdateHealthBar(false);
                        }

                        return;
                    }
                }

                _health.ResetHealthToMaxHealth();
                _health.Revive();
            }

            if (CharacterBrain != null)
            {
                CharacterBrain.enabled = true;
            }
            if (FindAbility<CharacterOrientation2D>() != null)
            {
                FindAbility<CharacterOrientation2D>().Face(facingDirection);
            }
            if (FindAbility<CharacterOrientation3D>() != null)
            {
                FindAbility<CharacterOrientation3D>().Face(facingDirection);
            }
        }
        public virtual void FlipAllAbilities()
        {
            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled)
                {
                    ability.Flip();
                }
            }
        }
        protected virtual void UpdateAnimationRandomNumber()
        {
            _animatorRandomNumber = Random.Range(0f, 1f);
        }
        public virtual void SetCameraDirection(Vector3 direction)
        {
            CameraDirection = direction;
        }
        public virtual void Freeze()
        {
            _controller.SetGravityActive(false);
            _controller.SetMovement(Vector2.zero);
            ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
        }
        public virtual void UnFreeze()
        {
            _controller.SetGravityActive(true);
            ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
        }
        public virtual void Disable()
        {
            this.enabled = false;
            _controller.enabled = false;
        }
        public virtual void Reset()
        {
            _spawnDirectionForced = false;

            if (_characterAbilities == null)
            {
                return;
            }

            if (_characterAbilities.Length == 0)
            {
                return;
            }

            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled)
                {
                    ability.ResetAbility();
                }
            }
        }
        protected virtual void OnRevive()
        {
            if (CharacterBrain != null)
            {
                CharacterBrain.enabled = true;
                CharacterBrain.ResetBrain();
            }
        }

        protected virtual void OnDeath()
        {
            if (CharacterBrain != null)
            {
                CharacterBrain.TransitionToState("");
                CharacterBrain.enabled = false;
            }

            if (MovementState.CurrentState != CharacterStates.MovementStates.FallingDownHole)
            {
                MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            }
        }

        protected virtual void OnHit()
        {
        }
        protected virtual void OnEnable()
        {
            if (_health != null)
            {
                _health.OnRevive += OnRevive;
                _health.OnDeath += OnDeath;
                _health.OnHit += OnHit;
            }
        }
        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
                _health.OnHit -= OnHit;
            }
        }
    }
}