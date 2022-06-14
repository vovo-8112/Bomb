using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionReload")]
    public class AIActionReload : AIAction
    {
        public bool OnlyReloadOnceInThisSate = true;

        protected CharacterHandleWeapon _characterHandleWeapon;
        protected bool _reloadedOnce = false;
        protected override void Initialization()
        {
            base.Initialization();
            _characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
        }
        public override void PerformAction()
        {
            if (OnlyReloadOnceInThisSate && _reloadedOnce)
            {
                return;
            }
            if (_characterHandleWeapon == null)
            {
                return;
            }
            _characterHandleWeapon.Reload();
            _reloadedOnce = true;
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _reloadedOnce = false;
        }
    }
}
