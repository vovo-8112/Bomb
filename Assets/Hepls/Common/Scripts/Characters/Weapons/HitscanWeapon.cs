using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    public class HitscanWeapon : Weapon
    {
        public enum Modes { TwoD, ThreeD }

        [MMInspectorGroup("Hitscan Spawn", true, 23)]
        [Tooltip("the offset position at which the projectile will spawn")]
        public Vector3 ProjectileSpawnOffset = Vector3.zero;
        [Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
        public Vector3 Spread = Vector3.zero;
        [Tooltip("whether or not the weapon should rotate to align with the spread angle")]
        public bool RotateWeaponOnSpread = false;
        [Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
        public bool RandomSpread = true;
        [MMReadOnly]
        [Tooltip("the projectile's spawn position")]
        public Vector3 SpawnPosition = Vector3.zero;

        [MMInspectorGroup("Hitscan", true, 24)]
        [Tooltip("whether this hitscan should work in 2D or 3D")]
        public Modes Mode = Modes.ThreeD;
        [Tooltip("the layer(s) on which to hitscan ray should collide")]
        public LayerMask HitscanTargetLayers;
        [Tooltip("the maximum distance of this weapon, after that bullets will be considered lost")]
        public float HitscanMaxDistance = 100f;
        [Tooltip("the amount of damage to apply to a damageable (something with a Health component) every time there's a hit")]
        public int DamageCaused = 5;
        [Tooltip("the duration of the invincibility after a hit (to prevent insta death in the case of rapid fire)")]
        public float DamageCausedInvincibilityDuration = 0.2f;
        
        [MMInspectorGroup("Hit Damageable", true, 25)]
        [Tooltip("a MMFeedbacks to move to the position of the hit and to play when hitting something with a Health component")]
        public MMFeedbacks HitDamageable;
        [Tooltip("a particle system to move to the position of the hit and to play when hitting something with a Health component")]
        public ParticleSystem DamageableImpactParticles;
        
        [MMInspectorGroup("Hit Non Damageable", true, 26)]
        [Tooltip("a MMFeedbacks to move to the position of the hit and to play when hitting something without a Health component")]
        public MMFeedbacks HitNonDamageable;
        [Tooltip("a particle system to move to the position of the hit and to play when hitting something without a Health component")]
        public ParticleSystem NonDamageableImpactParticles;

        protected Vector3 _flippedProjectileSpawnOffset;
        protected Vector3 _randomSpreadDirection;
        protected bool _initialized = false;
        protected Transform _projectileSpawnTransform;
        public RaycastHit _hit { get; protected set; }
        public RaycastHit2D _hit2D { get; protected set; }
        public Vector3 _origin { get; protected set; }
        protected Vector3 _destination;
        protected Vector3 _direction;
        protected GameObject _hitObject = null;
        protected Vector3 _hitPoint;
        protected Health _health;
        protected Vector3 _damageDirection;

        [MMInspectorButton("TestShoot")]
		public bool TestShootButton;
        protected virtual void TestShoot()
        {
            if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
            {
                WeaponInputStart();
            }
            else
            {
                WeaponInputStop();
            }
        }
        public override void Initialization()
        {
            base.Initialization();
            _weaponAim = GetComponent<WeaponAim>();

            if (!_initialized)
            {
                if (FlipWeaponOnCharacterFlip)
                {
                    _flippedProjectileSpawnOffset = ProjectileSpawnOffset;
                    _flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
                }
                _initialized = true;
            }
        }
		public override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();
            DetermineDirection();
            SpawnProjectile(SpawnPosition, true);
            HandleDamage();
        }
        protected virtual void DetermineDirection()
        {
            if (RandomSpread)
            {
                _randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
                _randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
                _randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
            }
            else
            {
                
                _randomSpreadDirection = Vector3.zero;
            }
            
            Quaternion spread = Quaternion.Euler(_randomSpreadDirection);
            
            if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D)
            {
                _randomSpreadDirection = spread * transform.forward;
            }
            else
            {
                _randomSpreadDirection = spread * transform.right * (Flipped ? -1 : 1);
            }
            
            if (RotateWeaponOnSpread)
            {
                this.transform.rotation = this.transform.rotation * spread;
            }
        }
        public virtual void SpawnProjectile(Vector3 spawnPosition, bool triggerObjectActivation = true)
        {
            _hitObject = null;
            if (Mode == Modes.ThreeD)
            {
                _origin = SpawnPosition;
                _hit = MMDebug.Raycast3D(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
                if (_hit.transform != null)
                {
                    _hitObject = _hit.collider.gameObject;
                    _hitPoint = _hit.point;

                }
                else
                {
                    _hitObject = null;
                }
            }
            else
            {
                _origin = SpawnPosition;
                _hit2D = MMDebug.RayCast(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
                if (_hit2D)
                {
                    _hitObject = _hit2D.collider.gameObject;
                    _hitPoint = _hit2D.point;
                }
                else
                {
                    _hitObject = null;
                }
            }      
        }
        protected virtual void HandleDamage()
        {
            if (_hitObject == null)
            {
                return;
            }

            _health = _hitObject.MMGetComponentNoAlloc<Health>();

            if (_health == null)
            {
                if (HitNonDamageable != null)
                {
                    HitNonDamageable.transform.position = _hitPoint;
                    HitNonDamageable.transform.LookAt(this.transform);
                    HitNonDamageable.PlayFeedbacks();
                }

                if (NonDamageableImpactParticles != null)
                {
                    NonDamageableImpactParticles.transform.position = _hitPoint;
                    NonDamageableImpactParticles.transform.LookAt(this.transform);
                    NonDamageableImpactParticles.Play();
                }
            }
            else
            {
                _damageDirection = (_hitObject.transform.position - this.transform.position).normalized;
                _health.Damage(DamageCaused, this.gameObject, DamageCausedInvincibilityDuration, DamageCausedInvincibilityDuration, _damageDirection);

                if (HitDamageable != null)
                {
                    HitDamageable.transform.position = _hitPoint;
                    HitDamageable.transform.LookAt(this.transform);
                    HitDamageable.PlayFeedbacks();
                }
                
                if (DamageableImpactParticles != null)
                {
                    DamageableImpactParticles.transform.position = _hitPoint;
                    DamageableImpactParticles.transform.LookAt(this.transform);
                    DamageableImpactParticles.Play();
                }
            }

        }
        public virtual void DetermineSpawnPosition()
        {
            if (Flipped)
            {
                if (FlipWeaponOnCharacterFlip)
                {
                    SpawnPosition = this.transform.position - this.transform.rotation * _flippedProjectileSpawnOffset;
                }
                else
                {
                    SpawnPosition = this.transform.position - this.transform.rotation * ProjectileSpawnOffset;
                }
            }
            else
            {
                SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
            }

            if (WeaponUseTransform != null)
            {
                SpawnPosition = WeaponUseTransform.position;
            }
        }
        protected virtual void OnDrawGizmosSelected()
        {
            DetermineSpawnPosition();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
        }

    }
}
