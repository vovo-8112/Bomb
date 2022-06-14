using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Items/ExplodudesBombPicker")]
	public class ExplodudesBombPicker : PickableItem
	{
        [Header("Explodudes Bomb Picker")]
        [Tooltip("The amount of points to add when collected")]
        public int BombsToAdd = 1;

        protected CharacterHandleWeapon _characterHandleWeapon;
        protected ExplodudesWeapon _explodudesWeapon;
		protected override void Pick(GameObject picker) 
		{
            _characterHandleWeapon = picker.MMGetComponentNoAlloc<CharacterHandleWeapon>();
            if (_characterHandleWeapon == null)
            {
                return;
            }
            _explodudesWeapon = _characterHandleWeapon.CurrentWeapon.GetComponent<ExplodudesWeapon>();
            if (_explodudesWeapon == null)
            {
                return;
            }
            _explodudesWeapon.MaximumAmountOfBombsAtOnce += BombsToAdd;
            _explodudesWeapon.RemainingBombs += BombsToAdd;
		}
	}
}