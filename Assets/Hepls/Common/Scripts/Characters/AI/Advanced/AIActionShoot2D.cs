using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionShoot2D")]
    public class AIActionShoot2D : AIAction
    {
        public enum AimOrigins { Transform, SpawnPoint }
        
        [Header("Binding")]
        [Tooltip("the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;

        [Header("Behaviour")]
        [Tooltip("the origin we'll take into account when computing the aim direction towards the target")]
        public AimOrigins AimOrigin = AimOrigins.Transform;
        [Tooltip("if true, the Character will face the target (left/right) when shooting")]
        public bool FaceTarget = true;
        [Tooltip("if true the Character will aim at the target when shooting")]
        public bool AimAtTarget = false;

        protected CharacterOrientation2D _orientation2D;
        protected Character _character;
        protected WeaponAim _weaponAim;
        protected ProjectileWeapon _projectileWeapon;
        protected Vector3 _weaponAimDirection;
        protected int _numberOfShoots = 0;
        protected bool _shooting = false;
        protected override void Initialization()
        {
            _character = GetComponentInParent<Character>();
            _orientation2D = _character?.FindAbility<CharacterOrientation2D>();
            if (TargetHandleWeaponAbility == null)
            {
                TargetHandleWeaponAbility = _character?.FindAbility<CharacterHandleWeapon>();
            }
        }
        public override void PerformAction()
        {
            MakeChangesToTheWeapon();
            TestFaceTarget();
            TestAimAtTarget();
            Shoot();
        }
        protected virtual void Update()
        {
            if (TargetHandleWeaponAbility.CurrentWeapon != null)
            {
                if (_weaponAim != null)
                {
                    if (_shooting)
                    {
                        _weaponAim.SetCurrentAim(_weaponAimDirection);
                    }
                    else
                    {
                        if (_orientation2D != null)
                        {
                            if (_orientation2D.IsFacingRight)
                            {
                                _weaponAim.SetCurrentAim(Vector3.right);
                            }
                            else
                            {
                                _weaponAim.SetCurrentAim(Vector3.left);
                            }
                        }                        
                    }
                }
            }
        }
        protected virtual void MakeChangesToTheWeapon()
        {
            if (TargetHandleWeaponAbility.CurrentWeapon != null)
            {
                TargetHandleWeaponAbility.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
            }
        }
        protected virtual void TestFaceTarget()
        {
            if (!FaceTarget)
            {
                return;
            }

            if (this.transform.position.x > _brain.Target.position.x)
            {
                _orientation2D.FaceDirection(-1);
            }
            else
            {
                _orientation2D.FaceDirection(1);
            }            
        }
        protected virtual void TestAimAtTarget()
        {
            if (!AimAtTarget)
            {
                return;
            }

            if (TargetHandleWeaponAbility.CurrentWeapon != null)
            {
                if (_weaponAim == null)
                {
                    _weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
                }

                if (_weaponAim != null)
                {
                    if ((AimOrigin == AimOrigins.SpawnPoint) && (_projectileWeapon != null))
                    {
                        _projectileWeapon.DetermineSpawnPosition();
                        _weaponAimDirection = _brain.Target.position - _projectileWeapon.SpawnPosition;
                    }
                    else
                    {
                        _weaponAimDirection = _brain.Target.position - _character.transform.position;
                    }                    
                }                
            }
        }
        protected virtual void Shoot()
        {
            if (_numberOfShoots < 1)
            {
                TargetHandleWeaponAbility.ShootStart();
                _numberOfShoots++;
            }
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _numberOfShoots = 0;
            _shooting = true;
            _weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
            _projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();
        }
        public override void OnExitState()
        {
            base.OnExitState();
            if (TargetHandleWeaponAbility != null)
            {
                TargetHandleWeaponAbility.ForceStop();    
            }
            _shooting = false;
        }
    }
}
