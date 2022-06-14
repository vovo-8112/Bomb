using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using System.Reflection;

namespace MoreMountains.InventoryEngine
{	
	[CustomEditor(typeof(ItemPicker), true)]
	public class PickableInventoryItemEditor : Editor 
	{
		protected int _targetInventoryIndex = 0;
		protected List<string> _targetInventoriesList = new List<string>();
		protected string[] _targetInventories;
		public ItemPicker ItemTarget 
		{ 
			get 
			{ 
				return (ItemPicker)target;
			}
		}
		protected virtual void OnEnable()
		{
			foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
			{
				_targetInventoriesList.Add(inventory.name);			
			}
			_targetInventories = new string[_targetInventoriesList.Count];
			int i = 0;
			foreach (string inventoryName in _targetInventoriesList)
			{
				_targetInventories[i] = inventoryName;
				i++;
			}
		}
		public override void OnInspectorGUI()
	     {
	     	DrawDefaultInspector();
			if (ItemTarget.Item!=null)
			{
				System.Type type = ItemTarget.Item.GetType();

				foreach( FieldInfo fi in type.GetFields() )
			    {
					if (fi.GetValue(ItemTarget.Item)!=null)
					{
						EditorGUILayout.LabelField(fi.Name,fi.GetValue(ItemTarget.Item).ToString());
					}
			    }
				if (_targetInventories.Length>0)
				{
					_targetInventoryIndex = EditorGUILayout.Popup(_targetInventoryIndex,_targetInventories);
					if (GUILayout.Button("Add "+ItemTarget.Item.ItemName+" to "+_targetInventories[_targetInventoryIndex]))
					{
						ItemTarget.Pick(_targetInventories[_targetInventoryIndex]);
					}
				}
	     	}			
		}
	}
}
