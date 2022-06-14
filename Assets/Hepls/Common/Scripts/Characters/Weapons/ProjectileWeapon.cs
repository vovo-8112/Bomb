using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Weapons/Projectile Weapon")]
    public class ProjectileWeapon : Weapon 
	{
        [MMInspectorGroup("Projectiles", true, 22)]
        [Tooltip("the offset position at which the projectile will spawn")]
        public Vector3 ProjectileSpawnOffset = Vector3.zero;
        [Tooltip("the number of projectiles to spawn per shot")]
        public int ProjectilesPerShot = 1;
        [Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
        public Vector3 Spread = Vector3.zero;
        [Tooltip("whether or not the weapon should rotate to align with the spread angle")]
        public bool RotateWeaponOnSpread = false;
        [Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
        public bool RandomSpread = true;
        [MMReadOnly]
        [Tooltip("the projectile's spawn position")]
        public Vector3 SpawnPosition = Vector3.zero;
        public MMObjectPooler ObjectPooler { get; set; }

        protected Vector3 _flippedProjectileSpawnOffset;
        protected Vector3 _randomSpreadDirection;
        protected bool _poolInitialized = false;
        protected Transform _projectileSpawnTransform;

        [MMInspectorButton("TestShoot")]
		public bool TestShootButton;
		protected virtual void TestShoot()
		{
			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				WeaponInputStart ();				
			}
			else
			{
				WeaponInputStop ();
			}
		}
		public override void Initialization()
		{
			base.Initialization();            
			_weaponAim = GetComponent<WeaponAim> ();

            if (!_poolInitialized)
            {
                if (GetComponent<MMMultipleObjectPooler>() != null)
                {
                    ObjectPooler = GetComponent<MMMultipleObjectPooler>();
                }
                if (GetComponent<MMSimpleObjectPooler>() != null)
                {
                    ObjectPooler = GetComponent<MMSimpleObjectPooler>();
                }
                if (ObjectPooler == null)
                {
                    Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
                    return;
                }
                if (FlipWeaponOnCharacterFlip)
                {
                    _flippedProjectileSpawnOffset = ProjectileSpawnOffset;
                    _flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
                }
                _poolInitialized = true;
            }
        }
		public override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();

            for (int i = 0; i < ProjectilesPerShot; i++)
            {
                SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
            }
        }
        public virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            GameObject nextGameObject = ObjectPooler.GetPooledGameObject();
            if (nextGameObject == null) { return null; }
            if (nextGameObject.GetComponent<MMPoolableObject>() == null)
            {
                throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
            }
            nextGameObject.transform.position = spawnPosition;
            if (_projectileSpawnTransform != null)
            {
                nextGameObject.transform.position = _projectileSpawnTransform.position;
            }

            Projectile projectile = nextGameObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetWeapon(this);
                if (Owner != null)
                {
                    projectile.SetOwner(Owner.gameObject);
                }
            }
            nextGameObject.gameObject.SetActive(true);

            if (projectile != null)
            {
                if (RandomSpread)
                {
                    _randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
                    _randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
                    _randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
                }
                else
                {
                    if (totalProjectiles > 1)
                    {
                        _randomSpreadDirection.x = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.x, Spread.x);
                        _randomSpreadDirection.y = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.y, Spread.y);
                        _randomSpreadDirection.z = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.z, Spread.z);
                    }
                    else
                    {
                        _randomSpreadDirection = Vector3.zero;
                    }
                }

                Quaternion spread = Quaternion.Euler(_randomSpreadDirection);

                if (Owner == null)
                {
                    projectile.SetDirection(spread * transform.forward, transform.rotation, true);
                }
                else
                {
                    if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D)
                    {
                        projectile.SetDirection(spread * transform.forward, transform.rotation, true);
                    }
                    else
                    {
                        Vector3 newDirection = (spread * transform.right) * (Flipped ? -1 : 1);
                        if (Owner.Orientation2D != null)
                        {
                            projectile.SetDirection(newDirection, transform.rotation, Owner.Orientation2D.IsFacingRight);
                        }
                        else
                        {
                            projectile.SetDirection(newDirection, transform.rotation, true);
                        }
                    }
                }                

                if (RotateWeaponOnSpread)
                {
                    this.transform.rotation = this.transform.rotation * spread;
                }
            }

            if (triggerObjectActivation)
            {
                if (nextGameObject.GetComponent<MMPoolableObject>() != null)
                {
                    nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
                }
            }
            return (nextGameObject);
        }
        public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
        {
            _projectileSpawnTransform = newSpawnTransform;
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
			DetermineSpawnPosition ();

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(SpawnPosition, 0.2f);	
		}
	}
}
