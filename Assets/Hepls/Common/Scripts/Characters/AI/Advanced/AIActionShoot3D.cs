using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionShoot3D")]
    public class AIActionShoot3D : AIAction
    {
        [Header("Binding")]
        [Tooltip("the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;
        
        [Header("Behaviour")]
        [Tooltip("if true the Character will aim at the target when shooting")]
        public bool AimAtTarget = true;
        [Tooltip("an offset to apply to the aim (useful to aim at the head/torso/etc automatically)")]
        public Vector3 ShootOffset;
        [Tooltip("if this is set to true, vertical aim will be locked to remain horizontal")]
        public bool LockVerticalAim = false;

        protected CharacterOrientation3D _orientation3D;
        protected Character _character;
        protected WeaponAim _weaponAim;
        protected ProjectileWeapon _projectileWeapon;
        protected Vector3 _weaponAimDirection;
        protected int _numberOfShoots = 0;
        protected bool _shooting = false;
        protected override void Initialization()
        {
            _character = GetComponentInParent<Character>();
            _orientation3D = _character?.FindAbility<CharacterOrientation3D>();
            if (TargetHandleWeaponAbility == null)
            {
                TargetHandleWeaponAbility = _character?.FindAbility<CharacterHandleWeapon>();
            }
        }
        public override void PerformAction()
        {
            MakeChangesToTheWeapon();
            TestAimAtTarget();
            Shoot();
        }
        protected virtual void MakeChangesToTheWeapon()
        {
            if (TargetHandleWeaponAbility.CurrentWeapon != null)
            {
                TargetHandleWeaponAbility.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
            }
        }
        protected virtual void Update()
        {
            if (TargetHandleWeaponAbility.CurrentWeapon != null)
            {
                if (_weaponAim != null)
                {
                    if (_shooting)
                    {
                        if (LockVerticalAim)
                        {
                            _weaponAimDirection.y = 0;
                        }

                        if (AimAtTarget)
                        {
                            _weaponAim.SetCurrentAim(_weaponAimDirection);    
                        }
                    }
                }
            }
        }
        protected virtual void TestAimAtTarget()
        {
            if (!AimAtTarget || (_brain.Target == null))
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
                    if (_projectileWeapon != null)
                    {
                        _projectileWeapon.DetermineSpawnPosition();
                        _weaponAimDirection = _brain.Target.position + ShootOffset - _projectileWeapon.SpawnPosition;
                    }
                    else
                    {
                        _weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;
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
