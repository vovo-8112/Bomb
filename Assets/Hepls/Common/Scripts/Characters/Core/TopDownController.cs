using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	public class TopDownController : MonoBehaviour 
	{
        [Header("Gravity")]
		[Tooltip("the current gravity to apply to our character (positive goes down, negative goes up, higher value, higher acceleration)")]
        public float Gravity = 40f;
        [Tooltip("whether or not the gravity is currently being applied to this character")]
        public bool GravityActive = true;

        [Header("General Raycasts")]
        [Tooltip("by default, the length of the raycasts used to get back to normal size will be auto generated based on your character's normal/standing height, but here you can specify a different value")]
        public float CrouchedRaycastLengthMultiplier = 1f;
        [Tooltip("if this is true, extra raycasts will be cast on all 4 sides to detect obstacles and feed the CollidingWithCardinalObstacle bool, only useful when working with grid movement, or if you need that info for some reason")]
        public bool PerformCardinalObstacleRaycastDetection = false;
		[MMReadOnly]
        [Tooltip("the current speed of the character")]
        public Vector3 Speed;
		[MMReadOnly]
        [Tooltip("the current velocity in units/second")]
        public Vector3 Velocity;
		[MMReadOnly]
        [Tooltip("the velocity of the character last frame")]
        public Vector3 VelocityLastFrame;
		[MMReadOnly]
        [Tooltip("the current acceleration")]
        public Vector3 Acceleration;
		[MMReadOnly]
        [Tooltip("whether or not the character is grounded")]
        public bool Grounded;
		[MMReadOnly]
        [Tooltip("whether or not the character got grounded this frame")]
        public bool JustGotGrounded;
		[MMReadOnly]
        [Tooltip("the current movement of the character")]
        public Vector3 CurrentMovement;
		[MMReadOnly]
        [Tooltip("the direction the character is going in")]
        public Vector3 CurrentDirection;
		[MMReadOnly]
        [Tooltip("the current friction")]
        public float Friction;
		[MMReadOnly]
        [Tooltip("the current added force, to be added to the character's movement")]
        public Vector3 AddedForce;
        [MMReadOnly]
        [Tooltip("whether or not the character is in free movement mode or not")]
        public bool FreeMovement = true;
        public virtual Vector3 ColliderCenter { get { return Vector3.zero; }  }
		public virtual Vector3 ColliderBottom { get { return Vector3.zero; }  }
		public virtual Vector3 ColliderTop { get { return Vector3.zero; }  }
		public GameObject ObjectBelow { get; set; }
		public SurfaceModifier SurfaceModifierBelow { get; set; }
        public virtual Vector3 AppliedImpact { get { return _impact; } }
        public virtual bool OnAMovingPlatform { get; set; }
        public virtual Vector3 MovingPlatformSpeed { get; set; }
        public GameObject DetectedObstacleLeft { get; set; }
        public GameObject DetectedObstacleRight { get; set; }
        public GameObject DetectedObstacleUp { get; set; }
        public GameObject DetectedObstacleDown { get; set; }
        public bool CollidingWithCardinalObstacle { get; set; }

        protected Vector3 _positionLastFrame;
		protected Vector3 _speedComputation;
		protected bool _groundedLastFrame;
        protected Vector3 _impact;		
		protected const float _smallValue=0.0001f;
		protected virtual void Awake()
		{			
			CurrentDirection = transform.forward;
		}
		protected virtual void Update()
		{
			CheckIfGrounded ();
			HandleFriction ();
			DetermineDirection ();
		}
		protected virtual void ComputeSpeed ()
		{
            if (Time.deltaTime != 0f)
            {
                Speed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
            }
            Speed.x = Mathf.Round(Speed.x * 100f) / 100f;
            Speed.y = Mathf.Round(Speed.y * 100f) / 100f;
            Speed.z = Mathf.Round(Speed.z * 100f) / 100f;
            _positionLastFrame = this.transform.position;
		}
        protected virtual void DetermineDirection()
		{
			
		}
        public virtual void DetectObstacles(float distance, Vector3 offset)
        {

        }
        protected virtual void FixedUpdate()
		{

		}
		protected virtual void LateUpdate()
        {
            ComputeSpeed();
        }
		protected virtual void CheckIfGrounded()
		{
            JustGotGrounded = (!_groundedLastFrame && Grounded);
            _groundedLastFrame = Grounded;
        }
        public virtual void Impact(Vector3 direction, float force)
        {

        }
        public virtual void SetGravityActive(bool status)
        {
            GravityActive = status;
        }
        public virtual void AddForce(Vector3 movement)
		{

		}
		public virtual void SetMovement(Vector3 movement)
		{

		}
		public virtual void MovePosition(Vector3 newPosition)
		{
			
		}
		public virtual void ResizeColliderHeight(float newHeight)
		{

		}
		public virtual void ResetColliderSize()
		{

		}
		public virtual bool CanGoBackToOriginalSize()
		{
			return true;
		}
		public virtual void CollisionsOn()
		{

		}
		public virtual void CollisionsOff()
		{

		}
        public virtual void SetKinematic(bool state)
        {

        }
        protected virtual void HandleFriction()
        {

        }
        public virtual void Reset()
        {
            _impact = Vector3.zero;
            GravityActive = true;
            Speed = Vector3.zero;
            Velocity = Vector3.zero;
            VelocityLastFrame = Vector3.zero;
            Acceleration = Vector3.zero;
            Grounded = true;
            JustGotGrounded = false;
            CurrentMovement = Vector3.zero;
            CurrentDirection = Vector3.zero;
            AddedForce = Vector3.zero;
        }
    }
}
