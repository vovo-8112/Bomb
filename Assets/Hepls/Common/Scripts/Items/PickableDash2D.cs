using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace  MoreMountains.TopDownEngine
{
    public class PickableDash2D : PickableItem
    {
        protected override bool CheckIfPickable()
        {
            _character = _collidingObject.GetComponent<Character>();

            if (_character == null)
            {
                return false;
            }
            if (_character.CharacterType != Character.CharacterTypes.Player)
            {
                return false;
            }
            return true;
        }
        protected override void Pick(GameObject picker)
        {
            _character.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterDash2D>()?.PermitAbility(true);
        }
    }
}

