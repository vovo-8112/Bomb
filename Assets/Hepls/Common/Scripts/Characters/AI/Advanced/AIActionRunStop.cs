using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionRunStop")]
    public class AIActionRunStop : AIAction
    {
        public bool OnlyRunOnce = true;
        
        protected Character _character;
        protected CharacterRun _characterRun;
        protected bool _alreadyRan = false;
        protected override void Initialization()
        {
            base.Initialization();
            _character = this.gameObject.GetComponentInParent<Character>();
            _characterRun = _character?.FindAbility<CharacterRun>();
        }
        public override void PerformAction()
        {
            if (OnlyRunOnce && _alreadyRan)
            {
                return;
            }
            _characterRun.RunStop();
            _alreadyRan = true;
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _alreadyRan = false;
        }
    }
}
