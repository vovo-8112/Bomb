using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionAimObject")]
    public class AIActionAimObject : AIAction
    {
        public enum Modes { Movement, WeaponAim }
        public enum PossibleAxis { Right, Forward }
        
        [Header("Aim Object")]
        [Tooltip("an object to aim")]
        public GameObject GameObjectToAim;
        [Tooltip("whether to aim at the AI's movement direction or the weapon aim direction")]
        public Modes Mode = Modes.Movement;
        [Tooltip("the axis to aim at the moment or weapon aim direction (usually right for 2D, forward for 3D)")]
        public PossibleAxis Axis = PossibleAxis.Right;

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
        protected override void Initialization()
        {
            _characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
            _controller = this.gameObject.GetComponentInParent<TopDownController>();
        }

        public override void PerformAction()
        {
            AimObject();
        }
        protected virtual void AimObject()
        {
            if (GameObjectToAim == null)
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
            
            switch (Axis)
            {
                case PossibleAxis.Forward:
                    GameObjectToAim.transform.forward = _newAim;
                    break;
                case PossibleAxis.Right:
                    GameObjectToAim.transform.right = _newAim;
                    break;
            }
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
