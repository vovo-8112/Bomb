using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Items/Pickable Weapon")]
	public class PickableWeapon : PickableItem
	{
		[Tooltip("the new weapon the player gets when collecting this object")]
		public Weapon WeaponToGive;
		[Tooltip("the ID of the CharacterHandleWeapon ability you want this weapon to go to (1 by default)")]
		public int HandleWeaponID = 1;


        protected CharacterHandleWeapon _characterHandleWeapon;
		protected override void Pick(GameObject picker)
		{
			Character character = _collidingObject.gameObject.MMGetComponentNoAlloc<Character>();

			if (character == null)
			{
				return;
			}
			
			if (_characterHandleWeapon  != null)
			{
				_characterHandleWeapon .ChangeWeapon(WeaponToGive, null);
			}
		}
		protected override bool CheckIfPickable()
		{
			_character = _collidingObject.GetComponent<Character>();
			if ((_character == null) || (_collidingObject.GetComponent<CharacterHandleWeapon>() == null))
			{
				return false;
			}
			if (_character.CharacterType != Character.CharacterTypes.Player)
			{
				return false;
			}
			CharacterHandleWeapon[] handleWeapons = _character.GetComponentsInChildren<CharacterHandleWeapon>();
			foreach (CharacterHandleWeapon handleWeapon in handleWeapons)
			{
				if ((handleWeapon.HandleWeaponID == HandleWeaponID) && (handleWeapon.CanPickupWeapons))
				{
					_characterHandleWeapon = handleWeapon;
				}
			}

			if (_characterHandleWeapon == null)
			{
				return false;
			}
			return true;
		}
	}
}