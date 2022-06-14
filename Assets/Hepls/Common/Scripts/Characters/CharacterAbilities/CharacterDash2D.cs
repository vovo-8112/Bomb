using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine {
  [AddComponentMenu("TopDown Engine/Character/Abilities/Character Dash 2D")]
  public class CharacterDash2D : CharacterAbility {
    public enum DashModes {
      Fixed,
      MainMovement,
      SecondaryMovement,
      MousePosition
    }
    [Tooltip("the dash mode to apply the dash in")]
    public DashModes DashMode = DashModes.MainMovement;

    [Header("Dash")]
    [Tooltip("the dash direction")]
    public Vector3 DashDirection = Vector3.forward;
    [Tooltip("the distance the dash should last for")]
    public float DashDistance = 6f;
    [Tooltip("the duration of the dash, in seconds")]
    public float DashDuration = 0.2f;
    [Tooltip("the animation curve to apply to the dash acceleration")]
    public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    [Header("Cooldown")]
    [Tooltip("this ability's cooldown")]
    public MMCooldown Cooldown;

    [Header("Feedback")]
    [Tooltip("the feedbacks to play when dashing")]
    public MMFeedbacks DashFeedback;

    protected bool _dashing;
    protected float _dashTimer;
    protected Vector3 _dashOrigin;
    protected Vector3 _dashDestination;
    protected Vector3 _newPosition;
    protected Vector3 _dashAnimParameterDirection;
    protected Vector3 _dashAngle = Vector3.zero;
    protected Vector3 _inputPosition;
    protected Camera _mainCamera;
    protected const string _dashingAnimationParameterName = "Dashing";
    protected const string _dashingDirectionXAnimationParameterName = "DashingDirectionX";
    protected const string _dashingDirectionYAnimationParameterName = "DashingDirectionY";
    protected int _dashingAnimationParameter;
    protected int _dashingDirectionXAnimationParameter;
    protected int _dashingDirectionYAnimationParameter;
    protected override void Initialization() {
      base.Initialization();
      Cooldown.Initialization();

      _mainCamera = Camera.main;

      if (GUIManager.Instance != null && _character.CharacterType == Character.CharacterTypes.Player) {
        GUIManager.Instance.SetDashBar(true, _character.PlayerID);
        UpdateDashBar();
      }
    }
    protected override void HandleInput() {
      base.HandleInput();
      if (!AbilityAuthorized
          || (!Cooldown.Ready())
          || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)) {
        return;
      }

      if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) {
        DashStart();
      }
    }
    public virtual void DashStart() {
      if (!Cooldown.Ready()) {
        return;
      }

      Cooldown.Start();
      _movement.ChangeState(CharacterStates.MovementStates.Dashing);
      _dashing = true;
      _dashTimer = 0f;
      _dashOrigin = this.transform.position;
      _controller.FreeMovement = false;
      DashFeedback?.PlayFeedbacks(this.transform.position);
      PlayAbilityStartFeedbacks();

      switch (DashMode) {
        case DashModes.MainMovement:
          _dashDestination = this.transform.position + _controller.CurrentDirection.normalized * DashDistance;
          break;

        case DashModes.Fixed:
          _dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
          break;

        case DashModes.SecondaryMovement:
          _dashDestination = this.transform.position +
                             (Vector3) _character.LinkedInputManager.SecondaryMovement.normalized * DashDistance;
          break;

        case DashModes.MousePosition:
          _inputPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
          _inputPosition.z = this.transform.position.z;
          _dashDestination = this.transform.position +
                             (_inputPosition - this.transform.position).normalized * DashDistance;
          break;
      }
    }
    protected virtual void DashStop() {
      DashFeedback?.StopFeedbacks(this.transform.position);

      StopStartFeedbacks();
      PlayAbilityStopFeedbacks();

      _movement.ChangeState(CharacterStates.MovementStates.Idle);
      _dashing = false;
      _controller.FreeMovement = true;
    }
    public override void ProcessAbility() {
      base.ProcessAbility();
      Cooldown.Update();
      UpdateDashBar();

      if (_dashing) {
        if (_dashTimer < DashDuration) {
          _dashAnimParameterDirection = (_dashDestination - _dashOrigin).normalized;
          _newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));
          _dashTimer += Time.deltaTime;
          _controller.MovePosition(_newPosition);
        } else {
          DashStop();
        }
      }
    }
    protected virtual void UpdateDashBar() {
      if ((GUIManager.Instance != null) && (_character.CharacterType == Character.CharacterTypes.Player)) {
        GUIManager.Instance.UpdateDashBars(Cooldown.CurrentDurationLeft, 0f, Cooldown.ConsumptionDuration,
          _character.PlayerID);
      }
    }
    protected override void InitializeAnimatorParameters() {
      RegisterAnimatorParameter(_dashingAnimationParameterName, AnimatorControllerParameterType.Bool,
        out _dashingAnimationParameter);
      RegisterAnimatorParameter(_dashingDirectionXAnimationParameterName, AnimatorControllerParameterType.Float,
        out _dashingDirectionXAnimationParameter);
      RegisterAnimatorParameter(_dashingDirectionYAnimationParameterName, AnimatorControllerParameterType.Float,
        out _dashingDirectionYAnimationParameter);
    }
    public override void UpdateAnimator() {
      MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter,
        (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters,
        _character.RunAnimatorSanityChecks);
      MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionXAnimationParameter,
        _dashAnimParameterDirection.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
      MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionYAnimationParameter,
        _dashAnimParameterDirection.y, _character._animatorParameters, _character.RunAnimatorSanityChecks);
    }
  }
}