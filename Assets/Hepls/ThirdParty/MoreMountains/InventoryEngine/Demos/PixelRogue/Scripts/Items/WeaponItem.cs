using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "WeaponItem", menuName = "MoreMountains/InventoryEngine/WeaponItem", order = 2)]
	[Serializable]
	public class WeaponItem : InventoryItem 
	{
		[Header("Weapon")]
		public Sprite WeaponSprite;
		public override bool Equip()
		{
			base.Equip();
			InventoryDemoGameManager.Instance.Player.SetWeapon(WeaponSprite,this);
            return true;
		}
		public override bool UnEquip()
		{
			base.UnEquip();
			InventoryDemoGameManager.Instance.Player.SetWeapon(null,this);
            return true;
        }
		
	}
}