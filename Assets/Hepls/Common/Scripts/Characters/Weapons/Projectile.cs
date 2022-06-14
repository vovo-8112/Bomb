using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Weapons/Projectile")]
	public class Projectile : MMPoolableObject  
	{
		[Header("Movement")]
		[Tooltip("if true, the projectile will rotate at initialization towards its rotation")]
		public bool FaceDirection = true;
		[Tooltip("the speed of the object (relative to the level's speed)")]
		public float Speed=0;
		[Tooltip("the acceleration of the object over time. Starts accelerating on enable.")]
		public float Acceleration = 0;
		[Tooltip("the current direction of the object")]
		public Vector3 Direction = Vector3.left;
		[Tooltip("if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.")]
		public bool DirectionCanBeChangedBySpawner = true;
		[Tooltip("the flip factor to apply if and when the projectile is mirrored")]
		public Vector3 FlipValue = new Vector3(-1,1,1);
        [Tooltip("the flip factor to apply if and when the projectile is mirrored")]
		public bool ProjectileIsFacingRight = true;

		[Header("Spawn")]
		[MMInformation("Here you can define an initial delay (in seconds) during which this object won't take or cause damage. This delay starts when the object gets enabled. You can also define whether the projectiles should damage their owner (think rockets and the likes) or not",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("the initial delay during which the projectile can't be destroyed")]
		public float InitialInvulnerabilityDuration=0f;
		[Tooltip("should the projectile damage its owner?")]
		public bool DamageOwner = false;
        public DamageOnTouch TargetDamageOnTouch { get { return _damageOnTouch; } }

        protected Weapon _weapon;
		protected GameObject _owner;
		protected Vector3 _movement;
		protected float _initialSpeed;
		protected SpriteRenderer _spriteRenderer;
		protected DamageOnTouch _damageOnTouch;
		protected WaitForSeconds _initialInvulnerabilityDurationWFS;
		protected Collider _collider;
        protected Collider2D _collider2D;
		protected Rigidbody _rigidBody;
		protected Rigidbody2D _rigidBody2D;
		protected bool _facingRightInitially;
		protected bool _initialFlipX;
		protected Vector3 _initialLocalScale;
		protected bool _shouldMove = true;
		protected Health _health;
        protected bool _spawnerIsFacingRight;
        protected virtual void Awake ()
		{
			_facingRightInitially = ProjectileIsFacingRight;
			_initialSpeed = Speed;
			_health = GetComponent<Health> ();
			_collider = GetComponent<Collider> ();
            _collider2D = GetComponent<Collider2D>();
			_spriteRenderer = GetComponent<SpriteRenderer> ();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_rigidBody = GetComponent<Rigidbody> ();
			_rigidBody2D = GetComponent<Rigidbody2D> ();
			_initialInvulnerabilityDurationWFS = new WaitForSeconds (InitialInvulnerabilityDuration);
			if (_spriteRenderer != null) {	_initialFlipX = _spriteRenderer.flipX ;		}
			_initialLocalScale = transform.localScale;
		}
		protected virtual IEnumerator InitialInvulnerability()
		{
			if (_damageOnTouch == null) { yield break; }
			if (_weapon == null) { yield break; }

            _damageOnTouch.ClearIgnoreList();
            _damageOnTouch.IgnoreGameObject(_weapon.Owner.gameObject);
			yield return _initialInvulnerabilityDurationWFS;
			if (DamageOwner)
			{
				_damageOnTouch.StopIgnoringObject(_weapon.Owner.gameObject);
			}
		}
		protected virtual void Initialization()
		{
			Speed = _initialSpeed;
			ProjectileIsFacingRight = _facingRightInitially;
			if (_spriteRenderer != null) {	_spriteRenderer.flipX = _initialFlipX;	}
			transform.localScale = _initialLocalScale;	
			_shouldMove = true;
            _damageOnTouch?.InitializeFeedbacks();

            if (_collider != null)
            {
                _collider.enabled = true;
            }
            if (_collider2D != null)
            {
                _collider2D.enabled = true;
            }
		}
        protected virtual void FixedUpdate ()
		{
			base.Update ();
			if (_shouldMove)
			{
				Movement();
			}
		}
		public virtual void Movement()
		{
			_movement = Direction * (Speed / 10) * Time.deltaTime;
			if (_rigidBody != null)
			{
				_rigidBody.MovePosition (this.transform.position + _movement);
			}
			if (_rigidBody2D != null)
			{
				_rigidBody2D.MovePosition(this.transform.position + _movement);
			}
			Speed += Acceleration * Time.deltaTime;
		}
		public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
            _spawnerIsFacingRight = spawnerIsFacingRight;

            if (DirectionCanBeChangedBySpawner)
			{
				Direction = newDirection;
			}
			if (ProjectileIsFacingRight != spawnerIsFacingRight)
			{
				Flip ();
			}
			if (FaceDirection)
			{
				transform.rotation = newRotation;
			}
		}
		protected virtual void Flip()
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = !_spriteRenderer.flipX;
			}	
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale,FlipValue) ;
			}
		}
        protected virtual void Flip(bool state)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = state;
            }
            else
            {
                this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
            }
        }
        public virtual void SetWeapon(Weapon newWeapon)
		{
			_weapon = newWeapon;
		}
        public virtual void SetDamage(int newDamage)
        {
            if (_damageOnTouch != null)
            {
                _damageOnTouch.DamageCaused = newDamage;
            }
        }
        public virtual void SetOwner(GameObject newOwner)
        {
            _owner = newOwner;
            DamageOnTouch damageOnTouch = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
            if (damageOnTouch != null)
            {
                damageOnTouch.Owner = newOwner;
                damageOnTouch.Owner = newOwner;
                if (!DamageOwner)
                {
                    damageOnTouch.IgnoreGameObject(newOwner);
                }
            }
        }
		public virtual void StopAt()
		{
            if (_collider != null)
            {
                _collider.enabled = false;
            }
            if (_collider2D != null)
            {
                _collider2D.enabled = false;
            }
			
			_shouldMove = false;
		}
		protected virtual void OnDeath()
		{
			StopAt ();
		}
		protected override void OnEnable ()
		{
			base.OnEnable ();

			Initialization();
			if (InitialInvulnerabilityDuration>0)
			{
				StartCoroutine(InitialInvulnerability());
			}

			if (_health != null)
			{
				_health.OnDeath += OnDeath;
			}
		}
		protected override void OnDisable()
		{
			base.OnDisable ();
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
			}			
		}
	}	
}