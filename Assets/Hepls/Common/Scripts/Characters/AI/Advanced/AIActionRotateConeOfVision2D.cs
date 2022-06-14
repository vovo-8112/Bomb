using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionRotateConeOfVision2D")]
    public class AIActionRotateConeOfVision2D : AIAction
    {
        public enum Modes { Movement, WeaponAim }
        
        [Header("Aim")]
        [Tooltip("whether to aim at the AI's movement direction or the weapon aim direction")]
        public Modes Mode = Modes.Movement;

        [Header("Interpolation")]
        [Tooltip("whether or not to interpolate the rotation")]
        public bool Interpolate = false;
        [Tooltip("the rate at which to interpolate the rotation")]
        [MMCondition("Interpolate", true)] 
        public float InterpolateRate = 5f;
        
        protected CharacterHandleWeapon _characterHandleWeapon;
        protected WeaponAim _weaponAim;
        protected TopDownController _controller;
        protected Vector3 _newAim;
        protected MMConeOfVision2D _coneOfVision2D;
        protected float _angle;
        protected Vector3 _eulerAngles = Vector3.zero;
        protected override void Initialization()
        {
            _characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
            _controller = this.gameObject.GetComponentInParent<TopDownController>();
            _coneOfVision2D = this.gameObject.GetComponent<MMConeOfVision2D>();
        }

        public override void PerformAction()
        {
            AimCone();
        }
        protected virtual void AimCone()
        {
            if (_coneOfVision2D == null)
            {
                return;
            }
            
            switch (Mode )
            {
                case Modes.Movement:
                    AimAt(_controller.CurrentDirection.normalized);
                    break;
                case Modes.WeaponAim:
                    if (_weaponAim == null)
                    {
                        GrabWeaponAim();
                    }
                    else
                    {
                        AimAt(_weaponAim.CurrentAim.normalized);    
                    }
                    break;
            }
        }
        protected virtual void AimAt(Vector3 direction)
        {
            if (Interpolate)
            {
                _newAim = MMMaths.Lerp(_newAim, direction, InterpolateRate, Time.deltaTime);
            }
            else
            {
                _newAim = direction;
            }

            _angle = MMMaths.AngleBetween(this.transform.right, _newAim);
            _eulerAngles.y = -_angle;
            
            _coneOfVision2D.SetDirectionAndAngles(_newAim, _eulerAngles);
        }
        protected virtual void GrabWeaponAim()
        {
            if ((_characterHandleWeapon != null) && (_characterHandleWeapon.CurrentWeapon != null))
            {
                _weaponAim = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
            }            
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            GrabWeaponAim();
        }
    }
}
