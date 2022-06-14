using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Damage/DamageOnTouch")]
    public class DamageOnTouch : MonoBehaviour
    {
        public enum KnockbackStyles { NoKnockback, AddForce }
        public enum KnockbackDirections { BasedOnOwnerPosition, BasedOnSpeed }

        [Header("Targets")]

        [MMInformation("This component will make your object cause damage to objects that collide with it. Here you can define what layers will be affected by the damage (for a standard enemy, choose Player), how much damage to give, and how much force should be applied to the object that gets the damage on hit. You can also specify how long the post-hit invincibility should last (in seconds).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the layers that will be damaged by this object")]
        public LayerMask TargetLayerMask;

        [Header("Damage Caused")]
        [Tooltip("The amount of health to remove from the player's health")]
        public int DamageCaused = 10;
        [Tooltip("the type of knockback to apply when causing damage")]
        public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;
        [Tooltip("The direction to apply the knockback ")]
        public KnockbackDirections DamageCausedKnockbackDirection;
        [Tooltip("The force to apply to the object that gets damaged")]
        public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 0);
        [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
        public float InvincibilityDuration = 0.5f;

        [Header("Damage Taken")]

        [MMInformation("After having applied the damage to whatever it collided with, you can have this object hurt itself. A bullet will explode after hitting a wall for example. Here you can define how much damage it'll take every time it hits something, or only when hitting something that's damageable, or non damageable. Note that this object will need a Health component too for this to be useful.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("The amount of damage taken every time, whether what we collide with is damageable or not")]
        public int DamageTakenEveryTime = 0;
        [Tooltip("The amount of damage taken when colliding with a damageable object")]
        public int DamageTakenDamageable = 0;
        [Tooltip("The amount of damage taken when colliding with something that is not damageable")]
        public int DamageTakenNonDamageable = 0;
        [Tooltip("the type of knockback to apply when taking damage")]
        public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;
        [Tooltip("The force to apply to the object that gets damaged")]
        public Vector3 DamageTakenKnockbackForce = Vector3.zero;
        [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
        public float DamageTakenInvincibilityDuration = 0.5f;

        [Header("Feedbacks")]
        [Tooltip("the feedback to play when hitting a Damageable")]
        public MMFeedbacks HitDamageableFeedback;
        [Tooltip("the feedback to play when hitting a non Damageable")]
        public MMFeedbacks HitNonDamageableFeedback;
        [MMReadOnly]
        [Tooltip("the owner of the DamageOnTouch zone")]
        public GameObject Owner;
        protected Vector3 _lastPosition, _lastDamagePosition, _velocity, _knockbackForce, _damageDirection;
        protected float _startTime = 0f;
        protected Health _colliderHealth;
        protected TopDownController _topDownController;
        protected TopDownController _colliderTopDownController;
        protected Rigidbody _colliderRigidBody;
        protected Health _health;
        protected List<GameObject> _ignoredGameObjects;
        protected Vector3 _knockbackForceApplied;
        protected CircleCollider2D _circleCollider2D;
        protected BoxCollider2D _boxCollider2D;
        protected SphereCollider _sphereCollider;
        protected BoxCollider _boxCollider;
        protected Color _gizmosColor;
        protected Vector3 _gizmoSize;
        protected Vector3 _gizmoOffset;
        protected Transform _gizmoTransform;
        protected bool _twoD = false;
        protected bool _initializedFeedbacks = false;
        protected virtual void Awake()
        {
            InitializeIgnoreList();
            
            _health = GetComponent<Health>();
            _topDownController = GetComponent<TopDownController>();
            _boxCollider = GetComponent<BoxCollider>();
            _sphereCollider = GetComponent<SphereCollider>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _circleCollider2D = GetComponent<CircleCollider2D>();
            _lastDamagePosition = this.transform.position;

            _twoD = ((_boxCollider2D != null) || (_circleCollider2D != null));

            _gizmosColor = Color.red;
            _gizmosColor.a = 0.25f;
            if (_boxCollider2D != null) { SetGizmoOffset(_boxCollider2D.offset); _boxCollider2D.isTrigger = true; }
            if (_boxCollider != null) { SetGizmoOffset(_boxCollider.center); _boxCollider.isTrigger = true; }
            if (_sphereCollider != null) { SetGizmoOffset(_sphereCollider.center); _sphereCollider.isTrigger = true; }
            if (_circleCollider2D != null) { SetGizmoOffset(_circleCollider2D.offset); _circleCollider2D.isTrigger = true; }

            InitializeFeedbacks();
        }

        public virtual void InitializeFeedbacks()
        {
            if (_initializedFeedbacks)
            {
                return;
            }
            HitDamageableFeedback?.Initialization(this.gameObject);
            HitNonDamageableFeedback?.Initialization(this.gameObject);
            _initializedFeedbacks = true;
        }

        public virtual void SetGizmoSize(Vector3 newGizmoSize)
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _boxCollider = GetComponent<BoxCollider>();
            _sphereCollider = GetComponent<SphereCollider>();
            _circleCollider2D = GetComponent<CircleCollider2D>();
            _gizmoSize = newGizmoSize;
        }

        public virtual void SetGizmoOffset(Vector3 newOffset)
        {
            _gizmoOffset = newOffset;
        }
        protected virtual void OnEnable()
        {
            _startTime = Time.time;
            _lastPosition = this.transform.position;
            _lastDamagePosition = this.transform.position;
        }
        protected virtual void Update()
        {
            ComputeVelocity();
        }
        protected virtual void InitializeIgnoreList()
        {
            if (_ignoredGameObjects == null)
            {
                _ignoredGameObjects = new List<GameObject>();
            }
        }
        public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
        {
            InitializeIgnoreList();
            _ignoredGameObjects.Add(newIgnoredGameObject);
        }
        public virtual void StopIgnoringObject(GameObject ignoredGameObject)
        {
            if (_ignoredGameObjects != null)
            {
                _ignoredGameObjects.Remove(ignoredGameObject);    
            }
        }
        public virtual void ClearIgnoreList()
        {
            InitializeIgnoreList();
            _ignoredGameObjects.Clear();
        }
        protected virtual void ComputeVelocity()
        {
            if (Time.deltaTime != 0f)
            {
                _velocity = (_lastPosition - (Vector3)transform.position) / Time.deltaTime;

                if (Vector3.Distance(_lastDamagePosition, this.transform.position) > 0.5f)
                {
                    _damageDirection = this.transform.position - _lastDamagePosition;
                    _lastDamagePosition = this.transform.position;
                }                

                _lastPosition = this.transform.position;
            }            
        }
        public virtual void OnTriggerStay2D(Collider2D collider)
        {
            Colliding(collider.gameObject);
        }
        public virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Colliding(collider.gameObject);
        }
        public virtual void OnTriggerStay(Collider collider)
        {
            Colliding(collider.gameObject);
        }
        public virtual void OnTriggerEnter(Collider collider)
        {
            Colliding(collider.gameObject);
        }
        protected virtual void Colliding(GameObject collider)
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }
            if (_ignoredGameObjects.Contains(collider))
            {
                return;
            }
            if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
            {

                return;
            }
            if (Time.time == 0f)
            {
                return;
            }
            _colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();
            if (_colliderHealth != null)
            {
                if (_colliderHealth.CurrentHealth > 0)
                {
                    OnCollideWithDamageable(_colliderHealth);
                }
            }
            else
            {
                OnCollideWithNonDamageable();
            }
        }
        protected virtual void OnCollideWithDamageable(Health health)
        {
            _colliderTopDownController = health.gameObject.MMGetComponentNoAlloc<TopDownController>();
            _colliderRigidBody = health.gameObject.MMGetComponentNoAlloc<Rigidbody>();

            if ((_colliderTopDownController != null) && (DamageCausedKnockbackForce != Vector3.zero) && (!_colliderHealth.Invulnerable) && (!_colliderHealth.ImmuneToKnockback))
            {
                _knockbackForce = DamageCausedKnockbackForce;

                if (_twoD)
                {
                    if (DamageCausedKnockbackDirection == KnockbackDirections.BasedOnSpeed)
                    {
                        Vector3 totalVelocity = _colliderTopDownController.Speed + _velocity;
                        _knockbackForce = Vector3.RotateTowards(DamageCausedKnockbackForce, totalVelocity.normalized, 10f, 0f);
                    }
                    if (DamageCausedKnockbackDirection == KnockbackDirections.BasedOnOwnerPosition)
                    {
                        if (Owner == null) { Owner = this.gameObject; }
                        Vector3 relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
                        _knockbackForce = Vector3.RotateTowards(DamageCausedKnockbackForce, relativePosition.normalized, 10f, 0f);
                    }    
                }
                else
                {
                    if (DamageCausedKnockbackDirection == KnockbackDirections.BasedOnSpeed)
                    {
                        Vector3 totalVelocity = _colliderTopDownController.Speed + _velocity;
                        _knockbackForce = DamageCausedKnockbackForce * totalVelocity.magnitude;
                    }
                    if (DamageCausedKnockbackDirection == KnockbackDirections.BasedOnOwnerPosition)
                    {
                        if (Owner == null) { Owner = this.gameObject; }
                        Vector3 relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
                        _knockbackForce.x = relativePosition.normalized.x * DamageCausedKnockbackForce.x;
                        _knockbackForce.y = DamageCausedKnockbackForce.y;
                        _knockbackForce.z = relativePosition.normalized.z * DamageCausedKnockbackForce.z;
                    } 
                }

                if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
                {
                    _colliderTopDownController.Impact(_knockbackForce.normalized, _knockbackForce.magnitude);
                }
            }

            HitDamageableFeedback?.PlayFeedbacks(this.transform.position);
            _colliderHealth.Damage(DamageCaused, gameObject, InvincibilityDuration, InvincibilityDuration, _damageDirection);
            if (DamageTakenEveryTime + DamageTakenDamageable > 0)
            {
                SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
            }
        }
        protected virtual void OnCollideWithNonDamageable()
        {
            if (DamageTakenEveryTime + DamageTakenNonDamageable > 0)
            {
                SelfDamage(DamageTakenEveryTime + DamageTakenNonDamageable);
            }

            HitNonDamageableFeedback?.PlayFeedbacks(this.transform.position);
        }
        protected virtual void SelfDamage(int damage)
        {
            if (_health != null)
            {
                _damageDirection = Vector3.up;
                _health.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection);
            }
            if (_topDownController != null)
            {
                Vector2 totalVelocity = _colliderTopDownController.Speed + _velocity;
                Vector2 knockbackForce = Vector3.RotateTowards(DamageTakenKnockbackForce, totalVelocity.normalized, 10f, 0f);

                if (DamageTakenKnockbackType == KnockbackStyles.AddForce)
                {
                    _topDownController.AddForce(knockbackForce);
                }
            }
        }
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = _gizmosColor;

            if (_boxCollider2D != null)
            {
                if (_boxCollider2D.enabled)
                {
                    MMDebug.DrawGizmoCube(this.transform, 
                                            _gizmoOffset,
                                            _boxCollider2D.size,
                                            false);
                }
                else
                {
                    MMDebug.DrawGizmoCube(this.transform,
                                            _gizmoOffset,
                                            _boxCollider2D.size,
                                            true);
                }                
            }

            if (_circleCollider2D != null)
            {
                if (_circleCollider2D.enabled)
                {
                    Gizmos.DrawSphere((Vector2)this.transform.position + _circleCollider2D.offset, _circleCollider2D.radius);
                }
                else
                {
                    Gizmos.DrawWireSphere((Vector2)this.transform.position + _circleCollider2D.offset, _circleCollider2D.radius);
                }
            }
            
            if (_boxCollider != null) 
            {
                if (_boxCollider.enabled)
                {
                    MMDebug.DrawGizmoCube(this.transform,
                                            _gizmoOffset,
                                            _boxCollider.size,
                                            false);
                }
                else
                {
                    MMDebug.DrawGizmoCube(this.transform,
                                            _gizmoOffset,
                                            _boxCollider.size,
                                            true);
                }
            }
            
            if (_sphereCollider != null)
            {
                if (_sphereCollider.enabled)
                {
                    Gizmos.DrawSphere(this.transform.position, _sphereCollider.radius);
                }
                else
                {
                    Gizmos.DrawWireSphere(this.transform.position, _sphereCollider.radius);
                }                
            }
        }

    }
}