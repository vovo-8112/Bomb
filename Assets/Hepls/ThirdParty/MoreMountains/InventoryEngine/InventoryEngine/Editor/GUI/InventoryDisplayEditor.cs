using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(InventoryDisplay),true)]
	public class InventoryDisplayEditor : Editor 
	{
		public InventoryDisplay InventoryDisplayTarget 
		{ 
			get 
			{ 
				return (InventoryDisplay)target;
			}
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck ();

			Editor.DrawPropertiesExcluding(serializedObject, new string[] {  });
			if (InventoryDisplayTarget==null )
			{
				return;
			}
			EditorGUILayout.Space();
			if (GUILayout.Button("Auto setup inventory display panel"))
			{
				InventoryDisplayTarget.SetupInventoryDisplay ();	
				SceneView.RepaintAll();
			}

			if (EditorGUI.EndChangeCheck ()) 
			{
				serializedObject.ApplyModifiedProperties();
				SceneView.RepaintAll();
			}
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
		}
	}


}