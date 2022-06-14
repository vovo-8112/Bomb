using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{	
	[CustomEditor(typeof(Inventory),true)]
	public class InventoryEditor : Editor 
	{
		public Inventory InventoryTarget 
		{ 
			get 
			{ 
				return (Inventory)target;
			}
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck ();
			if (InventoryTarget.InventoryType==Inventory.InventoryTypes.Main)
			{
				Editor.DrawPropertiesExcluding(serializedObject, new string[] { "TargetChoiceInventory" });
			}
			if (InventoryTarget.InventoryType==Inventory.InventoryTypes.Equipment)
			{
				Editor.DrawPropertiesExcluding(serializedObject, new string[] {  });
			}
			if (InventoryTarget==null )
			{
				Debug.LogWarning("inventory target is null");
				return;
			}
			if (InventoryTarget.Content!=null && InventoryTarget.DrawContentInInspector)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Debug Content",EditorStyles.boldLabel);
				if (InventoryTarget.NumberOfFilledSlots>0)
				{
					for (int i = 0; i < InventoryTarget.Content.Length; i++)
					{
						GUILayout.BeginHorizontal ();
						if (!InventoryItem.IsNull(InventoryTarget.Content[i]))
						{
							EditorGUILayout.LabelField("Content["+i+"]",InventoryTarget.Content[i].Quantity.ToString()+" "+InventoryTarget.Content[i].ItemName);

							if (GUILayout.Button("Empty Slot"))
							{
								InventoryTarget.DestroyItem(i)	;
							}
						}
						else
						{
							EditorGUILayout.LabelField("Content["+i+"]","empty");
						}
						GUILayout.EndHorizontal ();
					}
				}
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Free slots",InventoryTarget.NumberOfFreeSlots.ToString());
				EditorGUILayout.LabelField("Filled slots",InventoryTarget.NumberOfFilledSlots.ToString());
				EditorGUILayout.Space();
			}
			EditorGUILayout.Space();
			if (GUILayout.Button("Empty inventory"))
			{
				InventoryTarget.EmptyInventory()	;
			}
			if (GUILayout.Button("Reset saved inventory"))
			{
				InventoryTarget.ResetSavedInventory()	;
			}
			if (EditorGUI.EndChangeCheck ()) 
			{
				serializedObject.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}
			serializedObject.ApplyModifiedProperties();
		}
		public void Update()
		 {
		     Repaint();
		 }	
	}
}