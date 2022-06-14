﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEditor;

namespace MoreMountains.Tools
{	

	[CustomPropertyDrawer(typeof(MMReadOnlyAttribute))]

	public class MMReadOnlyAttributeDrawer : PropertyDrawer
	{
	    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	    {
	        return EditorGUI.GetPropertyHeight(property, label, true);
	    }
	    
		#if  UNITY_EDITOR
	    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	    {
	        GUI.enabled = false;
	        EditorGUI.PropertyField(position, property, label, true);
	        GUI.enabled = true;
	    }
	    #endif
	}
}