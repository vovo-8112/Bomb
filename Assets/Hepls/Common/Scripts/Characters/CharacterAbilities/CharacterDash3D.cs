﻿ using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Dash 3D")]
    public class CharacterDash3D : CharacterAbility
    {
        public enum DashModes { Fixed, MainMovement, SecondaryMovement, MousePosition }
        [Tooltip("the current dash mode (fixed : always the same direction, MainMovement : usually your left stick, SecondaryMovement : usually your right stick, MousePosition : the cursor's position")]
        public DashModes DashMode = DashModes.MainMovement;
        
        [Header("Dash")]
        [Tooltip("the direction of the dash, relative to the character")]
        public Vector3 DashDirection = Vector3.forward;
        [Tooltip("the distance to cover")]
        public float DashDistance = 10f;
        [Tooltip("the duration of the dash, in seconds")]
        public float DashDuration = 0.5f;
        [Tooltip("the curve to apply to the dash's acceleration")]
        public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        [Tooltip("if this is true, dash will be allowed while jumping, otherwise it'll be ignored")]
        public bool AllowDashWhenJumping = false;

        [Header("Cooldown")]
        [Tooltip("this ability's cooldown")]
        public MMCooldown Cooldown;
        [Tooltip("the feedbacks to play when dashing")]
        public MMFeedbacks DashFeedback;

        protected bool _dashing;
        protected bool _dashStartedThisFrame;
        protected float _dashTimer;
        protected Vector3 _dashOrigin;
        protected Vector3 _dashDestination;
        protected Vector3 _newPosition;
        protected Vector3 _dashAngle = Vector3.zero;
        protected Vector3 _inputDirection;
        protected Vector3 _dashAnimParameterDirection;
        protected Plane _playerPlane;
        protected Camera _mainCamera;
        protected const string _dashingAnimationParameterName = "Dashing";
        protected const string _dashStartedAnimationParameterName = "DashStarted";
        protected const string _dashingDirectionXAnimationParameterName = "DashingDirectionX";
        protected const string _dashingDirectionYAnimationParameterName = "DashingDirectionY";
        protected const string _dashingDirectionZAnimationParameterName = "DashingDirectionZ";
        protected int _dashingAnimationParameter;
        protected int _dashStartedAnimationParameter;
        protected int _dashingDirectionXAnimationParameter;
        protected int _dashingDirectionYAnimationParameter;
        protected int _dashingDirectionZAnimationParameter;
        protected override void Initialization()
        {
            base.Initialization();
            _playerPlane = new Plane(Vector3.up, Vector3.zero);
            _mainCamera = Camera.main;
            Cooldown.Initialization();
            DashFeedback?.Initialization(this.gameObject);
        }
        protected override void HandleInput()
        {
            base.HandleInput();
            if (!AbilityAuthorized
                || (!Cooldown.Ready())
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }

            if (!AllowDashWhenJumping && (_movement.CurrentState == CharacterStates.MovementStates.Jumping))
            {
                return;
            }

            if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                DashStart();
            }
        }
        public virtual void DashStart()
        {
            if (!Cooldown.Ready())
            {
                return;
            }
            Cooldown.Start();

            float angle  = 0f;
            _movement.ChangeState(CharacterStates.MovementStates.Dashing);
            _dashing = true;
            _dashTimer = 0f;
            _dashOrigin = this.transform.position;
            _controller.FreeMovement = false;
            DashFeedback?.PlayFeedbacks(this.transform.position);
            PlayAbilityStartFeedbacks();
            _dashStartedThisFrame = true;

            switch (DashMode)
            {
                case DashModes.MainMovement:
                    angle = Vector3.SignedAngle(this.transform.forward, _controller.CurrentDirection.normalized, Vector3.up);
                    _dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
                    _dashAngle.y = angle;
                    _dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);
                    break;

                case DashModes.Fixed:
                    _dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
                    break;

                case DashModes.SecondaryMovement:
                    _inputDirection = _character.LinkedInputManager.SecondaryMovement;
                    _inputDirection.z = _inputDirection.y;
                    _inputDirection.y = 0;

                    angle = Vector3.SignedAngle(this.transform.forward, _inputDirection.normalized, Vector3.up);
                    _dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
                    _dashAngle.y = angle;
                    _dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);

                    _controller.CurrentDirection = (_dashDestination - this.transform.position).normalized;
                    break;

                case DashModes.MousePosition:
                    Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                    Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
                    float distance;
                    _playerPlane.SetNormalAndPosition(_playerPlane.normal, this.transform.position);
                    if (_playerPlane.Raycast(ray, out distance))
                    {
                        _inputDirection = ray.GetPoint(distance);
                    }

                    angle = Vector3.SignedAngle(this.transform.forward, (_inputDirection - this.transform.position).normalized, Vector3.up);
                    _dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
                    _dashAngle.y = angle;
                    _dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);

                    _controller.CurrentDirection = (_dashDestination - this.transform.position).normalized;
                    break;
            }
        }
        protected virtual void DashStop()
        {
            Cooldown.Stop();
            _movement.ChangeState(CharacterStates.MovementStates.Idle);
            _dashing = false;
            _controller.FreeMovement = true;
            DashFeedback?.StopFeedbacks();
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            Cooldown.Update();

            if (_dashing)
            {
                if (_dashTimer < DashDuration)
                {
                    _dashAnimParameterDirection = (_dashDestination - _dashOrigin).normalized;
                    _newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));
                    _dashTimer += Time.deltaTime;
                    _controller.MovePosition(_newPosition);
                }
                else
                {
                    DashStop();                   
                }
            }
        }
        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_dashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashingAnimationParameter);
            RegisterAnimatorParameter(_dashStartedAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashStartedAnimationParameter);
            RegisterAnimatorParameter(_dashingDirectionXAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionXAnimationParameter);
            RegisterAnimatorParameter(_dashingDirectionYAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionYAnimationParameter);
            RegisterAnimatorParameter(_dashingDirectionZAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionZAnimationParameter);
        }
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashStartedAnimationParameter, _dashStartedThisFrame, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionXAnimationParameter, _dashAnimParameterDirection.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionYAnimationParameter, _dashAnimParameterDirection.y, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionZAnimationParameter, _dashAnimParameterDirection.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);

            _dashStartedThisFrame = false;
        }
    }
}