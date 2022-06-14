using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Crouch")]
    public class CharacterCrouch : CharacterAbility 
	{
		public override string HelpBoxText() { return "This component handles crouch and crawl behaviours. Here you can determine the crouch speed, and whether or not the collider should resize when crouched (to crawl into tunnels for example). If it should, please setup its new size here."; }
        [MMReadOnly]
		[Tooltip("if this is true, the character is in ForcedCrouch mode. A CrouchZone or an AI script can do that.")]
		public bool ForcedCrouch = false;

		[Header("Crawl")]
		[Tooltip("if this is set to false, the character won't be able to crawl, just to crouch")]
		public bool CrawlAuthorized = true;
		[Tooltip("the speed of the character when it's crouching")]
		public float CrawlSpeed = 4f;

		[Space(10)]	
		[Header("Crouching")]
		[Tooltip("if this is true, the collider will be resized when crouched")]
		public bool ResizeColliderWhenCrouched = false;
		[Tooltip("the size to apply to the collider when crouched (if ResizeColliderWhenCrouched is true, otherwise this will be ignored)")]
		public float CrouchedColliderHeight = 1.25f;

		[Space(10)]	
		[Header("Offset")]
		[Tooltip("a list of objects to offset when crouching")]
		public List<GameObject> ObjectsToOffset;
		[Tooltip("the offset to apply to objects when crouching")]
		public Vector3 OffsetCrouch;
		[Tooltip("the offset to apply to objects when crouching AND moving")]
		public Vector3 OffsetCrawl;
		[Tooltip("the speed at which to offset objects")]
		public float OffsetSpeed = 5f;
        [MMReadOnly]
        [Tooltip("whether or not the character is in a tunnel right now and can't get up")]
		public bool InATunnel;

		protected List<Vector3> _objectsToOffsetOriginalPositions;
        protected const string _crouchingAnimationParameterName = "Crouching";
        protected const string _crawlingAnimationParameterName = "Crawling";
        protected int _crouchingAnimationParameter;
        protected int _crawlingAnimationParameter;
        protected bool _crouching = false;
        protected CharacterRun _characterRun;
        protected override void Initialization()
		{
			base.Initialization();
			InATunnel = false;
			_characterRun = _character.FindAbility<CharacterRun>();
			if (ObjectsToOffset.Count > 0)
			{
				_objectsToOffsetOriginalPositions = new List<Vector3> ();
				foreach(GameObject go in ObjectsToOffset)
				{
                    if (go != null)
                    {
                        _objectsToOffsetOriginalPositions.Add(go.transform.localPosition);
                    }					
				}
			}
		}
		public override void ProcessAbility()
		{
			base.ProcessAbility();
            HandleForcedCrouch();
            DetermineState ();
			CheckExitCrouch();
			OffsetObjects ();
		}
        protected virtual void HandleForcedCrouch()
        {
            if (ForcedCrouch && (_movement.CurrentState != CharacterStates.MovementStates.Crouching) && (_movement.CurrentState != CharacterStates.MovementStates.Crawling))
            {
                Crouch();
            }
        }
		protected override void HandleInput()
		{			
			base.HandleInput ();
			if (_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)		
			{
				Crouch();
			}
		}
        public virtual void StartForcedCrouch()
        {
            ForcedCrouch = true;
            _crouching = true;
        }
        public virtual void StopForcedCrouch()
        {
            ForcedCrouch = false;
            _crouching = false;
        }
		protected virtual void Crouch()
		{
			if (!AbilityAuthorized// if the ability is not permitted
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)// or if we're not in our normal stance
			    || (!_controller.Grounded))// or if we're grounded
			{
				return;
			}
			if ((_movement.CurrentState != CharacterStates.MovementStates.Crouching) && (_movement.CurrentState != CharacterStates.MovementStates.Crawling))
			{
				PlayAbilityStartSfx();
				PlayAbilityUsedSfx();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Running)
			{
				_characterRun.RunStop();
			}

			_crouching = true;
			_movement.ChangeState(CharacterStates.MovementStates.Crouching);
			if ( (Mathf.Abs(_horizontalInput) > 0) && (CrawlAuthorized) )
			{
				_movement.ChangeState(CharacterStates.MovementStates.Crawling);
			}
			if (ResizeColliderWhenCrouched)
			{
				_controller.ResizeColliderHeight(CrouchedColliderHeight);		
			}
			if (_characterMovement != null)
			{
				_characterMovement.MovementSpeed = CrawlSpeed;
			}
			if (!CrawlAuthorized)
			{
				_characterMovement.MovementSpeed = 0f;
			}
		}

		protected virtual void OffsetObjects ()
		{
			if (ObjectsToOffset.Count > 0)
			{
				for (int i = 0; i < ObjectsToOffset.Count; i++)
				{
					Vector3 newOffset = Vector3.zero;
					if (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
					{
						newOffset = OffsetCrouch;
					}
					if (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
					{
						newOffset = OffsetCrawl;
					}
                    if (ObjectsToOffset[i] != null)
                    {
                        ObjectsToOffset[i].transform.localPosition = Vector3.Lerp(ObjectsToOffset[i].transform.localPosition, _objectsToOffsetOriginalPositions[i] + newOffset, Time.deltaTime * OffsetSpeed);
                    }					
				}
			}
		}
		protected virtual void DetermineState()
		{
			if ((_movement.CurrentState == CharacterStates.MovementStates.Crouching) || (_movement.CurrentState == CharacterStates.MovementStates.Crawling))
			{
				if ( (_controller.CurrentMovement.magnitude > 0) && (CrawlAuthorized) )
				{
					_movement.ChangeState(CharacterStates.MovementStates.Crawling);
				}
				else
				{
					_movement.ChangeState(CharacterStates.MovementStates.Crouching);
				}
			}
		}
		protected virtual void CheckExitCrouch()
		{
			if ( (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
				|| (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
				|| _crouching)
			{	
                if (_inputManager == null)
                {
                    if (!ForcedCrouch)
                    {
                        ExitCrouch();
                    }
                    return;
                }
				if ( (!_controller.Grounded) 
				     || ((_movement.CurrentState != CharacterStates.MovementStates.Crouching) 
				         && (_movement.CurrentState != CharacterStates.MovementStates.Crawling)
				         && (_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.Off) && (!ForcedCrouch))
				     || ((_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.Off) && (!ForcedCrouch)))
				{
					InATunnel = !_controller.CanGoBackToOriginalSize();
					if (!InATunnel)
					{
                        ExitCrouch();
                    }
				}
			}
		}
        protected virtual void ExitCrouch()
        {
	        _crouching = false;
            if (_characterMovement != null)
            {
                _characterMovement.ResetSpeed();
            }
            StopAbilityUsedSfx();
            PlayAbilityStopSfx();
            if ((_movement.CurrentState == CharacterStates.MovementStates.Crawling) ||
                (_movement.CurrentState == CharacterStates.MovementStates.Crouching))
            {
	            _movement.ChangeState(CharacterStates.MovementStates.Idle);    
            }
            
            _controller.ResetColliderSize();
        }
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_crouchingAnimationParameterName, AnimatorControllerParameterType.Bool, out _crouchingAnimationParameter);
			RegisterAnimatorParameter (_crawlingAnimationParameterName, AnimatorControllerParameterType.Bool, out _crawlingAnimationParameter);
		}
		public override void UpdateAnimator()
		{
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _crouchingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Crouching), _character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator,_crawlingAnimationParameter,(_movement.CurrentState == CharacterStates.MovementStates.Crawling), _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
