using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineHealth", menuName = "MoreMountains/TopDownEngine/InventoryEngineHealth", order = 1)]
	[Serializable]
	public class InventoryEngineHealth : InventoryItem 
	{
		[Header("Health")]
		[MMInformation("Here you need specify the amount of health gained when using this item.",MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("the amount of health to add to the player when the item is used")]
		public int HealthBonus;
		public override bool Use()
		{
			base.Use();

			if (TargetInventory.Owner == null)
			{
				return false;
			}

			Health characterHealth = TargetInventory.Owner.GetComponent<Health>();
			if (characterHealth != null)
			{
				characterHealth.GetHealth(HealthBonus,TargetInventory.gameObject);
                return true;
			}
            else
            {
                return false;
            }
		}

	}
}