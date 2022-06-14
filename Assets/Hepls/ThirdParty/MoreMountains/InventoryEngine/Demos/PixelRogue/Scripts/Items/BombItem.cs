using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "BombItem", menuName = "MoreMountains/InventoryEngine/BombItem", order = 2)]
	[Serializable]
	public class BombItem : InventoryItem 
	{
		public override bool Use()
		{
			base.Use();
			Debug.LogFormat("bomb explosion");
            return true;
		}		
	}
}