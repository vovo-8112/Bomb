using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	[CustomEditor(typeof(MMAchievementList),true)]
	public class MMAchievementListInspector : Editor 
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector ();
			MMAchievementList achievementList = (MMAchievementList)target;
			if(GUILayout.Button("Reset Achievements"))
			{
				achievementList.ResetAchievements();
			}	
			EditorUtility.SetDirty (achievementList);
		}
	}
}