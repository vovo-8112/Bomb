using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.InventoryEngine
{
	public static class InventoryEngineMenu 
	{
		const string _saveFolderName = "InventoryEngine"; 

		[MenuItem("Tools/More Mountains/Reset all saved inventories",false,31)]
		private static void ResetAllSavedInventories()
		{
			MMSaveLoadManager.DeleteSaveFolder (_saveFolderName);
			Debug.LogFormat ("Inventories Save Files Reset");
		}


	}
}