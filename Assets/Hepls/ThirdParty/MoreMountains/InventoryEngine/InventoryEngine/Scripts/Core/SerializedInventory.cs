using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.InventoryEngine
{
	[Serializable]
	public class SerializedInventory 
	{
		public int NumberOfRows;
		public int NumberOfColumns;
		public string InventoryName = "Inventory";
		public MoreMountains.InventoryEngine.Inventory.InventoryTypes InventoryType ;
		public bool DrawContentInInspector=false;
		public string[] ContentType;
		public int[] ContentQuantity;		
	}
}