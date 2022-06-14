using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "ArmorItem", menuName = "MoreMountains/InventoryEngine/ArmorItem", order = 2)]
	[Serializable]
	public class ArmorItem : InventoryItem 
	{
		[Header("Armor")]
		public int ArmorIndex;
		public override bool Equip()
		{
			base.Equip();
			InventoryDemoGameManager.Instance.Player.SetArmor(ArmorIndex);
            return true;
        }
		public override bool UnEquip()
		{
			base.UnEquip();
			InventoryDemoGameManager.Instance.Player.SetArmor(0);
            return true;
        }		
	}
}