using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionReloadNeeded")]
    public class AIDecisionReloadNeeded : AIDecision
    {
        protected CharacterHandleWeapon _characterHandleWeapon;
        public override void Initialization()
        {
            base.Initialization();
            _characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
        }
        public override bool Decide()
        {
            if (_characterHandleWeapon == null)
            {
                return false;
            }

            if (_characterHandleWeapon.CurrentWeapon == null)
            {
                return false;
            }

            return _characterHandleWeapon.CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadNeeded;
        }
    }
}
