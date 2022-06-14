using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Environment/Falling Platform 2D")]
    public class FallingPlatform2D : MonoBehaviour 
	{
        public enum FallingPlatformStates { Idle, Shaking, Falling, ColliderOff }
		[MMReadOnly]
		[Tooltip("the current state of the falling platform")]
		public FallingPlatformStates State;
		[Tooltip("if this is true, the platform will fall inevitably once touched")]
		public bool InevitableFall = false;
		[Tooltip("the time (in seconds) before the fall of the platform")]
		public float TimeBeforeFall = 2f;
		[Tooltip("the time (in seconds) before the collider turns itself off once the fall has started")]
		public float DelayBetweenFallAndColliderOff = 0.5f;
		protected Animator _animator;
		protected Vector2 _newPosition;
		protected Bounds _bounds;
		protected Collider2D _collider;
		protected Vector3 _initialPosition;
		protected float _timeLeftBeforeFall;
        protected float _fallStartedAt;
		protected bool _contact = false;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
            State = FallingPlatformStates.Idle;
            _animator = GetComponent<Animator>();
			_collider = GetComponent<Collider2D> ();
            _collider.enabled = true;
            _bounds =LevelManager.Instance.LevelBounds;
			_initialPosition = this.transform.position;
			_timeLeftBeforeFall = TimeBeforeFall;

        }
		protected virtual void FixedUpdate()
		{
			UpdateAnimator ();	

			if (_contact)
			{
				_timeLeftBeforeFall -= Time.deltaTime;
			}

			if (_timeLeftBeforeFall < 0)
			{
                if (State != FallingPlatformStates.Falling)
                {
                    _fallStartedAt = Time.time;
                }
                State = FallingPlatformStates.Falling;
			}

            if (State == FallingPlatformStates.Falling)
            {
                if (Time.time - _fallStartedAt >= DelayBetweenFallAndColliderOff)
                {
                    _collider.enabled = false;
                }
            }            
		}
		protected virtual void DisableFallingPlatform()
		{
			this.gameObject.SetActive (false);					
			this.transform.position = _initialPosition;		
			_timeLeftBeforeFall = TimeBeforeFall;
            State = FallingPlatformStates.Idle;
        }
		protected virtual void UpdateAnimator()
		{				
			if (_animator!=null)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Idle", (State == FallingPlatformStates.Idle));
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Shaking", (State == FallingPlatformStates.Shaking));
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Falling", (State == FallingPlatformStates.Falling));
            }
		}
        public virtual void OnTriggerStay2D(Collider2D collider)
		{
			TopDownController2D controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController2D>();
			if (controller == null)
			{
				return;
			}

            if (State == FallingPlatformStates.Falling)
            {
                return;
            }

			if (TimeBeforeFall>0)
			{
				_contact = true;
                State = FallingPlatformStates.Shaking;
			}	
			else
			{
				if (!InevitableFall)
				{
					_contact = false;
                    State = FallingPlatformStates.Idle;
                }
			}
		}
        protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			if (InevitableFall)
			{
				return;
			}

			TopDownController controller = collider.gameObject.GetComponent<TopDownController>();
			if (controller==null)
				return;

			_contact = false;
			if (State == FallingPlatformStates.Shaking)
            {
                State = FallingPlatformStates.Idle;
            }
		}
		protected virtual void OnRevive()
		{
			this.transform.position = _initialPosition;		
			_timeLeftBeforeFall = TimeBeforeFall;
            State = FallingPlatformStates.Idle;

        }
	}
}