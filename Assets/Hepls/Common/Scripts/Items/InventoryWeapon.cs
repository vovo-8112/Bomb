using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryWeapon", menuName = "MoreMountains/TopDownEngine/InventoryWeapon", order = 2)]
	[Serializable]
    public class InventoryWeapon : InventoryItem 
	{
        public enum AutoEquipModes { NoAutoEquip, AutoEquip, AutoEquipIfEmptyHanded }
        
        [Header("Weapon")]
		[MMInformation("Here you need to bind the weapon you want to equip when picking that item.",MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("the weapon to equip")]
		public Weapon EquippableWeapon;
		[Tooltip("how to equip this weapon when picked : not equip it, automatically equip it, or only equip it if no weapon is currently equipped")]
		public AutoEquipModes AutoEquipMode = AutoEquipModes.NoAutoEquip;
		[Tooltip("the ID of the CharacterHandleWeapon you want this weapon to be equipped to")]
		public int HandleWeaponID = 1;
		public override bool Equip()
		{
			EquipWeapon (EquippableWeapon);
            return true;
		}
        public override bool UnEquip()
        {
            if (this.TargetEquipmentInventory == null)
            {
                return false;
            }

            if (this.TargetEquipmentInventory.InventoryContains(this.ItemID).Count > 0)
            {
                EquipWeapon(null);
            }

            return true;
        }
        protected virtual void EquipWeapon(Weapon newWeapon)
		{
			if (EquippableWeapon == null)
			{
				return;
			}
			if (TargetInventory.Owner == null)
			{
				return;
			}

			Character character = TargetInventory.Owner.GetComponentInParent<Character>();

			if (character == null)
			{
				return;
			}
			CharacterHandleWeapon targetHandleWeapon = null;
			CharacterHandleWeapon[] handleWeapons = character.GetComponentsInChildren<CharacterHandleWeapon>();
			foreach (CharacterHandleWeapon handleWeapon in handleWeapons)
			{
				if (handleWeapon.HandleWeaponID == HandleWeaponID)
				{
					targetHandleWeapon = handleWeapon;
				}
			}
			
			if (targetHandleWeapon != null)
            {
	            targetHandleWeapon.ChangeWeapon(newWeapon, this.ItemID);
            }
		}
	}
}
