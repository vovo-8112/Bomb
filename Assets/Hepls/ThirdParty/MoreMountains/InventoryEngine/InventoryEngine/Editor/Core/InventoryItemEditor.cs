using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;


namespace MoreMountains.InventoryEngine
{	
	[CustomEditor(typeof(InventoryItem),true)]
	public class InventoryItemEditor : Editor 
	{
		public InventoryItem ItemTarget 
		{ 
			get 
			{ 
				return (InventoryItem)target;
			}
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			List<string> excludedProperties = new List<string>();
			if (!ItemTarget.Equippable)
			{
				excludedProperties.Add("TargetEquipmentInventoryName");
				excludedProperties.Add("EquippedSound");
			}
			if (!ItemTarget.Usable)
			{
				excludedProperties.Add("UsedSound");
			}
			Editor.DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());
			serializedObject.ApplyModifiedProperties();
		}
	}
}