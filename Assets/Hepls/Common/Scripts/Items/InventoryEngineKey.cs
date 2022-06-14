using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineKey", menuName = "MoreMountains/TopDownEngine/InventoryEngineKey", order = 1)]
	[Serializable]
	public class InventoryEngineKey : InventoryItem 
	{
		public override bool Use()
		{
			base.Use();
            return true;
		}
	}
}