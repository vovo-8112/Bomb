using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Environment/Falling Platform 3D")]
    public class FallingPlatform3D : MonoBehaviour 
	{
        public enum FallingPlatformStates { Idle, Shaking, Falling }
		[MMReadOnly]
		[Tooltip("the platform's current state")]
		public FallingPlatformStates State;
		[Tooltip("if this is true, the platform will fall inevitably once touched")]
		public bool InevitableFall = false;
		[Tooltip("the time (in seconds) before the fall of the platform")]
		public float TimeBeforeFall = 2f;
		[Tooltip("if this is true, the object's rigidbody will be turned non kinematic when falling. Only works in 3D.")]
		public bool UsePhysics = true;
		[Tooltip("the speed at which the platforms falls")]
		public float NonPhysicsFallSpeed = 2f;
		protected Animator _animator;
		protected Vector2 _newPosition;
		protected Bounds _bounds;
		protected Collider _collider;
		protected Vector3 _initialPosition;
		protected float _timer;
		protected float _platformTopY;
		protected Rigidbody _rigidbody;
		protected bool _contact = false;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
            State = FallingPlatformStates.Idle;
            _animator = GetComponent<Animator>();
			_collider = GetComponent<Collider> ();
			_bounds=LevelManager.Instance.LevelBounds;
			_initialPosition = this.transform.position;
			_timer = TimeBeforeFall;
			_rigidbody = GetComponent<Rigidbody> ();

        }
		protected virtual void FixedUpdate()
		{
			UpdateAnimator ();	

			if (_contact)
			{
				_timer -= Time.deltaTime;
			}

			if (_timer < 0)
			{
                State = FallingPlatformStates.Falling;
                if (UsePhysics)
				{
					_rigidbody.isKinematic = false;
				}
				else
				{
					_newPosition = new Vector2(0,-NonPhysicsFallSpeed*Time.deltaTime);
					transform.Translate(_newPosition,Space.World);

					if (transform.position.y < _bounds.min.y)
					{
						DisableFallingPlatform ();
					}	
				}
			}
		}
		protected virtual void DisableFallingPlatform()
		{
			this.gameObject.SetActive (false);					
			this.transform.position = _initialPosition;		
			_timer = TimeBeforeFall;
            State = FallingPlatformStates.Idle;
        }
		protected virtual void UpdateAnimator()
		{				
			if (_animator!=null)
			{
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Shaking", (State == FallingPlatformStates.Shaking));	
			}
		}
        public virtual void OnTriggerStay(Collider collider)
		{
			TopDownController controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController>();
			if (controller==null)
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
        protected virtual void OnTriggerExit(Collider collider)
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
			_timer = TimeBeforeFall;
            State = FallingPlatformStates.Idle;

        }
	}
}