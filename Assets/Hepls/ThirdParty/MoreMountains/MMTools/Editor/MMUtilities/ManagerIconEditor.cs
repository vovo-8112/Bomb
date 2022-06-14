using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MoreMountains.Tools
{
	[CustomEditor(typeof(MMSceneViewIcon))]
	[InitializeOnLoad]
	public class SceneViewIconEditor : Editor 
	{

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawGameObjectName(MMSceneViewIcon sceneViewIcon, GizmoType gizmoType)
		{   
			GUIStyle style = new GUIStyle();
	        style.normal.textColor = Color.blue;	 
			Handles.Label(sceneViewIcon.transform.position, sceneViewIcon.gameObject.name,style);
		}


	}
}