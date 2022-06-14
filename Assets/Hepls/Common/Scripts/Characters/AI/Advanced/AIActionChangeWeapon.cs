using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionChangeWeapon")]
    public class AIActionChangeWeapon : AIAction
    {
        [Tooltip("The new weapon to equip when that action is performed")]
        public Weapon NewWeapon;

        protected CharacterHandleWeapon _characterHandleWeapon;
        protected int _change = 0;
        protected override void Initialization()
        {
            _characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
        }
        public override void PerformAction()
        {
            ChangeWeapon();
        }
        protected virtual void ChangeWeapon()
        {
            if (_change < 1)
            {
                if (NewWeapon == null)
                {
                    _characterHandleWeapon.ChangeWeapon(NewWeapon, "");
                }
                else
                {
                    _characterHandleWeapon.ChangeWeapon(NewWeapon, NewWeapon.name);
                }
                
                _change++;
            }
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _change = 0;
        }
    }
}
