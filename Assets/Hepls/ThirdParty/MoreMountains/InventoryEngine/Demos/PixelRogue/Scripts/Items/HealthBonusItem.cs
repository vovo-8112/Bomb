using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "HealthBonusItem", menuName = "MoreMountains/InventoryEngine/HealthBonusItem", order = 1)]
	[Serializable]
	public class HealthBonusItem : InventoryItem 
	{
		[Header("Health Bonus")]
		public int HealthBonus;
		public override bool Use()
		{
			base.Use();
			Debug.LogFormat("increase character's health by "+HealthBonus);
            return true;
		}
		
	}
}